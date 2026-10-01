using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AMANC_Inventory.Core.Config;
using AMANC_Inventory.Core.Utilities;
using AMANC_Inventory.Trips.Models;
using Firebase.Database;

namespace AMANC_Inventory.Trips.Services
{
    public interface ITripService
    {
        Task<List<TripModel>> GetAllTripsAsync();
    }

    public class TripService : ITripService
    {
        private readonly FirebaseClient _firebaseClient = new(FirebaseConfig.RealtimeDbUrl);

        public async Task<List<TripModel>> GetAllTripsAsync()
        {
            return await PerformanceTracker.MeasureAsync(async () =>
            {
                try
                {
                    var items = await _firebaseClient
                        .Child("Trips")
                        .OnceAsync<TripModel>();

                    return items.Select(item =>
                    {
                        var trip = item.Object;
                        trip.Id = string.IsNullOrEmpty(trip.Id) ? item.Key : trip.Id;
                        return trip;
                    }).ToList();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Error al consultar viajes]: {ex.Message}");
                    return new List<TripModel>();
                }
            }, "Consultar Viajes desde Firebase");
        }
    }
}