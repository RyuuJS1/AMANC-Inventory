using AMANC_Inventory.Helpers;
using AMANC_Inventory.Interfaces;
using AMANC_Inventory.Models;
using AMANC_Inventory.Services;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace AMANC_Inventory.ViewModels
{
    public class UserProfileViewModel : ViewModelBase
    {
        private readonly UserModel _currentUser;
        private readonly IUserService _userService;
        private readonly IAuthService _authService; // Reemplazado IEmailService por IAuthService (Firebase)

        // --- Respaldo para Cancelar Edición ---
        private UserModel? _userBackup;
        private string _entryTimeBackup = "00:00";
        private string _exitTimeBackup = "00:00";

        public Action? OnReturnToInventoryRequested { get; set; }

        // --- Colecciones para los ComboBox ---
        public ObservableCollection<string> Branches { get; }
        public ObservableCollection<string> Departments { get; }
        public ObservableCollection<string> Relationships { get; }
        public ObservableCollection<string> Hours { get; }
        public ObservableCollection<string> ShirtSizes { get; }
        public ObservableCollection<string> AreasOfInterest { get; }

        // --- Estado de la Interfaz ---
        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        private bool _isProfileDetailsOpen = true;
        public bool IsProfileDetailsOpen
        {
            get => _isProfileDetailsOpen;
            set => SetProperty(ref _isProfileDetailsOpen, value);
        }

        // --- Propiedades Mapeadas con el XAML ---
        public string NombreUsuario
        {
            get => _currentUser.Name ?? string.Empty;
            set { _currentUser.Name = value; OnPropertyChanged(); OnPropertyChanged(nameof(UserInitials)); }
        }

        public string CorreoUsuario => _currentUser.Email ?? string.Empty;

        public string TelefonoUsuario
        {
            get => _currentUser.Phone ?? string.Empty;
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

        public string RolUsuario => _currentUser.Role ?? "Voluntario";

        public string UserInitials => _currentUser.UserInitials ?? "U";

        public string Branch
        {
            get => _currentUser.Branch ?? string.Empty;
            set { _currentUser.Branch = value; OnPropertyChanged(); }
        }

        public string Department
        {
            get => _currentUser.Department ?? string.Empty;
            set { _currentUser.Department = value; OnPropertyChanged(); }
        }

        public string City
        {
            get => _currentUser.City ?? string.Empty;
            set { _currentUser.City = value; OnPropertyChanged(); }
        }

        public DateTime? BirthDate
        {
            get => _currentUser.BirthDate;
            set { _currentUser.BirthDate = value; OnPropertyChanged(); }
        }

        public string EmergencyContactName
        {
            get => _currentUser.EmergencyContactName ?? string.Empty;
            set { _currentUser.EmergencyContactName = value; OnPropertyChanged(); }
        }

        public string EmergencyContactRelationship
        {
            get => _currentUser.EmergencyContactRelationship ?? string.Empty;
            set { _currentUser.EmergencyContactRelationship = value; OnPropertyChanged(); }
        }

        public string EmergencyContactPhone
        {
            get => _currentUser.EmergencyPhone ?? string.Empty;
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

        // --- Horario de Entrada y Salida (Logística) ---
        private string _entryTime = "00:00";
        public string EntryTime
        {
            get => _entryTime;
            set
            {
                if (SetProperty(ref _entryTime, value))
                {
                    UpdateAvailability();
                }
            }
        }

        private string _exitTime = "00:00";
        public string ExitTime
        {
            get => _exitTime;
            set
            {
                if (SetProperty(ref _exitTime, value))
                {
                    UpdateAvailability();
                }
            }
        }

        public string Availability
        {
            get => _currentUser.Availability ?? string.Empty;
            set { _currentUser.Availability = value; OnPropertyChanged(); }
        }

        public string Skills
        {
            get => _currentUser.Skills ?? string.Empty;
            set { _currentUser.Skills = value; OnPropertyChanged(); }
        }

        public string ShirtSize
        {
            get => _currentUser.ShirtSize ?? string.Empty;
            set { _currentUser.ShirtSize = value; OnPropertyChanged(); }
        }

        // --- Manejo de Foto de Perfil ---
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

        // --- Comandos Vinculados al XAML ---
        public ICommand CloseProfileCommand { get; }
        public ICommand ChangeProfilePictureCommand { get; }
        public ICommand EditProfileCommand { get; }
        public ICommand SaveChangesCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand ChangePasswordCommand { get; }

        // Constructor principal que recibe IAuthService
        public UserProfileViewModel(UserModel user, IUserService userService, IAuthService authService)
        {
            _currentUser = user ?? new UserModel();
            _userService = userService;
            _authService = authService;

            // Cargar Catálogos
            Branches = ProfileCatalogs.GetBranches();
            Departments = ProfileCatalogs.GetDepartments();
            Relationships = ProfileCatalogs.GetRelationships();
            Hours = ProfileCatalogs.GetHours();
            ShirtSizes = ProfileCatalogs.GetSize();
            AreasOfInterest = ProfileCatalogs.GetAreasOfInterest();

            ProfilePictureBase64 = _currentUser.ProfilePictureBase64;

            ParseInitialAvailability();

            CloseProfileCommand = new RelayCommand(_ => OnReturnToInventoryRequested?.Invoke());
            ChangeProfilePictureCommand = new RelayCommand(_ => SelectPicture());
            EditProfileCommand = new RelayCommand(_ => StartEditing());
            CancelEditCommand = new RelayCommand(_ => CancelEditing());
            SaveChangesCommand = new RelayCommand(async _ => await SaveChangesAsync());
            ChangePasswordCommand = new RelayCommand(async _ => await RequestPasswordResetAsync());
        }

        // Sobrecarga de compatibilidad en caso de que alguna vista siga pasando IEmailService o solo 2 parámetros
        public UserProfileViewModel(UserModel user, IUserService userService, IEmailService emailService)
            : this(user, userService, new AuthService())
        {
        }

        public UserProfileViewModel(UserModel user, IUserService userService)
            : this(user, userService, new AuthService())
        {
        }

        // --- Métodos de Edición / Cancelación ---
        private void StartEditing()
        {
            _userBackup = new UserModel
            {
                Name = _currentUser.Name,
                Phone = _currentUser.Phone,
                Branch = _currentUser.Branch,
                Department = _currentUser.Department,
                City = _currentUser.City,
                BirthDate = _currentUser.BirthDate,
                EmergencyContactName = _currentUser.EmergencyContactName,
                EmergencyContactRelationship = _currentUser.EmergencyContactRelationship,
                EmergencyPhone = _currentUser.EmergencyPhone,
                Availability = _currentUser.Availability,
                Skills = _currentUser.Skills,
                ShirtSize = _currentUser.ShirtSize,
                ProfilePictureBase64 = ProfilePictureBase64
            };

            _entryTimeBackup = _entryTime;
            _exitTimeBackup = _exitTime;

            IsEditing = true;
        }

        private void CancelEditing()
        {
            if (_userBackup != null)
            {
                _currentUser.Name = _userBackup.Name;
                _currentUser.Phone = _userBackup.Phone;
                _currentUser.Branch = _userBackup.Branch;
                _currentUser.Department = _userBackup.Department;
                _currentUser.City = _userBackup.City;
                _currentUser.BirthDate = _userBackup.BirthDate;
                _currentUser.EmergencyContactName = _userBackup.EmergencyContactName;
                _currentUser.EmergencyContactRelationship = _userBackup.EmergencyContactRelationship;
                _currentUser.EmergencyPhone = _userBackup.EmergencyPhone;
                _currentUser.Availability = _userBackup.Availability;
                _currentUser.Skills = _userBackup.Skills;
                _currentUser.ShirtSize = _userBackup.ShirtSize;

                _entryTime = _entryTimeBackup;
                _exitTime = _exitTimeBackup;
                ProfilePictureBase64 = _userBackup.ProfilePictureBase64;

                RefreshAllProperties();
            }

            IsEditing = false;
        }

        private void RefreshAllProperties()
        {
            OnPropertyChanged(nameof(NombreUsuario));
            OnPropertyChanged(nameof(TelefonoUsuario));
            OnPropertyChanged(nameof(Branch));
            OnPropertyChanged(nameof(Department));
            OnPropertyChanged(nameof(City));
            OnPropertyChanged(nameof(BirthDate));
            OnPropertyChanged(nameof(EmergencyContactName));
            OnPropertyChanged(nameof(EmergencyContactRelationship));
            OnPropertyChanged(nameof(EmergencyContactPhone));
            OnPropertyChanged(nameof(EntryTime));
            OnPropertyChanged(nameof(ExitTime));
            OnPropertyChanged(nameof(Availability));
            OnPropertyChanged(nameof(Skills));
            OnPropertyChanged(nameof(ShirtSize));
            OnPropertyChanged(nameof(UserInitials));
        }

        private void UpdateAvailability()
        {
            Availability = $"{EntryTime} - {ExitTime}";
        }

        private void ParseInitialAvailability()
        {
            if (!string.IsNullOrWhiteSpace(_currentUser.Availability) && _currentUser.Availability.Contains('-'))
            {
                string[] parts = _currentUser.Availability.Split('-');
                if (parts.Length == 2)
                {
                    _entryTime = parts[0].Trim();
                    _exitTime = parts[1].Trim();
                }
            }
        }

        private void SelectPicture()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Imágenes (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg" };
            if (openFileDialog.ShowDialog() == true)
            {
                byte[] bytes = File.ReadAllBytes(openFileDialog.FileName);
                ProfilePictureBase64 = Convert.ToBase64String(bytes);
            }
        }

        private async Task SaveChangesAsync()
        {
            _currentUser.ProfilePictureBase64 = ProfilePictureBase64;
            bool ok = await _userService.UpdateUserProfileAsync(_currentUser);
            if (ok)
            {
                IsEditing = false;
                _userBackup = null;
            }
            else
            {
                MessageBox.Show("No se pudieron guardar los cambios. Verifique su conexión.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RequestPasswordResetAsync()
        {
            string correo = CorreoUsuario?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(correo))
            {
                MessageBox.Show("No se encontró un correo electrónico asociado a este perfil.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Llamada directa a Firebase AuthService
            bool enviado = await _authService.SendPasswordResetEmailAsync(correo);

            if (enviado)
            {
                MessageBox.Show($"Se han enviado las instrucciones de restablecimiento al correo:\n{correo}\n\nRevisa tu bandeja de entrada o la carpeta de SPAM.",
                                "Correo Enviado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Ocurrió un error al intentar enviar el correo. Verifica tu conexión e intenta nuevamente.",
                                "Error de Envío", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string FormatPhoneNumber(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            bool hasPlus = input.StartsWith("+");
            string digits = Regex.Replace(input, @"[^\d]", "");

            if (digits.Length == 0) return hasPlus ? "+" : string.Empty;

            if (digits.Length > 13) digits = digits.Substring(0, 13);

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
    }
}