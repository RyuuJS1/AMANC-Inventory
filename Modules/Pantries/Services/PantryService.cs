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
using Newtonsoft.Json.Linq;

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
                    var token = await _firebaseClient
                        .Child(NodeName)
                        .OnceSingleAsync<JToken>();

                    if (token == null || token.Type == JTokenType.Null)
                        return new List<PantryModel>();

                    var pantryList = new List<PantryModel>();

                    if (token is JArray jArray)
                    {
                        foreach (var item in jArray)
                        {
                            if (item != null && item.Type != JTokenType.Null)
                            {
                                var pantry = item.ToObject<PantryModel>();
                                if (pantry != null)
                                {
                                    pantryList.Add(pantry);
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
                                var pantry = prop.Value.ToObject<PantryModel>();
                                if (pantry != null)
                                {
                                    if (string.IsNullOrEmpty(pantry.Id)) pantry.Id = prop.Name;
                                    pantryList.Add(pantry);
                                }
                            }
                        }
                    }

                    return pantryList;
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

                    if (string.IsNullOrWhiteSpace(pantry.Id) || pantry.Id == "0" || Guid.TryParse(pantry.Id, out _))
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
                var pantries = await GetAllPantriesAsync();

                if (!pantries.Any()) return "1";

                int maxId = pantries
                    .Select(p =>
                    {
                        string idToParse = !string.IsNullOrWhiteSpace(p.Id) ? p.Id : "0";
                        string numericPart = Regex.Match(idToParse, @"\d+").Value;
                        return int.TryParse(numericPart, out int num) ? num : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();

                return (maxId + 1).ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error al calcular el consecutivo de despensa]: {ex.Message}");
                return "1";
            }
        }
    }
}