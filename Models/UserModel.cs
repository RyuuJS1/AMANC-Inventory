using AMANC_Inventory.Helpers;
using AMANC_Inventory.ViewModels;
using System;
using System.IO;
using System.Text.Json.Serialization; // O using Newtonsoft.Json; según tu librería
using System.Windows.Media.Imaging;

namespace AMANC_Inventory.Models
{
    public class UserModel : ViewModelBase
    {
        public string Id { get; set; } = string.Empty;

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    OnPropertyChanged(nameof(UserInitials));
                }
            }
        }

        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Propiedades de Login/Roles
        public string Role { get; set; } = string.Empty;
        public DateTime? LastLogin { get; set; }

        // Datos de Perfil Extra
        private string? _profilePictureBase64;
        public string? ProfilePictureBase64
        {
            get => _profilePictureBase64;
            set
            {
                if (SetProperty(ref _profilePictureBase64, value))
                {
                    OnPropertyChanged(nameof(HasProfilePicture));
                    OnPropertyChanged(nameof(HasNoProfilePicture));
                    OnPropertyChanged(nameof(ProfileImageSource));
                }
            }
        }

        public string Phone { get; set; } = string.Empty;
        public string EmergencyPhone { get; set; } = string.Empty;
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactRelationship { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }

        // Logística y Preferencias
        public string Availability { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public string ShirtSize { get; set; } = string.Empty;

        public bool IsProfileComplete { get; set; } = false;

        // --- Propiedades ignoradas para la serialización JSON (Solo para UI) ---

        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public bool HasProfilePicture => !string.IsNullOrWhiteSpace(ProfilePictureBase64);

        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public bool HasNoProfilePicture => !HasProfilePicture;

        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public BitmapImage? ProfileImageSource
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ProfilePictureBase64)) return null;
                try
                {
                    string base64Data = ProfilePictureBase64.Contains(",")
                        ? ProfilePictureBase64.Split(',')[1]
                        : ProfilePictureBase64;

                    byte[] binaryData = Convert.FromBase64String(base64Data);
                    using (MemoryStream ms = new MemoryStream(binaryData))
                    {
                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        bi.StreamSource = ms;
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.EndInit();
                        bi.Freeze(); // Necesario para renderizar en UI
                        return bi;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string UserInitials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name)) return "U";

                var parts = Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 1) return parts[0][0].ToString().ToUpper();

                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            }
        }
    }
}