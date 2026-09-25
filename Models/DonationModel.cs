using System;
using System.Collections.Generic;
using System.Text;

namespace AMANC_Inventory.Models
{
    public class DonationModel
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public DateTime DonationDate { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Presentation { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Cost => UnitPrice * Quantity;
        public decimal TotalSpent { get; set; }
    }
}
