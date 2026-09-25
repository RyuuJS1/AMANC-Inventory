using System.Collections.ObjectModel;

namespace AMANC_Inventory.Helpers
{
    public static class ProfileCatalogs
    {
        public static ObservableCollection<string> GetBranches()
        {
            return new ObservableCollection<string>
            {
                // --- ZONA NORTE ---
                "Chihuahua",
                "Durango",
                "San Luis Potosí",
                "Sinaloa",
                "Sonora",
                "Zacatecas",

                // --- ZONA CENTRO ---
                "Aguascalientes",
                "Ciudad de México (Sede Central)",
                "Colima",
                "Estado de México",
                "Guanajuato",
                "Hidalgo",
                "Michoacán",
                "Nayarit",
                "Puebla",
                "Querétaro",
                "Tlaxcala",

                // --- ZONA SUR / SURESTE ---
                "Campeche",
                "Chiapas",
                "Quintana Roo",
                "Tabasco",
                "Veracruz",
                "Yucatán"
            };
        }

        public static ObservableCollection<string> GetDepartments()
        {
            return new ObservableCollection<string>
            {
                // --- Áreas Operativas / Almacén ---
                "Almacén e Inventario de Insumos",
                "Logística y Transporte",

                // --- Atención Psicosocial y Salud ---
                "Trabajo Social y Estancia",
                "Psicología y Acompañamiento",
                "Nutrición y Comedor",
                "Ludoteca y Actividades Recreativas",

                // --- Procuración de Fondos y Comunicación ---
                "Procuración de Fondos y Eventos",
                "Reciclaje y Campañas (Taparroscas/PET)",
                "Comunicación, Diseño y Redes Sociales",

                // --- Administración y Servicios ---
                "Administración y TI",
                "Voluntariado General / Participación Comunitaria",
                "Servicio Social / Prácticas"
            };
        }
    }
}