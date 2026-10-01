using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AMANC_Inventory.Core.Config;
using AMANC_Inventory.Core.Utilities;
using AMANC_Inventory.Trips.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace AMANC_Inventory.Trips.Services
{
    public interface ITripService
    {
        Task<List<TripModel>> GetAllTripsAsync();
        Task<bool> SaveTripAsync(TripModel trip);
    }

    public class TripService : ITripService
    {
        private readonly FirebaseClient _firebaseClient;
        private const string NodeName = "Trips";
        private const string IdPrefix = "VIA-";

        public TripService()
        {
            _firebaseClient = new FirebaseClient(FirebaseConfig.RealtimeDbUrl);
        }

        public async Task<List<TripModel>> GetAllTripsAsync()
        {
            return await PerformanceTracker.MeasureAsync(async () =>
            {
                try
                {
                    var items = await _firebaseClient
                        .Child(NodeName)
                        .OnceAsync<TripModel>();

                    return items
                        .Where(item => item.Object != null)
                        .Select(item =>
                        {
                            var trip = item.Object;
                            trip.Id = string.IsNullOrEmpty(trip.Id) ? item.Key : trip.Id;
                            return trip;
                        })
                        .ToList();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Error al consultar viajes]: {ex.Message}");
                    return new List<TripModel>();
                }
            }, "Consultar Viajes desde Firebase");
        }

        public async Task<bool> SaveTripAsync(TripModel trip)
        {
            return await PerformanceTracker.MeasureAsync(async () =>
            {
                try
                {
                    if (trip == null) return false;

                    // Si es un nuevo viaje o no tiene un ID asignado, se genera el consecutivo
                    if (string.IsNullOrWhiteSpace(trip.Id) || trip.Id == "0")
                    {
                        trip.Id = await GetNextCustomIdAsync();
                    }

                    // Guardado mediante PutAsync para usar la clave personalizada (VIA-000001)
                    await _firebaseClient
                        .Child(NodeName)
                        .Child(trip.Id)
                        .PutAsync(trip);

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Error al guardar viaje]: {ex.Message}");
                    return false;
                }
            }, "Guardar Viaje en Firebase");
        }

        /// <summary>
        /// Genera el siguiente ID consecutivo dinámico (ej. VIA-000001).
        /// Admite más de 999,999 registros ya que se expande automáticamente si el número crece.
        /// </summary>
        private async Task<string> GetNextCustomIdAsync()
        {
            try
            {
                var items = await _firebaseClient
                    .Child(NodeName)
                    .OnceAsync<TripModel>();

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
                Debug.WriteLine($"[Error al calcular el consecutivo del viaje]: {ex.Message}");
                return $"{IdPrefix}000001";
            }
        }
    }
}