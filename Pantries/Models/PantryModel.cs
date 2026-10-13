using System;

namespace AMANC_Inventory.Pantries.Models
{
    public class PantryModel
    {
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// </summary>
        public string Destination { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Unit { get; set; } = "Pza";
        public DateTime DeliveryDate { get; set; } = DateTime.Now;
        public string DeliveredBy { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}