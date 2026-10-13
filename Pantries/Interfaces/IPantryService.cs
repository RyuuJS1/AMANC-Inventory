using System.Collections.Generic;
using System.Threading.Tasks;
using AMANC_Inventory.Pantries.Models;

namespace AMANC_Inventory.Pantries.Interfaces
{
    public interface IPantryService
    {
        Task<List<PantryModel>> GetAllPantriesAsync();
        Task<bool> SavePantryAsync(PantryModel pantry);
    }
}