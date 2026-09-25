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

        public static ObservableCollection<string> GetSize()
        {
            return new ObservableCollection<string>
            {
                "XXXCH",
                "XXCH",
                "XCH",
                "S",
                "M",
                "G",
                "XG",
                "XXG",
                "XXXG",
            };
        }

        public static ObservableCollection<string> GetHours()
        {
            var hours = new ObservableCollection<string>();
            for (int i = 1; i <= 24; i++)
            {
                hours.Add($"{i:D2}:00");
            }
            return hours;
        }

        public static ObservableCollection<string> GetAreasOfInterest()
        {
            return new ObservableCollection<string>
            {
                "Eventos y Logística",
                "Administración y Oficina",
                "Acompañamiento a Pacientes",
                "Recreación y Terapia",
                "Colectas y Procuración de Fondos",
                "Difusión y Redes Sociales",
                "Apoyo General"
            };
        }
    }
}