using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AMANC_Inventory.Core.Config;
using AMANC_Inventory.Core.Utilities;
using AMANC_Inventory.Trips.Interfaces;
using AMANC_Inventory.Trips.Models;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;

namespace AMANC_Inventory.Trips.Services
{
    public class TripService : ITripService
    {
        private readonly FirebaseClient _firebaseClient;
        private const string NodeName = "Trips";

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
                    // Lee el token JSON crudo para procesar Objetos {} o Arreglos [] de Firebase
                    var token = await _firebaseClient
                        .Child(NodeName)
                        .OnceSingleAsync<JToken>();

                    if (token == null || token.Type == JTokenType.Null)
                        return new List<TripModel>();

                    var tripList = new List<TripModel>();

                    if (token is JArray jArray)
                    {
                        foreach (var item in jArray)
                        {
                            if (item != null && item.Type != JTokenType.Null)
                            {
                                var trip = item.ToObject<TripModel>();
                                if (trip != null)
                                {
                                    tripList.Add(trip);
                                }
                            }
                        }
                    }
                    else if (token is JObject jObject)
                    {
                        foreach (var prop in jObject.Properties())
                        {
                            if (prop.Value != null && prop.Value.Type != JTokenType.Null)
                            {
                                var trip = prop.Value.ToObject<TripModel>();
                                if (trip != null)
                                {
                                    if (string.IsNullOrEmpty(trip.Id)) trip.Id = prop.Name;
                                    tripList.Add(trip);
                                }
                            }
                        }
                    }

                    return tripList;
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

                    // Genera ID numérico consecutivo simple (1, 2, 3...)
                    if (string.IsNullOrWhiteSpace(trip.Id) || trip.Id == "0" || Guid.TryParse(trip.Id, out _))
                    {
                        trip.Id = await GetNextCustomIdAsync();
                    }

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

        private async Task<string> GetNextCustomIdAsync()
        {
            try
            {
                var trips = await GetAllTripsAsync();

                if (!trips.Any()) return "1";

                int maxId = trips
                    .Select(t =>
                    {
                        string idToParse = !string.IsNullOrWhiteSpace(t.Id) ? t.Id : "0";
                        string numericPart = Regex.Match(idToParse, @"\d+").Value;
                        return int.TryParse(numericPart, out int num) ? num : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();

                return (maxId + 1).ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error al calcular el consecutivo del viaje]: {ex.Message}");
                return "1";
            }
        }
    }
}