using System;

namespace AMANC_Inventory.Models
{
    public class UserModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Propiedades de Login/Roles
        public string Role { get; set; } = string.Empty;
        public DateTime? LastLogin { get; set; }

        // Datos de Perfil Extra
        public string? ProfilePictureBase64 { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string EmergencyPhone { get; set; } = string.Empty;
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactRelationship { get; set; } = string.Empty; // <-- Agregado
        public string Branch { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }

        // Logística y Preferencias
        public string Availability { get; set; } = string.Empty; // <-- Agregado
        public string Skills { get; set; } = string.Empty;       // <-- Agregado
        public string ShirtSize { get; set; } = string.Empty;    // <-- Agregado

        public bool IsProfileComplete { get; set; } = false;

        public string UserInitials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name)) return "U";

                var parts = Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // Si solo tiene un nombre: "Kevin" -> "K"
                if (parts.Length == 1) return parts[0][0].ToString().ToUpper();

                // Toma la 1ra letra del Nombre + 1ra letra del Primer Apellido: "Kevin Ramos Alarcón" -> "KR"
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            }
        }
    }
}