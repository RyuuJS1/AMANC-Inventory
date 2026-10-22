namespace AMANC_Inventory.Pantries.Models
{
    public class PantryItemModel
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => Quantity * UnitPrice;
        public string FormattedDetail => $"{ProductName} ({Unit}) — {Quantity} x {UnitPrice:C} = {Subtotal:C}";
    }
}