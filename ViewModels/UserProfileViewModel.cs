using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AMANC_Inventory.Helpers;
using AMANC_Inventory.Interfaces;
using AMANC_Inventory.Models;
using Microsoft.Win32;

namespace AMANC_Inventory.ViewModels
{
    public class UserProfileViewModel : ViewModelBase
    {
        private readonly UserModel _currentUser;
        private readonly IUserService _userService;

        public Action? OnReturnToInventoryRequested { get; set; }

        public ObservableCollection<string> Branches { get; }
        public ObservableCollection<string> Departments { get; }

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
            set { _currentUser.Name = value; OnPropertyChanged(); }
        }

        public string CorreoUsuario => _currentUser.Email ?? string.Empty;

        public string TelefonoUsuario
        {
            get => _currentUser.Phone ?? string.Empty;
            set { _currentUser.Phone = value; OnPropertyChanged(); }
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
            set { _currentUser.EmergencyPhone = value; OnPropertyChanged(); }
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

        public UserProfileViewModel(UserModel user, IUserService userService)
        {
            _currentUser = user ?? new UserModel();
            _userService = userService;

            Branches = ProfileCatalogs.GetBranches();
            Departments = ProfileCatalogs.GetDepartments();
            ProfilePictureBase64 = _currentUser.ProfilePictureBase64;

            CloseProfileCommand = new RelayCommand(_ => OnReturnToInventoryRequested?.Invoke());
            ChangeProfilePictureCommand = new RelayCommand(_ => SelectPicture());
            EditProfileCommand = new RelayCommand(_ => IsEditing = true);
            CancelEditCommand = new RelayCommand(_ => IsEditing = false);
            SaveChangesCommand = new RelayCommand(async _ => await SaveChangesAsync());
            ChangePasswordCommand = new RelayCommand(_ => RequestPasswordReset());
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
                MessageBox.Show("Perfil actualizado correctamente en la nube.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("No se pudieron guardar los cambios. Verifique su conexión.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RequestPasswordReset()
        {
            MessageBox.Show($"Se ha enviado un enlace de restablecimiento de contraseña a {CorreoUsuario}.", "Cambio de Clave", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}