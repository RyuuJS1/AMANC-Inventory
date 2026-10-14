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

namespace AMANC_Inventory.Inventory.Services
{
    public class ProductService : IProductService
    {
        private readonly FirebaseClient _firebaseClient;
        private const string NodeName = "Productos";

        public ProductService()
        {
            _firebaseClient = new FirebaseClient(FirebaseConfig.RealtimeDbUrl);
        }

        public async Task<bool> SaveProductAsync(ProductModel product)
        {
            return await PerformanceTracker.MeasureAsync(async () =>
            {
                try
                {
                    if (product == null) return false;

                    // Si no tiene ID, es "0" o contiene un GUID, asigna el número consecutivo (1, 2, 3...)
                    if (string.IsNullOrWhiteSpace(product.Id) || product.Id == "0" || Guid.TryParse(product.Id, out _))
                    {
                        product.Id = await GetNextCustomIdAsync();
                    }

                    await _firebaseClient
                        .Child(NodeName)
                        .Child(product.Id)
                        .PutAsync(product);

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Error al guardar producto]: {ex.Message}");
                    return false;
                }
            }, "Guardar Producto en Firebase");
        }

        public async Task<List<ProductModel>> GetAllProductsAsync()
        {
            return await PerformanceTracker.MeasureAsync(async () =>
            {
                try
                {
                    var items = await _firebaseClient
                        .Child(NodeName)
                        .OnceAsync<ProductModel>();

                    return items
                        .Where(item => item.Object != null)
                        .Select(item =>
                        {
                            var prod = item.Object;
                            prod.Id = string.IsNullOrEmpty(prod.Id) ? item.Key : prod.Id;
                            return prod;
                        })
                        .ToList();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Error al consultar productos]: {ex.Message}");
                    return new List<ProductModel>();
                }
            }, "Consultar Productos desde Firebase");
        }

        /// <summary>
        /// Genera el siguiente ID consecutivo numérico sin prefijos ni ceros a la izquierda (ej. 1, 2, 3, 4, 5...).
        /// Extrae el número más alto existente, incluso si hay registros anteriores con prefijos o ceros.
        /// </summary>
        private async Task<string> GetNextCustomIdAsync()
        {
            try
            {
                var items = await _firebaseClient
                    .Child(NodeName)
                    .OnceAsync<ProductModel>();

                if (!items.Any()) return "1";

                int maxId = items
                    .Select(i =>
                    {
                        // Toma el Id del objeto o la clave del nodo como respaldo
                        string idToParse = !string.IsNullOrWhiteSpace(i.Object?.Id) ? i.Object.Id : i.Key;
                        string numericPart = Regex.Match(idToParse ?? string.Empty, @"\d+").Value;
                        return int.TryParse(numericPart, out int num) ? num : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();

                int nextNumericId = maxId + 1;

                return nextNumericId.ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error al calcular el consecutivo del producto]: {ex.Message}");
                return "1";
            }
        }
    }
}