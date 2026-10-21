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
using AMANC_Inventory.Inventory.Services;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;

namespace AMANC_Inventory.Donations.Services
{
    public class DonationService : IDonationService
    {
        private readonly FirebaseClient _dbClient;
        private readonly ProductService _productService;
        private const string NodeName = "Donations";

        public DonationService()
        {
            _dbClient = new FirebaseClient(FirebaseConfig.RealtimeDbUrl);
            _productService = new ProductService();
        }

        public async Task<List<DonationModel>> GetAllDonationsAsync()
        {
            return await PerformanceTracker.MeasureAsync(async () =>
            {
                try
                {
                    var token = await _dbClient
                        .Child(NodeName)
                        .OnceSingleAsync<JToken>();

                    if (token == null || token.Type == JTokenType.Null)
                        return new List<DonationModel>();

                    var donationList = new List<DonationModel>();

                    if (token is JArray jArray)
                    {
                        foreach (var item in jArray)
                        {
                            if (item != null && item.Type != JTokenType.Null)
                            {
                                var donation = item.ToObject<DonationModel>();
                                if (donation != null) donationList.Add(donation);
                            }
                        }
                    }
                    else if (token is JObject jObject)
                    {
                        foreach (var prop in jObject.Properties())
                        {
                            if (prop.Value != null && prop.Value.Type != JTokenType.Null)
                            {
                                var donation = prop.Value.ToObject<DonationModel>();
                                if (donation != null)
                                {
                                    if (string.IsNullOrEmpty(donation.Id)) donation.Id = prop.Name;
                                    donationList.Add(donation);
                                }
                            }
                        }
                    }

                    return donationList;
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

                    if (string.IsNullOrWhiteSpace(donation.Id) || donation.Id == "0")
                    {
                        donation.Id = await GetNextCustomIdAsync();
                    }

                    // 1. Guardar la Donación
                    await _dbClient
                        .Child(NodeName)
                        .Child(donation.Id)
                        .PutAsync(donation);

                    // 2. ACTUALIZAR PRODUCTOS EXISTENTES EN EL INVENTARIO
                    if (donation.Items != null && donation.Items.Any())
                    {
                        var allProducts = await _productService.GetAllProductsAsync();

                        foreach (var item in donation.Items)
                        {
                            var existingProduct = allProducts.FirstOrDefault(p => p.Id == item.ProductId);
                            if (existingProduct != null)
                            {
                                existingProduct.Stock += item.Quantity; // Sumar la cantidad donada al stock
                                existingProduct.IsDonated = true;       // Marcar que es donado
                                existingProduct.Location = $"Donante: {donation.CompanyName}";
                                existingProduct.UpdatedAt = DateTime.UtcNow;

                                await _productService.SaveProductAsync(existingProduct);
                            }
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Error al guardar donación]: {ex.Message}");
                    return false;
                }
            }, "Guardar Donación en Firebase");
        }

        private async Task<string> GetNextCustomIdAsync()
        {
            try
            {
                var donations = await GetAllDonationsAsync();
                if (!donations.Any()) return "1";

                int maxId = donations
                    .Select(d =>
                    {
                        string numericPart = Regex.Match(d.Id ?? "0", @"\d+").Value;
                        return int.TryParse(numericPart, out int num) ? num : 0;
                    })
                    .DefaultIfEmpty(0)
                    .Max();

                return (maxId + 1).ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error consecutivo donación]: {ex.Message}");
                return "1";
            }
        }
    }
}