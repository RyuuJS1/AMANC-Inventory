using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AMANC_Inventory.Helpers;
using AMANC_Inventory.Interfaces;
using AMANC_Inventory.Models;
using Microsoft.Win32;

namespace AMANC_Inventory.ViewModels
{
    public class CompleteProfileOverlayViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly UserModel _currentUser;

        // Validar formato final: (+XX) 12 3456 7890, +XX 12 3456 7890 o 10 dígitos locales (12 3456 7890)
        private static readonly Regex PhoneRegex = new Regex(@"^(\(\+\d{1,3}\)|\+\d{1,3}\ )?\d{2}\ \d{4}\ \d{4}$");

        public Action? OnProfileCompleted { get; set; }

        public ObservableCollection<string> Branches { get; }
        public ObservableCollection<string> Departments { get; }
        public ObservableCollection<string> Relationships { get; }

        public string UserInitials => _currentUser.UserInitials;

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

        public bool HasProfilePicture => !string.IsNullOrEmpty(ProfilePictureBase64);
        public bool HasNoProfilePicture => !HasProfilePicture;

        public BitmapImage? ProfileImageSource
        {
            get
            {
                if (string.IsNullOrEmpty(ProfilePictureBase64)) return null;
                try
                {
                    byte[] binaryData = Convert.FromBase64String(ProfilePictureBase64);
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = new MemoryStream(binaryData);
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.EndInit();
                    return bi;
                }
                catch { return null; }
            }
        }

        // --- TELÉFONO PRINCIPAL CON AUTO-FORMATO ---
        public string Phone
        {
            get => _currentUser.Phone;
            set
            {
                string formatted = FormatPhoneNumber(value);
                if (_currentUser.Phone != formatted)
                {
                    _currentUser.Phone = formatted;
                    OnPropertyChanged();
                }
            }
        }

        // --- TELÉFONO EMERGENCIA CON AUTO-FORMATO ---
        public string EmergencyPhone
        {
            get => _currentUser.EmergencyPhone;
            set
            {
                string formatted = FormatPhoneNumber(value);
                if (_currentUser.EmergencyPhone != formatted)
                {
                    _currentUser.EmergencyPhone = formatted;
                    OnPropertyChanged();
                }
            }
        }

        // --- MAYÚSCULAS AUTOMÁTICAS ---
        public string EmergencyContactName
        {
            get => _currentUser.EmergencyContactName;
            set
            {
                _currentUser.EmergencyContactName = value?.ToUpper() ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public string EmergencyRelationship
        {
            get => _currentUser.EmergencyContactRelationship;
            set { _currentUser.EmergencyContactRelationship = value; OnPropertyChanged(); }
        }

        public string Branch { get => _currentUser.Branch; set { _currentUser.Branch = value; OnPropertyChanged(); } }
        public string Department { get => _currentUser.Department; set { _currentUser.Department = value; OnPropertyChanged(); } }

        public string City
        {
            get => _currentUser.City;
            set
            {
                _currentUser.City = value?.ToUpper() ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public DateTime? BirthDate { get => _currentUser.BirthDate; set { _currentUser.BirthDate = value; OnPropertyChanged(); } }

        public ICommand SelectPictureCommand { get; }
        public ICommand SaveProfileCommand { get; }

        public CompleteProfileOverlayViewModel(UserModel user, IUserService userService)
        {
            _currentUser = user;
            _userService = userService;

            Branches = ProfileCatalogs.GetBranches();
            Departments = ProfileCatalogs.GetDepartments();
            Relationships = ProfileCatalogs.GetRelationships();

            ProfilePictureBase64 = _currentUser.ProfilePictureBase64;

            SelectPictureCommand = new RelayCommand(_ => SelectPicture());
            SaveProfileCommand = new RelayCommand(async _ => await SaveProfileAsync());
        }

        /// <summary>
        /// Formatea automáticamente el número ingresado a (+XX) XX XXXX XXXX o XX XXXX XXXX
        /// </summary>
        private string FormatPhoneNumber(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            bool hasPlus = input.StartsWith("+");
            string digits = Regex.Replace(input, @"[^\d]", "");

            if (digits.Length == 0) return hasPlus ? "+" : string.Empty;

            // Máximo 13 dígitos totales (hasta 3 dígitos de clave de país + 10 del número)
            if (digits.Length > 13) digits = digits.Substring(0, 13);

            // Si se inició con '+' o hay más de 10 dígitos (ej. 521234567890)
            if (hasPlus || digits.Length > 10)
            {
                int ccLength = digits.Length > 10 ? digits.Length - 10 : Math.Min(2, digits.Length);
                if (ccLength > 3) ccLength = 3;

                string cc = digits.Substring(0, ccLength);
                string local = digits.Substring(ccLength);

                string formatted = $"+{cc}";

                if (local.Length > 0)
                {
                    string area = local.Substring(0, Math.Min(2, local.Length));
                    formatted += $" {area}";

                    if (local.Length > 2)
                    {
                        string mid = local.Substring(2, Math.Min(4, local.Length - 2));
                        formatted += $" {mid}";

                        if (local.Length > 6)
                        {
                            string end = local.Substring(6, Math.Min(4, local.Length - 6));
                            formatted += $" {end}";
                        }
                    }
                }
                return formatted;
            }
            else
            {
                // Formato local (10 dígitos o menos): XX XXXX XXXX
                string formatted = digits.Substring(0, Math.Min(2, digits.Length));

                if (digits.Length > 2)
                {
                    string mid = digits.Substring(2, Math.Min(4, digits.Length - 2));
                    formatted += $" {mid}";

                    if (digits.Length > 6)
                    {
                        string end = digits.Substring(6, Math.Min(4, digits.Length - 6));
                        formatted += $" {end}";
                    }
                }
                return formatted;
            }
        }

        private void SelectPicture()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Archivos de imagen (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                byte[] imageBytes = File.ReadAllBytes(openFileDialog.FileName);
                ProfilePictureBase64 = Convert.ToBase64String(imageBytes);
            }
        }

        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            return PhoneRegex.IsMatch(phone.Trim());
        }

        private async System.Threading.Tasks.Task SaveProfileAsync()
        {
            if (string.IsNullOrWhiteSpace(Phone) ||
                string.IsNullOrWhiteSpace(EmergencyPhone) ||
                string.IsNullOrWhiteSpace(EmergencyContactName) ||
                string.IsNullOrWhiteSpace(EmergencyRelationship) ||
                string.IsNullOrWhiteSpace(Branch) ||
                string.IsNullOrWhiteSpace(Department) ||
                string.IsNullOrWhiteSpace(City) ||
                BirthDate == null)
            {
                MessageBox.Show("Por favor, llena todos los campos obligatorios.", "Perfil Incompleto", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Si el usuario solo escribió los 10 dígitos (ej. 12 3456 7890), le anteponemos la lada +52 por defecto
            if (!Phone.StartsWith("+") && Regex.Replace(Phone, @"[^\d]", "").Length == 10)
            {
                Phone = "+52 " + Phone;
            }

            if (!EmergencyPhone.StartsWith("+") && Regex.Replace(EmergencyPhone, @"[^\d]", "").Length == 10)
            {
                EmergencyPhone = "+52 " + EmergencyPhone;
            }

            if (!IsValidPhone(Phone))
            {
                MessageBox.Show("El formato del número de teléfono debe ser completo (10 dígitos).\nEjemplo: +52 12 3456 7890 o 12 3456 7890", "Teléfono Incompleto", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsValidPhone(EmergencyPhone))
            {
                MessageBox.Show("El formato del teléfono de emergencia debe ser completo (10 dígitos).\nEjemplo: +52 12 3456 7890 o 12 3456 7890", "Teléfono Incompleto", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _currentUser.ProfilePictureBase64 = ProfilePictureBase64;
            _currentUser.IsProfileComplete = true;

            bool success = await _userService.UpdateUserProfileAsync(_currentUser);

            if (success)
            {
                OnProfileCompleted?.Invoke();
            }
            else
            {
                MessageBox.Show("Hubo un error al guardar los datos. Intenta nuevamente.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}