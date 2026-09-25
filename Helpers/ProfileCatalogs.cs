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
                // --- Operaciones e Inventario ---
                "Almacén y Control de Insumos",
                "Logística y Transporte",

                // --- Atención a Familias y Albergue ---
                "Trabajo Social y Hospedaje",
                "Comedor y Nutrición",

                // --- Procuración y Donaciones ---
                "Procuración de Fondos y Donaciones",
                "Comunicación y Campañas (Reciclaje / Eventos)",

                // --- Gestión y Sistema ---
                "Administración y Recursos Humanos",
                "Sistemas y Tecnologías de la Información",

                // --- Personal de Apoyo ---
                "Servicio Social / Apoyo Operativo"
            };
        }

        public static ObservableCollection<string> GetRelationships()
        {
            return new ObservableCollection<string>
            {
                "Padre / Madre",
                "Cónyuge / Pareja",
                "Hijo",
                "Hermano",
                "Familiar",
                "Amigo",
                "Tutor Legal",
                "Otro"
            };
        }
    }
}