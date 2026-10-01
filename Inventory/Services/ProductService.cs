using System;
using System.Collections.Generic;
using System.Linq;
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
                    if (string.IsNullOrEmpty(product.Id) || product.Id == "0")
                    {
                        await _firebaseClient
                            .Child("Products")
                            .PostAsync(product);
                    }
                    else
                    {
                        await _firebaseClient
                            .Child("Products")
                            .Child(product.Id.ToString())
                            .PutAsync(product);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Error al guardar producto]: {ex.Message}");
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
                        .Child("Products")
                        .OnceAsync<InventoryItemModel>();

                    return items.Select(item => item.Object).ToList();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Error al consultar productos]: {ex.Message}");
                    return new List<InventoryItemModel>();
                }
            }, "Consultar Productos desde Firebase");
        }
    }
}