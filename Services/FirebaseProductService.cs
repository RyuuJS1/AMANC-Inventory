using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AMANC_Inventory.Config;
using AMANC_Inventory.Interfaces;
using AMANC_Inventory.Models;

namespace AMANC_Inventory.Services
{
    public class FirebaseProductService : IProductService
    {
        private readonly HttpClient _httpClient;

        public FirebaseProductService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(FirebaseConfig.RealtimeDbUrl)
            };
        }

        public async Task<List<InventoryItemModel>> GetAllProductsAsync()
        {
            try
            {
                // Firebase REST API lee el nodo /products.json
                var response = await _httpClient.GetAsync("products.json");

                if (!response.IsSuccessStatusCode)
                    return new List<InventoryItemModel>();

                var jsonString = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(jsonString) || jsonString == "null")
                    return new List<InventoryItemModel>();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // Firebase guarda colecciones como diccionarios Key-Value
                var dictionary = JsonSerializer.Deserialize<Dictionary<string, InventoryItemModel>>(jsonString, options);

                if (dictionary == null) return new List<InventoryItemModel>();

                var productList = new List<InventoryItemModel>();
                int contador = 1;

                foreach (var kvp in dictionary)
                {
                    var item = kvp.Value;

                    item.Id = contador;
                    contador++;

                    productList.Add(item);
                }

                return productList;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al consultar Firebase: {ex.Message}");
                return new List<InventoryItemModel>();
            }
        }

        public async Task<bool> AddProductAsync(InventoryItemModel product)
        {
            try
            {
                var json = JsonSerializer.Serialize(product);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("products.json", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}