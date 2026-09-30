using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AMANC_Inventory.Helpers;
using AMANC_Inventory.Interfaces;
using AMANC_Inventory.Models;

namespace AMANC_Inventory.ViewModels
{
    public class InventoryViewModel : ViewModelBase
    {
        private readonly IProductService? _productService;
        private UserModel? _usuarioActual;
        private bool _isLoading;
        private bool _isEditing;
        private string _searchText = "";

        // Navegación de Pestañas (0 = Consultas, 1 = Registros)
        private int _selectedTab = 0;

        // Pestañas individuales para RadioButtons y Visibilidad de DataGrids
        private bool _isInventoryTabSelected = true;
        private bool _isDonationsTabSelected;
        private bool _isPatientsTabSelected;
        private bool _isTripsTabSelected;
        private bool _isUsersTabSelected;

        // Filtro de búsqueda global
        private string _filterText = string.Empty;

        // Campos para Formulario de Alta / Registro
        private string _newCode = string.Empty;
        private string _newName = string.Empty;
        private string _newCategory = string.Empty;
        private int _newStock;

        // Campos para el Header y Perfil Básicos
        private string _nombreUsuario = "Usuario";
        private string _userInitials = "U";
        private bool _hasProfilePicture;
        private bool _hasNoProfilePicture = true;
        private bool _isProfileDetailsOpen;

        // --- CAMPOS DE DETALLE DE PERFIL (Resuelven los Data Error: 40) ---
        private object? _profileImageSource;
        private string _phone = string.Empty;
        private string _emergencyPhone = string.Empty;
        private string _emergencyContactName = string.Empty;
        private string _emergencyRelationship = string.Empty;
        private string _city = string.Empty;
        private DateTime? _birthDate;
        private string _branch = string.Empty;
        private string _department = string.Empty;

        // Constructor predeterminado (compatibilidad sin DI)
        public InventoryViewModel() : this(null)
        {
        }

        // Constructor con Inyección de Dependencias
        public InventoryViewModel(IProductService? productService)
        {
            _productService = productService;

            Items = new ObservableCollection<string>();
            InventoryItems = new ObservableCollection<InventoryItemModel>();

            // Inicialización de colecciones filtradas para vistas
            FilteredInventoryList = new ObservableCollection<InventoryItemModel>();
            FilteredDonationsList = new ObservableCollection<object>();
            FilteredPatientsList = new ObservableCollection<object>();
            FilteredTripsList = new ObservableCollection<object>();
            FilteredUsersList = new ObservableCollection<UserModel>();
            CategoriesList = new ObservableCollection<string> { "General", "Medicamento", "Alimento", "Ropa", "Insumo Médico" };

            // Inicialización de listas desplegables del Perfil
            Relationships = new ObservableCollection<string> { "Padre/Madre", "Cónyuge", "Hermano/a", "Hijo/a", "Tutor", "Otro" };
            Branches = new ObservableCollection<string> { "Sede Central", "Veracruz", "Xalapa", "Córdoba" };
            Departments = new ObservableCollection<string> { "Administración", "Inventario", "Trabajo Social", "Logística", "Sistemas" };

            // Inicialización de comandos
            LoadItemsCommand = new RelayCommand(async _ => await LoadProductsAsync());
            AddItemCommand = new RelayCommand(_ => AgregarItem());
            CancelEditCommand = new RelayCommand(_ => CancelarEdicion());
            SaveItemCommand = new RelayCommand(_ => GuardarElemento());
            SelectTabCommand = new RelayCommand(p => SeleccionarPestana(p));
            ClearFilterCommand = new RelayCommand(_ => FilterText = string.Empty);
            AbrirPerfilCommand = new RelayCommand(_ => AbrirPerfil());
            CerrarPerfilCommand = new RelayCommand(_ => CerrarPerfil());

            // Comandos para la gestión del Perfil
            SelectPictureCommand = new RelayCommand(_ => SeleccionarFotoPerfil());
            SaveProfileCommand = new RelayCommand(_ => GuardarPerfil());
        }

        #region Propiedades

        public UserModel? CurrentUser
        {
            get => _usuarioActual;
            set => SetProperty(ref _usuarioActual, value);
        }

        public ObservableCollection<string> Items { get; }
        public ObservableCollection<InventoryItemModel> InventoryItems { get; }

        // --- Listas Filtradas para DataGrids ---
        public ObservableCollection<InventoryItemModel> FilteredInventoryList { get; }
        public ObservableCollection<object> FilteredDonationsList { get; }
        public ObservableCollection<object> FilteredPatientsList { get; }
        public ObservableCollection<object> FilteredTripsList { get; }
        public ObservableCollection<UserModel> FilteredUsersList { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (SetProperty(ref _filterText, value))
                {
                    AplicarFiltros();
                }
            }
        }

        // --- Pestañas y Navegación ---

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

        public bool IsInventoryTabSelected
        {
            get => _isInventoryTabSelected;
            set => SetProperty(ref _isInventoryTabSelected, value);
        }

        public bool IsDonationsTabSelected
        {
            get => _isDonationsTabSelected;
            set => SetProperty(ref _isDonationsTabSelected, value);
        }

        public bool IsPatientsTabSelected
        {
            get => _isPatientsTabSelected;
            set => SetProperty(ref _isPatientsTabSelected, value);
        }

        public bool IsTripsTabSelected
        {
            get => _isTripsTabSelected;
            set => SetProperty(ref _isTripsTabSelected, value);
        }

        public bool IsUsersTabSelected
        {
            get => _isUsersTabSelected;
            set => SetProperty(ref _isUsersTabSelected, value);
        }

        // --- Formulario de Registro / Edición ---

        public string NewCode
        {
            get => _newCode;
            set => SetProperty(ref _newCode, value);
        }

        public string NewName
        {
            get => _newName;
            set => SetProperty(ref _newName, value);
        }

        public string NewCategory
        {
            get => _newCategory;
            set => SetProperty(ref _newCategory, value);
        }

        public int NewStock
        {
            get => _newStock;
            set => SetProperty(ref _newStock, value);
        }

        public ObservableCollection<string> CategoriesList { get; }

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

        // --- Propiedades Faltantes del Perfil de Usuario ---

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

        public ICommand LoadItemsCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand SaveItemCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand SelectTabCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand AbrirPerfilCommand { get; }
        public ICommand CerrarPerfilCommand { get; }

        // Comandos adicionales del Perfil
        public ICommand SelectPictureCommand { get; }
        public ICommand SaveProfileCommand { get; }

        #endregion

        #region Métodos

        public void SeleccionarPestana(object? parameter)
        {
            if (parameter != null && int.TryParse(parameter.ToString(), out int tabIndex))
            {
                SelectedTab = tabIndex;
            }
        }

        public void AbrirPerfil()
        {
            IsProfileDetailsOpen = true;
        }

        public void CerrarPerfil()
        {
            IsProfileDetailsOpen = false;
        }

        private void SeleccionarFotoPerfil()
        {
            // Lógica para abrir OpenFileDialog y seleccionar la foto de perfil
        }

        private void GuardarPerfil()
        {
            // Lógica para guardar las modificaciones del perfil
            IsProfileDetailsOpen = false;
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

            _ = LoadProductsAsync();
        }

        public async Task LoadProductsAsync()
        {
            IsLoading = true;
            InventoryItems.Clear();

            if (_productService != null)
            {
                var products = await _productService.GetAllProductsAsync();
                if (products != null)
                {
                    foreach (var item in products)
                    {
                        InventoryItems.Add(item);
                    }
                }
            }
            else
            {
                await Task.Delay(300);
            }

            AplicarFiltros();
            IsLoading = false;
        }

        private void AplicarFiltros()
        {
            FilteredInventoryList.Clear();

            var query = InventoryItems.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(FilterText))
            {
                query = query.Where(i =>
                    (i.Name != null && i.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                    (i.Code != null && i.Code.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                    (i.Category != null && i.Category.Contains(FilterText, StringComparison.OrdinalIgnoreCase))
                );
            }

            foreach (var item in query)
            {
                FilteredInventoryList.Add(item);
            }
        }

        private void AgregarItem()
        {
            IsEditing = true;
        }

        private void GuardarElemento()
        {
            IsEditing = false;
        }

        private void CancelarEdicion()
        {
            IsEditing = false;
        }

        #endregion
    }
}