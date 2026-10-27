using System;
using System.Collections.Generic;
using System.Linq;

namespace AMANC_Inventory.Donations.Models
{
    public class DonationItemModel
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Presentation { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount => Quantity * UnitPrice;

        public string FormattedDetail => $"{ProductName} ({Presentation}) - {Quantity} pza(s) a {UnitPrice:C} = {TotalAmount:C}";
    }

    public class DonationModel
    {
        public string Id { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string DonorType { get; set; } = "Empresa";
        public DateTime DonationDate { get; set; } = DateTime.Now;
        public string Branch { get; set; } = string.Empty;
        public string ReceiptFolio { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        // Lista de varios productos asociados a la donación
        public List<DonationItemModel> Items { get; set; } = new();

        // Totales y resúmenes para el DataGrid
        public decimal TotalDonation => Items?.Sum(i => i.TotalAmount) ?? 0;

        public string ProductsSummary => Items != null && Items.Any()
            ? string.Join(", ", Items.Select(i => i.ProductName))
            : "Sin productos";

        public string DetailsSummary => Items != null && Items.Any()
            ? string.Join(" | ", Items.Select(i => $"{i.ProductName} ({i.Presentation}): {i.Quantity} x {i.UnitPrice:C}"))
            : "Sin detalles";
    }
}