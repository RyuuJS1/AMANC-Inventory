using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AMANC_Inventory.Inventory.Models;

namespace AMANC_Inventory.Inventory.Interfaces
{
    public interface IProductService
    {
        Task<bool> SaveProductAsync(InventoryItemModel product);
        Task<List<InventoryItemModel>> GetAllProductsAsync();
    }
}