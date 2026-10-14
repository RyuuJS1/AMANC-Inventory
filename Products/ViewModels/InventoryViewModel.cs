using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Donations.Interfaces;
using AMANC_Inventory.Donations.Models;
using AMANC_Inventory.Donations.ViewModels;
using AMANC_Inventory.Inventory.Interfaces;
using AMANC_Inventory.Inventory.Models;
using AMANC_Inventory.Inventory.ViewModels;
using AMANC_Inventory.Pantries.Interfaces;
using AMANC_Inventory.Pantries.Models;
using AMANC_Inventory.Pantries.ViewModels;
using AMANC_Inventory.Patients.Interfaces;
using AMANC_Inventory.Patients.Models;
using AMANC_Inventory.Patients.ViewModels;
using AMANC_Inventory.Trips.Interfaces;
using AMANC_Inventory.Trips.Models;
using AMANC_Inventory.Trips.ViewModels;
using AMANC_Inventory.Users.Interfaces;
using AMANC_Inventory.Users.Models;
using AMANC_Inventory.Users.Services;
using AMANC_Inventory.Users.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AMANC_Inventory.Inventory.ViewModels
{
    public class InventoryViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private UserModel? _usuarioActual;
        private int _selectedTab = 0; // 0 = Consulta, 1 = Registro
        private int _selectedSubTab = 0; // 0=Productos, 1=Despensas, 2=Donaciones, 3=Pacientes, 4=Traslados, 5=Usuarios
        private string _filterText = string.Empty;

        // PAGINACIÓN
        private int _currentPage = 1;
        private int _totalPages = 1;

        // SUB-VIEWMODELS DE CONSULTA
        public ProductConsultViewModel ProductConsult { get; }
        public PantryConsultViewModel PantryConsult { get; }
        public DonationConsultViewModel DonationConsult { get; }
        public PatientConsultViewModel PatientConsult { get; }
        public TripConsultViewModel TripConsult { get; }
        public UserConsultViewModel UserConsult { get; }

        // SUB-VIEWMODELS DE REGISTRO
        public ProductRegisterViewModel ProductRegister { get; }
        public PantryRegisterViewModel PantryRegister { get; }
        public DonationRegisterViewModel DonationRegister { get; }
        public PatientRegisterViewModel PatientRegister { get; }
        public TripRegisterViewModel TripRegister { get; }

        // PROXIES DE LISTAS PARA BINDING CON LA VISTA INVENTORYCONSULTVIEW.XAML
        public ObservableCollection<ProductModel> FilteredInventoryList => ProductConsult.FilteredItems;
        public ObservableCollection<PantryModel> FilteredPantriesList => PantryConsult.FilteredPantriesList;
        public ObservableCollection<DonationModel> FilteredDonationsList => DonationConsult.FilteredDonationsList;
        public ObservableCollection<PatientModel> FilteredPatientsList => PatientConsult.FilteredPatientsList;
        public ObservableCollection<TripModel> FilteredTripsList => TripConsult.FilteredTripsList;
        public ObservableCollection<UserModel> FilteredUsersList => UserConsult.FilteredUsersList;

        // PERFIL DE USUARIO
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
            _userService = userService ?? new UserService();

            // 1. Instanciar Sub-ViewModels de Consulta
            ProductConsult = new ProductConsultViewModel(productService);
            PantryConsult = new PantryConsultViewModel(pantryService);
            DonationConsult = new DonationConsultViewModel(donationService);
            PatientConsult = new PatientConsultViewModel(patientService);
            TripConsult = new TripConsultViewModel(tripService);
            UserConsult = new UserConsultViewModel(_userService);

            // 2. Instanciar Sub-ViewModels de Registro
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
                    await PantryRegister.LoadDestinationsAsync();
                });

            TripRegister = new TripRegisterViewModel(
                tripService,
                async () => await TripConsult.ForceReloadAsync());

            // Catálogos
            Relationships = new ObservableCollection<string> { "Padre/Madre", "Cónyuge", "Hermano/a", "Hijo/a", "Tutor", "Otro" };
            Branches = new ObservableCollection<string> { "Sede Central", "Veracruz", "Xalapa", "Córdoba" };
            Departments = new ObservableCollection<string> { "Administración", "Inventario", "Trabajo Social", "Logística", "Sistemas" };

            // Comandos principales
            SelectMainTabCommand = new RelayCommand(p => SeleccionarPestanaPrincipal(p));
            SelectTabCommand = SelectMainTabCommand;
            SelectSubTabCommand = new RelayCommand(p => SeleccionarSubPestana(p));
            ClearFilterCommand = new RelayCommand(_ => FilterText = string.Empty);
            AbrirPerfilCommand = new RelayCommand(_ => AbrirPerfil());
            CerrarPerfilCommand = new RelayCommand(_ => CerrarPerfil());
            SelectPictureCommand = new RelayCommand(_ => SeleccionarFotoPerfil());
            SaveProfileCommand = new RelayCommand(_ => GuardarPerfil());

            // Comandos de Paginación
            FirstPageCommand = new RelayCommand(_ => CurrentPage = 1, _ => CurrentPage > 1);
            PreviousPageCommand = new RelayCommand(_ => CurrentPage--, _ => CurrentPage > 1);
            NextPageCommand = new RelayCommand(_ => CurrentPage++, _ => CurrentPage < TotalPages);
            LastPageCommand = new RelayCommand(_ => CurrentPage = TotalPages, _ => CurrentPage < TotalPages);
            GoToPageCommand = new RelayCommand(p => { if (p != null && int.TryParse(p.ToString(), out int page)) CurrentPage = page; });

            _ = ProductConsult.EnsureDataLoadedAsync();
        }

        #region Paginación

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (SetProperty(ref _currentPage, value))
                {
                    (FirstPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (PreviousPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (NextPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (LastPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public int TotalPages
        {
            get => _totalPages;
            set
            {
                if (SetProperty(ref _totalPages, value))
                {
                    (NextPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (LastPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand FirstPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand LastPageCommand { get; }
        public ICommand GoToPageCommand { get; }

        #endregion

        #region Control de Pestañas y Filtros

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
                    NotificarCambioSubPestanas();
                    _ = CargarPestanaActivaAsync();
                }
            }
        }

        public bool IsTabConsultVisible => SelectedTab == 0;
        public bool IsTabRegisterVisible => SelectedTab == 1;

        public bool IsInventoryTabSelected
        {
            get => SelectedSubTab == 0;
            set { if (value) SelectedSubTab = 0; }
        }

        public bool IsPantriesTabSelected
        {
            get => SelectedSubTab == 1;
            set { if (value) SelectedSubTab = 1; }
        }

        public bool IsDonationsTabSelected
        {
            get => SelectedSubTab == 2;
            set { if (value) SelectedSubTab = 2; }
        }

        public bool IsPatientsTabSelected
        {
            get => SelectedSubTab == 3;
            set { if (value) SelectedSubTab = 3; }
        }

        public bool IsTripsTabSelected
        {
            get => SelectedSubTab == 4;
            set { if (value) SelectedSubTab = 4; }
        }

        public bool IsUsersTabSelected
        {
            get => SelectedSubTab == 5;
            set { if (value) SelectedSubTab = 5; }
        }

        private void NotificarCambioSubPestanas()
        {
            OnPropertyChanged(nameof(IsInventoryTabSelected));
            OnPropertyChanged(nameof(IsPantriesTabSelected));
            OnPropertyChanged(nameof(IsDonationsTabSelected));
            OnPropertyChanged(nameof(IsPatientsTabSelected));
            OnPropertyChanged(nameof(IsTripsTabSelected));
            OnPropertyChanged(nameof(IsUsersTabSelected));
        }

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

        #region Permisos por Rol
        public bool CanAccessDonations => CurrentUser?.CanAccessDonations ?? true;
        public bool CanAccessPatients => CurrentUser?.CanAccessPatients ?? true;
        public bool CanAccessTrips => CurrentUser?.CanAccessTrips ?? true;
        public bool CanAccessUsers => CurrentUser?.CanAccessUsers ?? true;
        #endregion

        #region Propiedades de Perfil de Usuario

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

        public ObservableCollection<string> Relationships { get; }
        public ObservableCollection<string> Branches { get; }
        public ObservableCollection<string> Departments { get; }

        #endregion

        #region Comandos

        public ICommand SelectMainTabCommand { get; }
        public ICommand SelectTabCommand { get; }
        public ICommand SelectSubTabCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand AbrirPerfilCommand { get; }
        public ICommand CerrarPerfilCommand { get; }
        public ICommand SelectPictureCommand { get; }
        public ICommand SaveProfileCommand { get; }

        #endregion

        #region Métodos

        public void InicializarUsuario(UserModel usuario)
        {
            CurrentUser = usuario;

            if (usuario != null)
            {
                NombreUsuario = !string.IsNullOrEmpty(usuario.Name) ? usuario.Name : "Usuario";
                UserInitials = !string.IsNullOrEmpty(usuario.UserInitials) ? usuario.UserInitials : "U";
                HasProfilePicture = !string.IsNullOrEmpty(usuario.ProfilePictureBase64);
                HasNoProfilePicture = !HasProfilePicture;

                ProductRegister.SetCurrentUser(usuario);
                PantryRegister.SetCurrentUser(usuario);
                DonationRegister.SetCurrentUser(usuario);
            }
        }

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