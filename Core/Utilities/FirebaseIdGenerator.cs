using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;

namespace AMANC_Inventory.Shared.Helpers
{
    public static class FirebaseIdGenerator
    {
        /// <summary>
        /// Genera el siguiente ID secuencial con prefijo (ej. PROD-0001, DON-0001, PAC-0001, VIA-0001)
        /// evitando que Firebase convierta el nodo en un arreglo JSON.
        /// </summary>
        public static async Task<string> GetNextCustomIdAsync<T>(FirebaseClient dbClient, string nodeName, string prefix)
        {
            try
            {
                var items = await dbClient
                    .Child(nodeName)
                    .OnceAsync<T>();

                if (!items.Any()) return $"{prefix}0001";

                int maxId = items
                    .Select(i =>
                    {
                        string key = i.Key.Replace(prefix, "");
                        return int.TryParse(key, out int num) ? num : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();

                return $"{prefix}{(maxId + 1):D4}";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al calcular ID para {nodeName}: {ex.Message}");
                return $"{prefix}0001";
            }
        }
    }
}