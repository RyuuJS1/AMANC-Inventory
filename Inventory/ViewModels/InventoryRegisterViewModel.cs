using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Donations.Interfaces;
using AMANC_Inventory.Donations.Models;
using AMANC_Inventory.Donations.Services;
using AMANC_Inventory.Inventory.Interfaces;
using AMANC_Inventory.Inventory.Models;
using AMANC_Inventory.Inventory.Services;
using AMANC_Inventory.Patients.Models;
using AMANC_Inventory.Patients.Services;
using AMANC_Inventory.Trips.Models;
using AMANC_Inventory.Trips.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AMANC_Inventory.Inventory.ViewModels
{
    public class InventoryRegisterViewModel : INotifyPropertyChanged
    {
        private readonly IProductService _productService;
        private readonly IDonationService _donationService;
        private readonly IPatientService _patientService;
        private readonly ITripService _tripService;
        private readonly Func<Task>? _onSaveSuccess;

        // -------------------------------------------------------------
        // ESTADOS DE LAS PESTAÑAS (TAB SELECTION)
        // -------------------------------------------------------------

        private string? _userDefaultBranch;
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

        private string? _newUnit;
        public string? NewUnit { get => _newUnit; set { _newUnit = value; OnPropertyChanged(); } }
        private string? _newPrice;
        public string? NewPrice
        {
            get => _newPrice;
            set { _newPrice = value; OnPropertyChanged(); }
        }

        private string? _newLocation;
        public string? NewLocation { get => _newLocation; set { _newLocation = value; OnPropertyChanged(); } }

        private string? _newBranch;
        public string? NewBranch { get => _newBranch; set { _newBranch = value; OnPropertyChanged(); } }

        public void SetUserBranch(string? branch)
        {
            _userDefaultBranch = branch;
            NewBranch = branch;
        }

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
        // COMANDOS
        // -------------------------------------------------------------
        public ICommand ResetFormCommand { get; }
        public ICommand SaveItemCommand { get; }
        public ICommand SaveDonationCommand { get; }
        public ICommand SavePatientCommand { get; }
        public ICommand SaveTripCommand { get; }

        public InventoryRegisterViewModel(
            IProductService? productService = null,
            IDonationService? donationService = null,
            IPatientService? patientService = null,
            ITripService? tripService = null,
            Func<Task>? onSaveSuccess = null)
        {
            _productService = productService ?? new ProductService();
            _donationService = donationService ?? new DonationService();
            _patientService = patientService ?? new PatientService();
            _tripService = tripService ?? new TripService();
            _onSaveSuccess = onSaveSuccess;

            IsInventoryTabSelected = true;
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

        private void ResetForm()
        {
            NewCode = string.Empty;
            NewName = string.Empty;
            NewCategory = null;
            NewStock = string.Empty;
            NewPrice = string.Empty;
            NewUnit = null;
            NewLocation = string.Empty;
            NewBranch = _userDefaultBranch;

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
            if (string.IsNullOrWhiteSpace(NewCode) || string.IsNullOrWhiteSpace(NewName))
            {
                MessageBox.Show("Por favor, ingrese al menos el código y el nombre del producto.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int.TryParse(NewStock, out int stockResult);
            decimal.TryParse(NewPrice, out decimal priceResult);

            var newItem = new InventoryItemModel
            {
                Code = NewCode ?? string.Empty,
                Name = NewName ?? string.Empty,
                Category = NewCategory ?? string.Empty,
                Stock = stockResult,
                Price = priceResult,
                Unit = NewUnit ?? string.Empty,
                Location = NewLocation ?? string.Empty,
                Branch = NewBranch ?? string.Empty,
                Status = "Disponible"
            };

            bool success = await _productService.SaveProductAsync(newItem);
            if (success)
            {
                MessageBox.Show("Producto guardado correctamente en Firebase.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetForm();
                if (_onSaveSuccess != null) await _onSaveSuccess.Invoke();
            }
            else
            {
                MessageBox.Show("Error al guardar el producto en la base de datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SaveDonationAsync()
        {
            if (string.IsNullOrWhiteSpace(NewDonorCompany) || string.IsNullOrWhiteSpace(NewDonatedProduct))
            {
                MessageBox.Show("Por favor, ingrese el donante y el producto donado.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int.TryParse(NewDonationQuantity, out int quantity);
            decimal.TryParse(NewUnitPrice, out decimal unitPrice);

            var donation = new DonationModel
            {
                CompanyName = NewDonorCompany.Trim(),
                DonationDate = NewDonationDate ?? DateTime.Now,
                ProductName = NewDonatedProduct.Trim(),
                Presentation = NewPresentation ?? string.Empty,
                Branch = NewBranch ?? string.Empty,
                Quantity = quantity,
                UnitPrice = unitPrice
            };

            bool success = await _donationService.SaveDonationAsync(donation);
            if (success)
            {
                MessageBox.Show("Donación registrada correctamente en Firebase.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetForm();
                if (_onSaveSuccess != null) await _onSaveSuccess.Invoke();
            }
            else
            {
                MessageBox.Show("Error al guardar la donación en la base de datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SavePatientAsync()
        {
            if (string.IsNullOrWhiteSpace(NewChildName) || string.IsNullOrWhiteSpace(NewCancerType))
            {
                MessageBox.Show("Por favor, ingrese el nombre del beneficiario y el diagnóstico.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int.TryParse(NewAge, out int age);

            var patient = new PatientModel
            {
                ChildName = NewChildName.Trim(),
                Curp = NewCurp ?? string.Empty,
                CancerType = NewCancerType.Trim(),
                Guardians = NewGuardians ?? string.Empty,
                Relationship = NewRelationship ?? string.Empty,
                Phones = NewPhones ?? string.Empty,
                Emails = NewEmails ?? string.Empty,
                City = NewCity ?? string.Empty,
                State = NewState ?? string.Empty,
                Status = NewPatientStatus ?? "Activo",
                Notes = NewPatientNotes ?? string.Empty
            };

            bool success = await _patientService.SavePatientAsync(patient);
            if (success)
            {
                MessageBox.Show("Beneficiario registrado correctamente en Firebase.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetForm();
                if (_onSaveSuccess != null) await _onSaveSuccess.Invoke();
            }
            else
            {
                MessageBox.Show("Error al guardar el beneficiario en la base de datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SaveTripAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTripChild) || string.IsNullOrWhiteSpace(NewOrigin) || string.IsNullOrWhiteSpace(NewDestination))
            {
                MessageBox.Show("Por favor, ingrese el paciente, origen y destino del viaje.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var trip = new TripModel
            {
                ChildName = NewTripChild.Trim(),
                GuardianName = NewTripGuardian ?? string.Empty,
                Origin = NewOrigin.Trim(),
                Destination = NewDestination.Trim(),
                TransportType = NewTransportType ?? string.Empty,
                DepartureDate = NewDepartureDate ?? DateTime.Now,
                ArrivalDate = NewReturnDate ?? DateTime.Now,
                Status = NewTripStatus ?? "Programado",
                Notes = NewTripNotes ?? string.Empty
            };

            bool success = await _tripService.SaveTripAsync(trip);
            if (success)
            {
                MessageBox.Show("Traslado registrado correctamente en Firebase.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetForm();
                if (_onSaveSuccess != null) await _onSaveSuccess.Invoke();
            }
            else
            {
                MessageBox.Show("Error al guardar el traslado en la base de datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}