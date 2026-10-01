using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AMANC_Inventory.Core.Config;
using AMANC_Inventory.Core.Utilities;
using AMANC_Inventory.Donations.Interfaces;
using AMANC_Inventory.Donations.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace AMANC_Inventory.Donations.Services
{
    public class DonationService : IDonationService
    {
        private readonly FirebaseClient _firebaseClient;

        public DonationService()
        {
            _firebaseClient = new FirebaseClient(FirebaseConfig.RealtimeDbUrl);
        }

        public async Task<List<DonationModel>> GetAllDonationsAsync()
        {
            return await PerformanceTracker.MeasureAsync(async () =>
            {
                try
                {
                    var items = await _firebaseClient
                        .Child("Donations")
                        .OnceAsync<DonationModel>();

                    return items.Select(item =>
                    {
                        var donation = item.Object;
                        donation.Id = string.IsNullOrEmpty(donation.Id) ? item.Key : donation.Id;
                        return donation;
                    }).ToList();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Error al consultar donaciones]: {ex.Message}");
                    return new List<DonationModel>();
                }
            }, "Consultar Donaciones desde Firebase");
        }

        public async Task<bool> SaveDonationAsync(DonationModel donation)
        {
            return await PerformanceTracker.MeasureAsync(async () =>
            {
                try
                {
                    if (string.IsNullOrEmpty(donation.Id))
                    {
                        var result = await _firebaseClient
                            .Child("Donations")
                            .PostAsync(donation);

                        donation.Id = result.Key;
                        await _firebaseClient
                            .Child("Donations")
                            .Child(result.Key)
                            .PutAsync(donation);
                    }
                    else
                    {
                        await _firebaseClient
                            .Child("Donations")
                            .Child(donation.Id)
                            .PutAsync(donation);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Error al guardar donación]: {ex.Message}");
                    return false;
                }
            }, "Guardar Donación en Firebase");
        }
    }
}