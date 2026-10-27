using System;
using System.Collections.Generic;
using System.Linq;

namespace AMANC_Inventory.Pantries.Models
{
    public class PantryModel
    {
        public string Id { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; } = DateTime.Now;
        public string DeliveredBy { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string SignatureBase64 { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public List<PantryItemModel> Items { get; set; } = new();
        public decimal TotalAmount => Items?.Sum(i => i.Subtotal) ?? 0;
    }
}