namespace AMANC_Inventory.Inventory.Models
{
    public class InventoryItemModel
    {
        public string Id { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Stock { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}