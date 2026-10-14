using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Donations.Interfaces;
using AMANC_Inventory.Donations.ViewModels;
using AMANC_Inventory.Inventory.Interfaces;
using AMANC_Inventory.Inventory.ViewModels;
using AMANC_Inventory.Pantries.Interfaces;
using AMANC_Inventory.Pantries.ViewModels;
using AMANC_Inventory.Patients.Interfaces;
using AMANC_Inventory.Patients.ViewModels;
using AMANC_Inventory.Trips.Interfaces;
using AMANC_Inventory.Trips.ViewModels;
using AMANC_Inventory.Users.Interfaces;
using AMANC_Inventory.Users.Models;
using AMANC_Inventory.Users.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AMANC_Inventory.Inventory.ViewModels
{
    public class InventoryViewModel : ViewModelBase
    {
        private UserModel? _usuarioActual;
        private int _selectedTab = 0; // 0 = Consulta, 1 = Registro
        private int _selectedSubTab = 0; // 0=Productos, 1=Despensas, 2=Donaciones, 3=Pacientes, 4=Traslados, 5=Usuarios
        private string _filterText = string.Empty;

        // -------------------------------------------------------------
        // SUB-VIEWMODELS DE CONSULTA
        // -------------------------------------------------------------
        public ProductConsultViewModel ProductConsult { get; }
        public PantryConsultViewModel PantryConsult { get; }
        public DonationConsultViewModel DonationConsult { get; }
        public PatientConsultViewModel PatientConsult { get; }
        public TripConsultViewModel TripConsult { get; }
        public UserConsultViewModel UserConsult { get; }

        // -------------------------------------------------------------
        // SUB-VIEWMODELS DE REGISTRO
        // -------------------------------------------------------------
        public ProductRegisterViewModel ProductRegister { get; }
        public PantryRegisterViewModel PantryRegister { get; }
        public DonationRegisterViewModel DonationRegister { get; }
        public PatientRegisterViewModel PatientRegister { get; }
        public TripRegisterViewModel TripRegister { get; }

        // -------------------------------------------------------------
        // PROPIEDADES DE PERFIL DE USUARIO
        // -------------------------------------------------------------
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

        public InventoryViewModel() : this(null, null, null, null, null, null)
        {
        }

        public InventoryViewModel(
            IProductService? productService = null,
            IPantryService? pantryService = null,
            IDonationService? donationService = null,
            IPatientService? patientService = null,
            ITripService? tripService = null,
            IUserService? userService = null)
        {
            // 1. Instanciar Sub-ViewModels de Consulta
            ProductConsult = new ProductConsultViewModel(productService);
            PantryConsult = new PantryConsultViewModel(pantryService);
            DonationConsult = new DonationConsultViewModel(donationService);
            PatientConsult = new PatientConsultViewModel(patientService);
            TripConsult = new TripConsultViewModel(tripService);
            UserConsult = new UserConsultViewModel(userService);

            // 2. Instanciar Sub-ViewModels de Registro con Callbacks de auto-refresco
            ProductRegister = new ProductRegisterViewModel(
                productService,
                async () => await ProductConsult.ForceReloadAsync());

            PantryRegister = new PantryRegisterViewModel(
                pantryService,
                patientService,
                async () => await PantryConsult.ForceReloadAsync());

            DonationRegister = new DonationRegisterViewModel(
                donationService,
                async () => await DonationConsult.ForceReloadAsync());

            PatientRegister = new PatientRegisterViewModel(
                patientService,
                async () =>
                {
                    await PatientConsult.ForceReloadAsync();
                    await PantryRegister.LoadDestinationsAsync(); // Actualiza combo de despensas
                });

            TripRegister = new TripRegisterViewModel(
                tripService,
                async () => await TripConsult.ForceReloadAsync());

            // 3. Catálogos de perfil
            Relationships = new ObservableCollection<string> { "Padre/Madre", "Cónyuge", "Hermano/a", "Hijo/a", "Tutor", "Otro" };
            Branches = new ObservableCollection<string> { "Sede Central", "Veracruz", "Xalapa", "Córdoba" };
            Departments = new ObservableCollection<string> { "Administración", "Inventario", "Trabajo Social", "Logística", "Sistemas" };

            // 4. Comandos
            SelectMainTabCommand = new RelayCommand(p => SeleccionarPestanaPrincipal(p));
            SelectSubTabCommand = new RelayCommand(p => SeleccionarSubPestana(p));
            ClearFilterCommand = new RelayCommand(_ => FilterText = string.Empty);
            AbrirPerfilCommand = new RelayCommand(_ => AbrirPerfil());
            CerrarPerfilCommand = new RelayCommand(_ => CerrarPerfil());
            SelectPictureCommand = new RelayCommand(_ => SeleccionarFotoPerfil());
            SaveProfileCommand = new RelayCommand(_ => GuardarPerfil());

            // Carga inicial de la primera pestaña (Productos)
            _ = ProductConsult.EnsureDataLoadedAsync();
        }

        #region Control de Pestañas y Filtro

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

        public int SelectedSubTab
        {
            get => _selectedSubTab;
            set
            {
                if (SetProperty(ref _selectedSubTab, value))
                {
                    _ = CargarPestanaActivaAsync();
                }
            }
        }

        public bool IsTabConsultVisible => SelectedTab == 0;
        public bool IsTabRegisterVisible => SelectedTab == 1;

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (SetProperty(ref _filterText, value))
                {
                    AplicarFiltros(value);
                }
            }
        }

        public void SeleccionarPestanaPrincipal(object? parameter)
        {
            if (parameter != null && int.TryParse(parameter.ToString(), out int tabIndex))
            {
                SelectedTab = tabIndex;
            }
        }

        public void SeleccionarSubPestana(object? parameter)
        {
            if (parameter != null && int.TryParse(parameter.ToString(), out int subIndex))
            {
                SelectedSubTab = subIndex;
            }
        }

        /// <summary>
        /// Carga perezosa (Lazy Loading): Solo consulta Firebase cuando la sub-pestaña es abierta.
        /// </summary>
        private async Task CargarPestanaActivaAsync()
        {
            switch (SelectedSubTab)
            {
                case 0: await ProductConsult.EnsureDataLoadedAsync(); break;
                case 1: await PantryConsult.EnsureDataLoadedAsync(); break;
                case 2: await DonationConsult.EnsureDataLoadedAsync(); break;
                case 3: await PatientConsult.EnsureDataLoadedAsync(); break;
                case 4: await TripConsult.EnsureDataLoadedAsync(); break;
                case 5: await UserConsult.EnsureDataLoadedAsync(); break;
            }
        }

        private void AplicarFiltros(string text)
        {
            ProductConsult.ApplyFilter(text);
            PantryConsult.ApplyFilter(text);
            DonationConsult.ApplyFilter(text);
            PatientConsult.ApplyFilter(text);
            TripConsult.ApplyFilter(text);
            UserConsult.ApplyFilter(text);
        }

        #endregion

        #region Propiedades de Perfil de Usuario

        public UserModel? CurrentUser
        {
            get => _usuarioActual;
            set => SetProperty(ref _usuarioActual, value);
        }

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
        public ICommand SelectSubTabCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand AbrirPerfilCommand { get; }
        public ICommand CerrarPerfilCommand { get; }
        public ICommand SelectPictureCommand { get; }
        public ICommand SaveProfileCommand { get; }

        #endregion

        #region Métodos de Perfil

        public void InicializarUsuario(UserModel usuario)
        {
            CurrentUser = usuario;

            if (usuario != null)
            {
                NombreUsuario = !string.IsNullOrEmpty(usuario.Name) ? usuario.Name : "Usuario";
                UserInitials = !string.IsNullOrEmpty(usuario.UserInitials) ? usuario.UserInitials : "U";
                HasProfilePicture = !string.IsNullOrEmpty(usuario.ProfilePictureBase64);
                HasNoProfilePicture = !HasProfilePicture;

                // Transmitir información del usuario a los formularios de registro
                ProductRegister.SetCurrentUser(usuario);
                PantryRegister.SetCurrentUser(usuario);
                DonationRegister.SetCurrentUser(usuario);
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
        }

        private void GuardarPerfil()
        {
            IsProfileDetailsOpen = false;
        }

        #endregion
    }
}