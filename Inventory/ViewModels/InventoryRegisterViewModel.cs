using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using AMANC_Inventory.Core.Architecture;

namespace AMANC_Inventory.Inventory.ViewModels
{
    public class InventoryRegisterViewModel : INotifyPropertyChanged
    {
        // -------------------------------------------------------------
        // ESTADOS DE LAS PESTAÑAS (TAB SELECTION)
        // -------------------------------------------------------------
        private bool _isInventoryTabSelected = true;
        public bool IsInventoryTabSelected
        {
            get => _isInventoryTabSelected;
            set { _isInventoryTabSelected = value; OnPropertyChanged(); }
        }

        private bool _isDonationsTabSelected;
        public bool IsDonationsTabSelected
        {
            get => _isDonationsTabSelected;
            set { _isDonationsTabSelected = value; OnPropertyChanged(); }
        }

        private bool _isPatientsTabSelected;
        public bool IsPatientsTabSelected
        {
            get => _isPatientsTabSelected;
            set { _isPatientsTabSelected = value; OnPropertyChanged(); }
        }

        private bool _isTripsTabSelected;
        public bool IsTripsTabSelected
        {
            get => _isTripsTabSelected;
            set { _isTripsTabSelected = value; OnPropertyChanged(); }
        }

        // -------------------------------------------------------------
        // PROPIEDADES - PESTAÑA INVENTARIO
        // -------------------------------------------------------------
        private string? _newCode;
        public string? NewCode { get => _newCode; set { _newCode = value; OnPropertyChanged(); } }

        private string? _newName;
        public string? NewName { get => _newName; set { _newName = value; OnPropertyChanged(); } }

        private string? _newCategory;
        public string? NewCategory { get => _newCategory; set { _newCategory = value; OnPropertyChanged(); } }

        private string? _newStock;
        public string? NewStock { get => _newStock; set { _newStock = value; OnPropertyChanged(); } }

        private string? _newMinStock;
        public string? NewMinStock { get => _newMinStock; set { _newMinStock = value; OnPropertyChanged(); } }

        private string? _newUnit;
        public string? NewUnit { get => _newUnit; set { _newUnit = value; OnPropertyChanged(); } }

        private string? _newLocation;
        public string? NewLocation { get => _newLocation; set { _newLocation = value; OnPropertyChanged(); } }

        private string? _newBranch;
        public string? NewBranch { get => _newBranch; set { _newBranch = value; OnPropertyChanged(); } }

        private string? _newStatus;
        public string? NewStatus { get => _newStatus; set { _newStatus = value; OnPropertyChanged(); } }

        private string? _newDescription;
        public string? NewDescription { get => _newDescription; set { _newDescription = value; OnPropertyChanged(); } }

        // -------------------------------------------------------------
        // PROPIEDADES - PESTAÑA DONACIONES
        // -------------------------------------------------------------
        private string? _newDonorCompany;
        public string? NewDonorCompany { get => _newDonorCompany; set { _newDonorCompany = value; OnPropertyChanged(); } }

        private string? _newDonorType;
        public string? NewDonorType { get => _newDonorType; set { _newDonorType = value; OnPropertyChanged(); } }

        private DateTime? _newDonationDate = DateTime.Now;
        public DateTime? NewDonationDate { get => _newDonationDate; set { _newDonationDate = value; OnPropertyChanged(); } }

        private string? _newDonatedProduct;
        public string? NewDonatedProduct { get => _newDonatedProduct; set { _newDonatedProduct = value; OnPropertyChanged(); } }

        private string? _newPresentation;
        public string? NewPresentation { get => _newPresentation; set { _newPresentation = value; OnPropertyChanged(); } }

        private string? _newDonationQuantity;
        public string? NewDonationQuantity { get => _newDonationQuantity; set { _newDonationQuantity = value; OnPropertyChanged(); } }

        private string? _newUnitPrice;
        public string? NewUnitPrice { get => _newUnitPrice; set { _newUnitPrice = value; OnPropertyChanged(); } }

        private string? _newReceiptFolio;
        public string? NewReceiptFolio { get => _newReceiptFolio; set { _newReceiptFolio = value; OnPropertyChanged(); } }

        private string? _newDonationNotes;
        public string? NewDonationNotes { get => _newDonationNotes; set { _newDonationNotes = value; OnPropertyChanged(); } }

        // -------------------------------------------------------------
        // PROPIEDADES - PESTAÑA PACIENTES
        // -------------------------------------------------------------
        private string? _newChildName;
        public string? NewChildName { get => _newChildName; set { _newChildName = value; OnPropertyChanged(); } }

        private string? _newAge;
        public string? NewAge { get => _newAge; set { _newAge = value; OnPropertyChanged(); } }

        private string? _newCurp;
        public string? NewCurp { get => _newCurp; set { _newCurp = value; OnPropertyChanged(); } }

        private string? _newCancerType;
        public string? NewCancerType { get => _newCancerType; set { _newCancerType = value; OnPropertyChanged(); } }

        private string? _newGuardians;
        public string? NewGuardians { get => _newGuardians; set { _newGuardians = value; OnPropertyChanged(); } }

        private string? _newRelationship;
        public string? NewRelationship { get => _newRelationship; set { _newRelationship = value; OnPropertyChanged(); } }

        private string? _newPhones;
        public string? NewPhones { get => _newPhones; set { _newPhones = value; OnPropertyChanged(); } }

        private string? _newEmails;
        public string? NewEmails { get => _newEmails; set { _newEmails = value; OnPropertyChanged(); } }

        private string? _newCity;
        public string? NewCity { get => _newCity; set { _newCity = value; OnPropertyChanged(); } }

        private string? _newState;
        public string? NewState { get => _newState; set { _newState = value; OnPropertyChanged(); } }

        private string? _newPatientStatus;
        public string? NewPatientStatus { get => _newPatientStatus; set { _newPatientStatus = value; OnPropertyChanged(); } }

        private string? _newPatientNotes;
        public string? NewPatientNotes { get => _newPatientNotes; set { _newPatientNotes = value; OnPropertyChanged(); } }

        // -------------------------------------------------------------
        // PROPIEDADES - PESTAÑA VIAJES / TRIPS
        // -------------------------------------------------------------
        private string? _newTripChild;
        public string? NewTripChild { get => _newTripChild; set { _newTripChild = value; OnPropertyChanged(); } }

        private string? _newTripGuardian;
        public string? NewTripGuardian { get => _newTripGuardian; set { _newTripGuardian = value; OnPropertyChanged(); } }

        private string? _newOrigin;
        public string? NewOrigin { get => _newOrigin; set { _newOrigin = value; OnPropertyChanged(); } }

        private string? _newDestination;
        public string? NewDestination { get => _newDestination; set { _newDestination = value; OnPropertyChanged(); } }

        private string? _newTransportType;
        public string? NewTransportType { get => _newTransportType; set { _newTransportType = value; OnPropertyChanged(); } }

        private DateTime? _newDepartureDate = DateTime.Now;
        public DateTime? NewDepartureDate { get => _newDepartureDate; set { _newDepartureDate = value; OnPropertyChanged(); } }

        private DateTime? _newReturnDate = DateTime.Now;
        public DateTime? NewReturnDate { get => _newReturnDate; set { _newReturnDate = value; OnPropertyChanged(); } }

        private string? _newTripStatus;
        public string? NewTripStatus { get => _newTripStatus; set { _newTripStatus = value; OnPropertyChanged(); } }

        private string? _newTripNotes;
        public string? NewTripNotes { get => _newTripNotes; set { _newTripNotes = value; OnPropertyChanged(); } }

        // -------------------------------------------------------------
        // COLECCIONES PARA COMBOBOX
        // -------------------------------------------------------------
        public ObservableCollection<string> CategoriesList { get; set; } = new();
        public ObservableCollection<string> UnitsList { get; set; } = new();
        public ObservableCollection<string> BranchesList { get; set; } = new();
        public ObservableCollection<string> StatusList { get; set; } = new();
        public ObservableCollection<string> DonorTypesList { get; set; } = new();
        public ObservableCollection<string> PatientStatusList { get; set; } = new();
        public ObservableCollection<string> TransportTypesList { get; set; } = new();
        public ObservableCollection<string> TripStatusList { get; set; } = new();

        // -------------------------------------------------------------
        // DECLARACIÓN DE COMANDOS (ICommand)
        // -------------------------------------------------------------
        public ICommand ResetFormCommand { get; }
        public ICommand SaveItemCommand { get; }
        public ICommand SaveDonationCommand { get; }
        public ICommand SavePatientCommand { get; }
        public ICommand SaveTripCommand { get; }

        // -------------------------------------------------------------
        // CONSTRUCTOR
        // -------------------------------------------------------------
        public InventoryRegisterViewModel()
        {
            IsInventoryTabSelected = true;

            // Inicializar opciones por defecto para los ComboBox
            LoadCatalogDefaults();

            ResetFormCommand = new RelayCommand(_ => ResetForm());
            SaveItemCommand = new RelayCommand(async _ => await SaveItemAsync());
            SaveDonationCommand = new RelayCommand(async _ => await SaveDonationAsync());
            SavePatientCommand = new RelayCommand(async _ => await SavePatientAsync());
            SaveTripCommand = new RelayCommand(async _ => await SaveTripAsync());
        }

        private void LoadCatalogDefaults()
        {
            BranchesList = new ObservableCollection<string> { "Sede Central", "Sucursal Norte", "Sucursal Sur" };
            CategoriesList = new ObservableCollection<string> { "Medicamento", "Alimento", "Ropa", "Insumo Médico" };
            UnitsList = new ObservableCollection<string> { "Pieza", "Caja", "Kg", "Litro" };
            StatusList = new ObservableCollection<string> { "Disponible", "Agotado", "Reservado" };
            DonorTypesList = new ObservableCollection<string> { "Particular", "Empresa", "Fundación" };
            PatientStatusList = new ObservableCollection<string> { "Activo", "En Tratamiento", "Inactivo" };
            TransportTypesList = new ObservableCollection<string> { "Autobús", "Avión", "Particular" };
            TripStatusList = new ObservableCollection<string> { "Programado", "En Proceso", "Completado", "Cancelado" };
        }

        // -------------------------------------------------------------
        // MÉTODOS DE LÓGICA DE NEGOCIO
        // -------------------------------------------------------------
        private void ResetForm()
        {
            NewCode = string.Empty;
            NewName = string.Empty;
            NewCategory = null;
            NewStock = string.Empty;
            NewMinStock = string.Empty;
            NewUnit = null;
            NewLocation = string.Empty;
            NewBranch = null;
            NewStatus = null;
            NewDescription = string.Empty;

            NewDonorCompany = string.Empty;
            NewDonorType = null;
            NewDonationDate = DateTime.Now;
            NewDonatedProduct = string.Empty;
            NewPresentation = string.Empty;
            NewDonationQuantity = string.Empty;
            NewUnitPrice = string.Empty;
            NewReceiptFolio = string.Empty;
            NewDonationNotes = string.Empty;

            NewChildName = string.Empty;
            NewAge = string.Empty;
            NewCurp = string.Empty;
            NewCancerType = string.Empty;
            NewGuardians = string.Empty;
            NewRelationship = string.Empty;
            NewPhones = string.Empty;
            NewEmails = string.Empty;
            NewCity = string.Empty;
            NewState = string.Empty;
            NewPatientStatus = null;
            NewPatientNotes = string.Empty;

            NewTripChild = string.Empty;
            NewTripGuardian = string.Empty;
            NewOrigin = string.Empty;
            NewDestination = string.Empty;
            NewTransportType = null;
            NewDepartureDate = DateTime.Now;
            NewReturnDate = DateTime.Now;
            NewTripStatus = null;
            NewTripNotes = string.Empty;
        }

        private async Task SaveItemAsync()
        {
            await Task.Delay(500);
        }

        private async Task SaveDonationAsync()
        {
            await Task.Delay(500);
        }

        private async Task SavePatientAsync()
        {
            await Task.Delay(500);
        }

        private async Task SaveTripAsync()
        {
            await Task.Delay(500);
        }

        // -------------------------------------------------------------
        // IMPLEMENTACIÓN DE INotifyPropertyChanged
        // -------------------------------------------------------------
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}