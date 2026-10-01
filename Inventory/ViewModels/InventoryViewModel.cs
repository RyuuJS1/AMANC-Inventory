using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Inventory.Interfaces;
using AMANC_Inventory.Users.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AMANC_Inventory.Inventory.ViewModels
{
    public class InventoryViewModel : ViewModelBase
    {

        private UserModel? _usuarioActual;

        // Navegación de Pestañas Principales (0 = Consultas, 1 = Registros)
        private int _selectedTab = 0;

        // Sub-ViewModels
        public InventoryConsultViewModel ConsultViewModel { get; }
        public InventoryRegisterViewModel RegisterViewModel { get; }

        // Campos para el Header y Perfil de Usuario
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

        public InventoryViewModel() : this(null)
        {
        }

        public InventoryViewModel(IProductService? productService)
        {
            // Instanciación de Sub-ViewModels delegados
            ConsultViewModel = new InventoryConsultViewModel(productService);
            RegisterViewModel = new InventoryRegisterViewModel();

            // Colecciones para el modal de Perfil
            Relationships = new ObservableCollection<string> { "Padre/Madre", "Cónyuge", "Hermano/a", "Hijo/a", "Tutor", "Otro" };
            Branches = new ObservableCollection<string> { "Sede Central", "Veracruz", "Xalapa", "Córdoba" };
            Departments = new ObservableCollection<string> { "Administración", "Inventario", "Trabajo Social", "Logística", "Sistemas" };

            // Comandos de navegación principal
            SelectMainTabCommand = new RelayCommand(p => SeleccionarPestanaPrincipal(p));

            // Comandos del Perfil de Usuario
            AbrirPerfilCommand = new RelayCommand(_ => AbrirPerfil());
            CerrarPerfilCommand = new RelayCommand(_ => CerrarPerfil());
            SelectPictureCommand = new RelayCommand(_ => SeleccionarFotoPerfil());
            SaveProfileCommand = new RelayCommand(_ => GuardarPerfil());
            SelectTabCommand = new RelayCommand(p => SeleccionarPestanaPrincipal(p));
        }

        #region Propiedades

        public UserModel? CurrentUser
        {
            get => _usuarioActual;
            set => SetProperty(ref _usuarioActual, value);
        }

        public int SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (SetProperty(ref _selectedTab, value))
                {
                    OnPropertyChanged(nameof(IsTabConsultVisible));
                    OnPropertyChanged(nameof(IsTabRegisterVisible));
                }
            }
        }

        public bool IsTabConsultVisible => SelectedTab == 0;
        public bool IsTabRegisterVisible => SelectedTab == 1;

        // --- Header y Perfil ---

        public string NombreUsuario
        {
            get => _nombreUsuario;
            set => SetProperty(ref _nombreUsuario, value);
        }

        public string UserInitials
        {
            get => _userInitials;
            set => SetProperty(ref _userInitials, value);
        }

        public bool HasProfilePicture
        {
            get => _hasProfilePicture;
            set => SetProperty(ref _hasProfilePicture, value);
        }

        public bool HasNoProfilePicture
        {
            get => _hasNoProfilePicture;
            set => SetProperty(ref _hasNoProfilePicture, value);
        }

        public bool IsProfileDetailsOpen
        {
            get => _isProfileDetailsOpen;
            set => SetProperty(ref _isProfileDetailsOpen, value);
        }

        public object? ProfileImageSource
        {
            get => _profileImageSource;
            set => SetProperty(ref _profileImageSource, value);
        }

        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        public string EmergencyPhone
        {
            get => _emergencyPhone;
            set => SetProperty(ref _emergencyPhone, value);
        }

        public string EmergencyContactName
        {
            get => _emergencyContactName;
            set => SetProperty(ref _emergencyContactName, value);
        }

        public string EmergencyRelationship
        {
            get => _emergencyRelationship;
            set => SetProperty(ref _emergencyRelationship, value);
        }

        public string City
        {
            get => _city;
            set => SetProperty(ref _city, value);
        }

        public DateTime? BirthDate
        {
            get => _birthDate;
            set => SetProperty(ref _birthDate, value);
        }

        public string Branch
        {
            get => _branch;
            set => SetProperty(ref _branch, value);
        }

        public string Department
        {
            get => _department;
            set => SetProperty(ref _department, value);
        }

        public ObservableCollection<string> Relationships { get; }
        public ObservableCollection<string> Branches { get; }
        public ObservableCollection<string> Departments { get; }

        #endregion

        #region Comandos

        public ICommand SelectMainTabCommand { get; }
        public ICommand AbrirPerfilCommand { get; }
        public ICommand CerrarPerfilCommand { get; }
        public ICommand SelectPictureCommand { get; }
        public ICommand SaveProfileCommand { get; }
        public ICommand SelectTabCommand { get; }

        #endregion

        #region Métodos

        public void SeleccionarPestanaPrincipal(object? parameter)
        {
            if (parameter != null && int.TryParse(parameter.ToString(), out int tabIndex))
            {
                SelectedTab = tabIndex;
            }
        }

        public void InicializarUsuario(UserModel usuario)
        {
            CurrentUser = usuario;

            if (usuario != null)
            {
                NombreUsuario = !string.IsNullOrEmpty(usuario.Name) ? usuario.Name : "Usuario";
                UserInitials = !string.IsNullOrEmpty(usuario.UserInitials) ? usuario.UserInitials : "U";
                HasProfilePicture = !string.IsNullOrEmpty(usuario.ProfilePictureBase64);
                HasNoProfilePicture = !HasProfilePicture;
            }
            else
            {
                NombreUsuario = "Usuario";
                UserInitials = "U";
                HasProfilePicture = false;
                HasNoProfilePicture = true;
            }
        }

        public void AbrirPerfil() => IsProfileDetailsOpen = true;

        public void CerrarPerfil() => IsProfileDetailsOpen = false;

        private void SeleccionarFotoPerfil()
        {
            // Lógica para abrir OpenFileDialog y cargar la foto
        }

        private void GuardarPerfil()
        {
            IsProfileDetailsOpen = false;
        }

        #endregion
    }
}