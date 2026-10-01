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
        private const string IdPrefix = "DON-";

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
        /// Genera el siguiente ID consecutivo dinámico (ej. DON-000001).
        /// Admite más de 999,999 registros ya que se expande automáticamente si el número crece.
        /// </summary>
        private async Task<string> GetNextCustomIdAsync()
        {
            try
            {
                var items = await _firebaseClient
                    .Child(NodeName)
                    .OnceAsync<DonationModel>();

                if (!items.Any()) return $"{IdPrefix}000001";

                int maxId = items
                    .Select(i =>
                    {
                        string key = i.Key;
                        // Extrae únicamente los dígitos de la clave
                        string numericPart = Regex.Match(key, @"\d+").Value;
                        return int.TryParse(numericPart, out int num) ? num : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();

                int nextNumericId = maxId + 1;

                // :D6 garantiza al menos 6 dígitos con ceros a la izquierda.
                return $"{IdPrefix}{nextNumericId:D6}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error al calcular el consecutivo de donación]: {ex.Message}");
                return $"{IdPrefix}000001";
            }
        }
    }
}