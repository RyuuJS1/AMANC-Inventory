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
        private const string IdPrefix = "PROD-";

        public ProductService()
        {
            _firebaseClient = new FirebaseClient(FirebaseConfig.RealtimeDbUrl);
        }

        public async Task<bool> SaveProductAsync(InventoryItemModel product)
        {
            return await PerformanceTracker.MeasureAsync(async () =>
            {
                try
                {
                    if (product == null) return false;

                    if (string.IsNullOrWhiteSpace(product.Id) || product.Id == "0")
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

        public async Task<List<InventoryItemModel>> GetAllProductsAsync()
        {
            return await PerformanceTracker.MeasureAsync(async () =>
            {
                try
                {
                    var items = await _firebaseClient
                        .Child(NodeName)
                        .OnceAsync<InventoryItemModel>();

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
                    return new List<InventoryItemModel>();
                }
            }, "Consultar Productos desde Firebase");
        }

        /// <summary>
        /// Genera el siguiente ID consecutivo dinámico (ej. PROD-000001).
        /// Admite más de 999,999 registros ya que se expande automáticamente si el número crece.
        /// </summary>
        private async Task<string> GetNextCustomIdAsync()
        {
            try
            {
                var items = await _firebaseClient
                    .Child(NodeName)
                    .OnceAsync<InventoryItemModel>();

                if (!items.Any()) return $"{IdPrefix}000001";

                int maxId = items
                    .Select(i =>
                    {
                        string key = i.Key;
                        string numericPart = Regex.Match(key, @"\d+").Value;
                        return int.TryParse(numericPart, out int num) ? num : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();

                int nextNumericId = maxId + 1;

                return $"{IdPrefix}{nextNumericId:D6}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error al calcular el consecutivo del producto]: {ex.Message}");
                return $"{IdPrefix}000001";
            }
        }
    }
}