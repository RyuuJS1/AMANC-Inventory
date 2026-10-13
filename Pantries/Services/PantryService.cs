using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AMANC_Inventory.Core.Config;
using AMANC_Inventory.Core.Utilities;
using AMANC_Inventory.Pantries.Interfaces;
using AMANC_Inventory.Pantries.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace AMANC_Inventory.Pantries.Services
{
    public class PantryService : IPantryService
    {
        private readonly FirebaseClient _firebaseClient;
        private const string NodeName = "Despensas";

        public PantryService()
        {
            _firebaseClient = new FirebaseClient(FirebaseConfig.RealtimeDbUrl);
        }

        public async Task<List<PantryModel>> GetAllPantriesAsync()
        {
            return await PerformanceTracker.MeasureAsync(async () =>
            {
                try
                {
                    var items = await _firebaseClient
                        .Child(NodeName)
                        .OnceAsync<PantryModel>();

                    return items
                        .Where(item => item.Object != null)
                        .Select(item =>
                        {
                            var pantry = item.Object;
                            pantry.Id = string.IsNullOrEmpty(pantry.Id) ? item.Key : pantry.Id;
                            return pantry;
                        })
                        .ToList();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Error al consultar despensas]: {ex.Message}");
                    return new List<PantryModel>();
                }
            }, "Consultar Despensas desde Firebase");
        }

        public async Task<bool> SavePantryAsync(PantryModel pantry)
        {
            return await PerformanceTracker.MeasureAsync(async () =>
            {
                try
                {
                    if (pantry == null) return false;

                    if (string.IsNullOrWhiteSpace(pantry.Id) || pantry.Id == "0")
                    {
                        pantry.Id = await GetNextCustomIdAsync();
                    }

                    await _firebaseClient
                        .Child(NodeName)
                        .Child(pantry.Id)
                        .PutAsync(pantry);

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Error al guardar despensa]: {ex.Message}");
                    return false;
                }
            }, "Guardar Despensa en Firebase");
        }

        private async Task<string> GetNextCustomIdAsync()
        {
            try
            {
                var items = await _firebaseClient
                    .Child(NodeName)
                    .OnceAsync<PantryModel>();

                if (!items.Any()) return "1";

                int maxId = items
                    .Select(i =>
                    {
                        string key = i.Key;
                        string numericPart = Regex.Match(key, @"\d+").Value;
                        return int.TryParse(numericPart, out int num) ? num : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();

                int nextNumericId = maxId + 1;
                return nextNumericId.ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error al calcular el consecutivo de despensa]: {ex.Message}");
                return "1";
            }
        }
    }
}