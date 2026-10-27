namespace AMANC_Inventory.Inventory.Models
{
    /// <summary>
    /// Representa un insumo o producto dentro del sistema de inventario de AMANC.
    /// </summary>
    public class ProductModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();     // Identificador único del registro en base de datos (ej. "3fa85f64-5717-4562-b3fc-2c963f66afa6").
        public string SKU { get; set; } = string.Empty;                 // Código de inventario interno del producto (ej. "MED-PAR-500", "HIG-PAÑ-G").
        public string? Barcode { get; set; }                            // Código de barras comercial del empaque (ej. "750123456789", "750987654321").
        public string BranchId { get; set; } = string.Empty;            // Sede de AMANC donde reside el insumo (ej. "Veracruz", "CDMX").
        public string Name { get; set; } = string.Empty;                // Nombre descriptivo del producto (ej. "Paracetamol 500mg", "Pañales Etapa 4").
        public string Category { get; set; } = string.Empty;            // Categoría del insumo (ej. "Medicamentos", "Higiene Personal").
        public int Stock { get; set; }                                  // Cantidad física disponible en almacén (ej. 20, 100).
        public int MinStock { get; set; }                               // Umbral mínimo para alertas de reabastecimiento (ej. 5, 10).
        public string Unit { get; set; } = string.Empty;                // Unidad de medida del insumo (ej. "Pza", "Caja").
        public decimal EstimatedValue { get; set; }                     // Valor económico unitario estimado (ej. 150.00, 45.50).
        public bool IsDonated { get; set; } = true;                     // Indica si ingresó por donación o compra (ej. true, false).
        public string Location { get; set; } = string.Empty;            // Ubicación física dentro del inmueble (ej. "Estante A-2", "Refrigerador 1").
        public string Status { get; set; } = "Active";                  // Estado operativo del registro (ej. "Active", "Inactive").
        public DateTime? ExpirationDate { get; set; }                   // Fecha de caducidad del insumo (ej. "2026-12-31", "2027-05-15").
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;      // Fecha y hora de creación del registro (ej. "2026-10-09T14:30:00Z", "2026-01-15T08:00:00Z").
        public DateTime? UpdatedAt { get; set; }                        // Fecha y hora de última actualización del registro (ej. "2026-10-09T16:00:00Z").
    }
}