using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Users.Models;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace AMANC_Inventory.Inventory.ViewModels
{
    public partial class InventoryViewModel
    {
        private UserModel? _usuarioActual;
        private string _nombreUsuario = "Usuario";
        private string _userInitials = "U";
        private bool _hasProfilePicture;
        private bool _hasNoProfilePicture = true;
        private bool _isProfileDetailsOpen;

        private object? _profileImageSource;
        private string _phone = string.Empty;
        private string _emergencyPhone = string.Empty;
        private string _emergencyContactName = string.Empty;
        private string _emergencyRelationship = string.Empty;
        private string _city = string.Empty;
        private DateTime? _birthDate;
        private string _branch = string.Empty;
        private string _department = string.Empty;

        public ObservableCollection<string> Relationships { get; private set; } = null!;
        public ObservableCollection<string> Branches { get; private set; } = null!;
        public ObservableCollection<string> Departments { get; private set; } = null!;

        public ICommand AbrirPerfilCommand { get; private set; } = null!;
        public ICommand CerrarPerfilCommand { get; private set; } = null!;
        public ICommand SelectPictureCommand { get; private set; } = null!;
        public ICommand SaveProfileCommand { get; private set; } = null!;

        private void InicializarComandosPerfil()
        {
            AbrirPerfilCommand = new RelayCommand(_ => AbrirPerfil());
            CerrarPerfilCommand = new RelayCommand(_ => CerrarPerfil());
            SelectPictureCommand = new RelayCommand(_ => SeleccionarFotoPerfil());
            SaveProfileCommand = new RelayCommand(_ => GuardarPerfil());
        }

        #region Propiedades de Perfil

        public UserModel? CurrentUser
        {
            get => _usuarioActual;
            set
            {
                if (SetProperty(ref _usuarioActual, value))
                {
                    OnPropertyChanged(nameof(CanAccessDonations));
                    OnPropertyChanged(nameof(CanAccessPatients));
                    OnPropertyChanged(nameof(CanAccessTrips));
                    OnPropertyChanged(nameof(CanAccessUsers));
                }
            }
        }

        public string NombreUsuario { get => _nombreUsuario; set => SetProperty(ref _nombreUsuario, value); }
        public string UserInitials { get => _userInitials; set => SetProperty(ref _userInitials, value); }
        public bool HasProfilePicture { get => _hasProfilePicture; set => SetProperty(ref _hasProfilePicture, value); }
        public bool HasNoProfilePicture { get => _hasNoProfilePicture; set => SetProperty(ref _hasNoProfilePicture, value); }
        public bool IsProfileDetailsOpen { get => _isProfileDetailsOpen; set => SetProperty(ref _isProfileDetailsOpen, value); }

        public object? ProfileImageSource { get => _profileImageSource; set => SetProperty(ref _profileImageSource, value); }
        public string Phone { get => _phone; set => SetProperty(ref _phone, value); }
        public string EmergencyPhone { get => _emergencyPhone; set => SetProperty(ref _emergencyPhone, value); }
        public string EmergencyContactName { get => _emergencyContactName; set => SetProperty(ref _emergencyContactName, value); }
        public string EmergencyRelationship { get => _emergencyRelationship; set => SetProperty(ref _emergencyRelationship, value); }
        public string City { get => _city; set => SetProperty(ref _city, value); }
        public DateTime? BirthDate { get => _birthDate; set => SetProperty(ref _birthDate, value); }
        public string Branch { get => _branch; set => SetProperty(ref _branch, value); }
        public string Department { get => _department; set => SetProperty(ref _department, value); }

        #endregion

        #region Métodos de Perfil

        public void AbrirPerfil() => IsProfileDetailsOpen = true;
        public void CerrarPerfil() => IsProfileDetailsOpen = false;

        private async void SeleccionarFotoPerfil()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Archivos de imagen (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    byte[] bytes = File.ReadAllBytes(openFileDialog.FileName);
                    string base64 = Convert.ToBase64String(bytes);

                    if (CurrentUser != null)
                    {
                        CurrentUser.ProfilePictureBase64 = base64;
                        HasProfilePicture = true;
                        HasNoProfilePicture = false;
                        ProfileImageSource = CurrentUser.ProfileImageSource;

                        await _userService.UpdateUserProfileAsync(CurrentUser);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al seleccionar la imagen: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void GuardarPerfil()
        {
            if (CurrentUser != null)
            {
                await _userService.UpdateUserProfileAsync(CurrentUser);
            }
            IsProfileDetailsOpen = false;
        }

        #endregion
    }
}