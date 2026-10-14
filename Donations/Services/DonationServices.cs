using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
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
        private const string NodeName = "Donations";

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
                        .Child(NodeName)
                        .OnceAsync<DonationModel>();

                    return items
                        .Where(item => item.Object != null)
                        .Select(item =>
                        {
                            var donation = item.Object;
                            donation.Id = string.IsNullOrEmpty(donation.Id) ? item.Key : donation.Id;
                            return donation;
                        })
                        .ToList();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Error al consultar donaciones]: {ex.Message}");
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
                    if (donation == null) return false;

                    // Si es una nueva donación o no tiene un ID asignado, se genera el consecutivo
                    if (string.IsNullOrWhiteSpace(donation.Id) || donation.Id == "0")
                    {
                        donation.Id = await GetNextCustomIdAsync();
                    }

                    // Guardado mediante PutAsync para evitar claves alfanuméricas aleatorias
                    await _firebaseClient
                        .Child(NodeName)
                        .Child(donation.Id)
                        .PutAsync(donation);

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Error al guardar donación]: {ex.Message}");
                    return false;
                }
            }, "Guardar Donación en Firebase");
        }

        /// <summary>
        /// Genera el siguiente ID consecutivo numérico simple (1, 2, 3...).
        /// Extrae el número mayor ignorando prefijos antiguos si existieran en la base de datos.
        /// </summary>
        private async Task<string> GetNextCustomIdAsync()
        {
            try
            {
                var items = await _firebaseClient
                    .Child(NodeName)
                    .OnceAsync<DonationModel>();

                if (!items.Any()) return "1";

                int maxId = items
                    .Select(i =>
                    {
                        string idToParse = !string.IsNullOrWhiteSpace(i.Object?.Id) ? i.Object.Id : i.Key;
                        // Extrae únicamente los dígitos numéricos
                        string numericPart = Regex.Match(idToParse ?? string.Empty, @"\d+").Value;
                        return int.TryParse(numericPart, out int num) ? num : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();

                return (maxId + 1).ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error al calcular el consecutivo de donación]: {ex.Message}");
                return "1";
            }
        }
    }
}