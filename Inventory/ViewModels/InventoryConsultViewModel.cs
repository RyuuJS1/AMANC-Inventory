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
using AMANC_Inventory.Users.Interfaces;
using AMANC_Inventory.Users.Models;
using AMANC_Inventory.Users.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AMANC_Inventory.Inventory.ViewModels
{
    public class InventoryConsultViewModel : ViewModelBase
    {
        private readonly IProductService _productService;
        private readonly IDonationService _donationService;
        private readonly IPatientService _patientService;
        private readonly ITripService _tripService;
        private readonly IUserService _userService;

        private bool _isLoading;
        private string _filterText = string.Empty;

        private bool _isInventoryTabSelected = true;
        private bool _isDonationsTabSelected;
        private bool _isPatientsTabSelected;
        private bool _isTripsTabSelected;
        private bool _isUsersTabSelected;

        public InventoryConsultViewModel(
            IProductService? productService = null,
            IDonationService? donationService = null,
            IPatientService? patientService = null,
            ITripService? tripService = null,
            IUserService? userService = null)
        {
            _productService = productService ?? new ProductService();
            _donationService = donationService ?? new DonationService();
            _patientService = patientService ?? new PatientService();
            _tripService = tripService ?? new TripService();
            _userService = userService ?? new UserService();

            InventoryItems = new ObservableCollection<InventoryItemModel>();
            FilteredInventoryList = new ObservableCollection<InventoryItemModel>();

            DonationItems = new ObservableCollection<DonationModel>();
            FilteredDonationsList = new ObservableCollection<DonationModel>();

            PatientItems = new ObservableCollection<PatientModel>();
            FilteredPatientsList = new ObservableCollection<PatientModel>();

            TripItems = new ObservableCollection<TripModel>();
            FilteredTripsList = new ObservableCollection<TripModel>();

            UserItems = new ObservableCollection<UserModel>();
            FilteredUsersList = new ObservableCollection<UserModel>();

            LoadItemsCommand = new RelayCommand(async _ => await LoadAllDataAsync());
            SelectTabCommand = new RelayCommand(p => SeleccionarPestana(p));
            ClearFilterCommand = new RelayCommand(_ => FilterText = string.Empty);

            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                _ = LoadAllDataAsync();
            }
        }

        #region Colecciones

        public ObservableCollection<InventoryItemModel> InventoryItems { get; }
        public ObservableCollection<InventoryItemModel> FilteredInventoryList { get; }

        public ObservableCollection<DonationModel> DonationItems { get; }
        public ObservableCollection<DonationModel> FilteredDonationsList { get; }

        public ObservableCollection<PatientModel> PatientItems { get; }
        public ObservableCollection<PatientModel> FilteredPatientsList { get; }

        public ObservableCollection<TripModel> TripItems { get; }
        public ObservableCollection<TripModel> FilteredTripsList { get; }

        public ObservableCollection<UserModel> UserItems { get; }
        public ObservableCollection<UserModel> FilteredUsersList { get; }

        #endregion

        #region Propiedades de Estado y Pestañas

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
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

        #endregion

        #region Comandos

        public ICommand LoadItemsCommand { get; }
        public ICommand SelectTabCommand { get; }
        public ICommand ClearFilterCommand { get; }

        #endregion

        #region Métodos de Carga y Filtrado

        public void SeleccionarPestana(object? parameter)
        {
            if (parameter == null) return;
            string tab = parameter.ToString() ?? string.Empty;

            IsInventoryTabSelected = tab == "0" || tab.Equals("Inventory", StringComparison.OrdinalIgnoreCase);
            IsDonationsTabSelected = tab == "1" || tab.Equals("Donations", StringComparison.OrdinalIgnoreCase);
            IsPatientsTabSelected = tab == "2" || tab.Equals("Patients", StringComparison.OrdinalIgnoreCase);
            IsTripsTabSelected = tab == "3" || tab.Equals("Trips", StringComparison.OrdinalIgnoreCase);
            IsUsersTabSelected = tab == "4" || tab.Equals("Users", StringComparison.OrdinalIgnoreCase);

            AplicarFiltros();
        }

        public async Task LoadAllDataAsync()
        {
            IsLoading = true;
            try
            {
                // Carga simultánea de los 5 nodos de Firebase
                var productsTask = _productService.GetAllProductsAsync();
                var donationsTask = _donationService.GetAllDonationsAsync();
                var patientsTask = _patientService.GetAllPatientsAsync();
                var tripsTask = _tripService.GetAllTripsAsync();
                var usersTask = _userService.GetAllUsersAsync();

                await Task.WhenAll(productsTask, donationsTask, patientsTask, tripsTask, usersTask);

                ExecuteOnUIThread(() =>
                {
                    InventoryItems.Clear();
                    foreach (var p in productsTask.Result ?? Enumerable.Empty<InventoryItemModel>())
                        InventoryItems.Add(p);

                    DonationItems.Clear();
                    foreach (var d in donationsTask.Result ?? Enumerable.Empty<DonationModel>())
                        DonationItems.Add(d);

                    PatientItems.Clear();
                    foreach (var pa in patientsTask.Result ?? Enumerable.Empty<PatientModel>())
                        PatientItems.Add(pa);

                    TripItems.Clear();
                    foreach (var t in tripsTask.Result ?? Enumerable.Empty<TripModel>())
                        TripItems.Add(t);

                    UserItems.Clear();
                    foreach (var u in usersTask.Result ?? Enumerable.Empty<UserModel>())
                        UserItems.Add(u);

                    AplicarFiltros();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Error al cargar datos globales]: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AplicarFiltros()
        {
            ExecuteOnUIThread(() =>
            {
                // 1. Productos
                FilteredInventoryList.Clear();
                var invQuery = InventoryItems.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(FilterText))
                {
                    invQuery = invQuery.Where(i =>
                        (i.Name?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (i.Code?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (i.Category?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (i.Branch?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false)
                    );
                }
                foreach (var item in invQuery) FilteredInventoryList.Add(item);

                // 2. Donaciones
                FilteredDonationsList.Clear();
                var donQuery = DonationItems.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(FilterText))
                {
                    donQuery = donQuery.Where(d =>
                        (d.CompanyName?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (d.ProductName?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (d.Branch?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false)
                    );
                }
                foreach (var item in donQuery) FilteredDonationsList.Add(item);

                // 3. Niños / Beneficiarios
                FilteredPatientsList.Clear();
                var patQuery = PatientItems.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(FilterText))
                {
                    patQuery = patQuery.Where(p =>
                        (p.ChildName?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (p.Guardians?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (p.CancerType?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (p.City?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false)
                    );
                }
                foreach (var item in patQuery) FilteredPatientsList.Add(item);

                // 4. Traslados / Viajes
                FilteredTripsList.Clear();
                var tripQuery = TripItems.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(FilterText))
                {
                    tripQuery = tripQuery.Where(t =>
                        (t.ChildName?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (t.GuardianName?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (t.Origin?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (t.Destination?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false)
                    );
                }
                foreach (var item in tripQuery) FilteredTripsList.Add(item);

                // 5. Usuarios
                FilteredUsersList.Clear();
                var usrQuery = UserItems.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(FilterText))
                {
                    usrQuery = usrQuery.Where(u =>
                        (u.Name?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (u.Email?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (u.Branch?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (u.Department?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) ?? false)
                    );
                }
                foreach (var item in usrQuery) FilteredUsersList.Add(item);
            });
        }

        private void ExecuteOnUIThread(Action action)
        {
            if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(action);
            }
            else
            {
                action();
            }
        }

        #endregion
    }
}