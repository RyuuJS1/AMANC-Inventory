using System;

namespace AMANC_Inventory.Donations.Models
{
    public class DonationModel
    {
        public string Id { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateTime DonationDate { get; set; } = DateTime.Now;
        public string ProductName { get; set; } = string.Empty;
        public string Presentation { get; set; } = string.Empty;
        public string Branch { get; set;  } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalSpent => Quantity * UnitPrice;
    }
}