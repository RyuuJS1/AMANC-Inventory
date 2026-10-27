using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AMANC_Inventory.Core.Config;
using AMANC_Inventory.Core.Utilities;
using AMANC_Inventory.Inventory.Interfaces;
using AMANC_Inventory.Inventory.Models;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;

namespace AMANC_Inventory.Inventory.Services
{
    public class ProductService : IProductService
    {
        private readonly FirebaseClient _dbClient;
        private const string NodeName = "Productos";

        public ProductService()
        {
            _dbClient = new FirebaseClient(FirebaseConfig.RealtimeDbUrl);
        }

        public async Task<List<ProductModel>> GetAllProductsAsync()
        {
            return await PerformanceTracker.MeasureAsync(async () =>
            {
                try
                {
                    // Obtiene el token JSON crudo para procesar Objetos {} o Arreglos []
                    var token = await _dbClient
                        .Child(NodeName)
                        .OnceSingleAsync<JToken>();

                    if (token == null || token.Type == JTokenType.Null)
                        return new List<ProductModel>();

                    var productList = new List<ProductModel>();

                    if (token is JArray jArray)
                    {
                        // Procesa la respuesta cuando Firebase devuelve un Arreglo por usar claves 1, 2, 3...
                        foreach (var item in jArray)
                        {
                            if (item != null && item.Type != JTokenType.Null)
                            {
                                var product = item.ToObject<ProductModel>();
                                if (product != null)
                                {
                                    productList.Add(product);
                                }
                            }
                        }
                    }
                    else if (token is JObject jObject)
                    {
                        // Procesa la respuesta cuando Firebase devuelve un Objeto JSON estándar
                        foreach (var prop in jObject.Properties())
                        {
                            if (prop.Value != null && prop.Value.Type != JTokenType.Null)
                            {
                                var product = prop.Value.ToObject<ProductModel>();
                                if (product != null)
                                {
                                    if (string.IsNullOrEmpty(product.Id)) product.Id = prop.Name;
                                    productList.Add(product);
                                }
                            }
                        }
                    }

                    return productList;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error crítico en GetAllProductsAsync: {ex.Message}");
                    return new List<ProductModel>();
                }
            }, "Consultar Productos desde Firebase");
        }

        public async Task<bool> SaveProductAsync(ProductModel product)
        {
            return await PerformanceTracker.MeasureAsync(async () =>
            {
                try
                {
                    if (product == null) return false;

                    // Genera ID numérico consecutivo (1, 2, 3...)
                    if (string.IsNullOrWhiteSpace(product.Id) || product.Id == "0" || Guid.TryParse(product.Id, out _))
                    {
                        product.Id = await GetNextCustomIdAsync();
                    }

                    if (product.CreatedAt == default)
                    {
                        product.CreatedAt = DateTime.UtcNow;
                    }

                    // Guardar en Realtime Database
                    await _dbClient
                        .Child(NodeName)
                        .Child(product.Id)
                        .PutAsync(product);

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error en SaveProductAsync: {ex.Message}");
                    throw;
                }
            }, "Guardar Producto en Firebase");
        }

        private async Task<string> GetNextCustomIdAsync()
        {
            try
            {
                var products = await GetAllProductsAsync();

                if (!products.Any()) return "1";

                int maxId = products
                    .Select(p =>
                    {
                        string idToParse = !string.IsNullOrWhiteSpace(p.Id) ? p.Id : "0";
                        string numericPart = Regex.Match(idToParse, @"\d+").Value;
                        return int.TryParse(numericPart, out int num) ? num : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();

                return (maxId + 1).ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al calcular siguiente ID de producto: {ex.Message}");
                return "1";
            }
        }
    }
}