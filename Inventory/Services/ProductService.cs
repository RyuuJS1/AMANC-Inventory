using System.Collections.Generic;
using System.Threading.Tasks;
using AMANC_Inventory.Inventory.Interfaces;
using AMANC_Inventory.Inventory.Models;

namespace AMANC_Inventory.Inventory.Services
{
    public class ProductService : IProductService
    {
        public async Task<bool> SaveProductAsync(InventoryItemModel product)
        {
            // Lógica para guardar el producto en Firebase
            await Task.Delay(100);
            return true;
        }

        public async Task<List<InventoryItemModel>> GetAllProductsAsync()
        {
            // Lógica para consultar la lista de productos en Firebase
            await Task.Delay(100);
            return new List<InventoryItemModel>();
        }
    }
}