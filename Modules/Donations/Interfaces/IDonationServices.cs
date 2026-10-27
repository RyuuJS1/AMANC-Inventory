using System.Collections.Generic;
using System.Threading.Tasks;
using AMANC_Inventory.Donations.Models;

namespace AMANC_Inventory.Donations.Interfaces
{
    public interface IDonationService
    {
        Task<List<DonationModel>> GetAllDonationsAsync();
        Task<bool> SaveDonationAsync(DonationModel donation);
    }
}