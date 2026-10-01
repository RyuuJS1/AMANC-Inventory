using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Donations.Interfaces;
using AMANC_Inventory.Donations.Models;
using AMANC_Inventory.Donations.Services;
using AMANC_Inventory.Inventory.Interfaces;
using AMANC_Inventory.Inventory.Models;
using AMANC_Inventory.Inventory.Services;
using AMANC_Inventory.Users.Models;
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

        private bool _isLoading;
        private string _searchText = string.Empty;
        private string _filterText = string.Empty;

        private bool _isInventoryTabSelected = true;
        private bool _isDonationsTabSelected;
        private bool _isPatientsTabSelected;
        private bool _isTripsTabSelected;
        private bool _isUsersTabSelected;

        public InventoryConsultViewModel(IProductService? productService = null, IDonationService? donationService = null)
        {
            _productService = productService ?? new ProductService();
            _donationService = donationService ?? new DonationService();

            InventoryItems = new ObservableCollection<InventoryItemModel>();
            FilteredInventoryList = new ObservableCollection<InventoryItemModel>();

            DonationItems = new ObservableCollection<DonationModel>();
            FilteredDonationsList = new ObservableCollection<DonationModel>();

            FilteredPatientsList = new ObservableCollection<object>();
            FilteredTripsList = new ObservableCollection<object>();
            FilteredUsersList = new ObservableCollection<UserModel>();

            LoadItemsCommand = new RelayCommand(async _ => await LoadAllDataAsync());
            SelectTabCommand = new RelayCommand(p => SeleccionarPestana(p));
            ClearFilterCommand = new RelayCommand(_ => FilterText = string.Empty);

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                CargarDatosDiseno();
            }
            else
            {
                _ = LoadAllDataAsync();
            }
        }

        #region Propiedades

        public ObservableCollection<InventoryItemModel> InventoryItems { get; }
        public ObservableCollection<InventoryItemModel> FilteredInventoryList { get; }

        public ObservableCollection<DonationModel> DonationItems { get; }
        public ObservableCollection<DonationModel> FilteredDonationsList { get; }

        public ObservableCollection<object> FilteredPatientsList { get; }
        public ObservableCollection<object> FilteredTripsList { get; }
        public ObservableCollection<UserModel> FilteredUsersList { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
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

        #region Métodos

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
                var products = await _productService.GetAllProductsAsync();
                var donations = await _donationService.GetAllDonationsAsync();

                // Asegurar la actualización en el hilo principal de la interfaz
                ExecuteOnUIThread(() =>
                {
                    InventoryItems.Clear();
                    if (products != null)
                    {
                        foreach (var p in products) InventoryItems.Add(p);
                    }

                    DonationItems.Clear();
                    if (donations != null)
                    {
                        foreach (var d in donations) DonationItems.Add(d);
                    }

                    AplicarFiltros();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Error al cargar datos]: {ex.Message}");
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
                // 1. Filtrar Productos
                FilteredInventoryList.Clear();
                var inventoryQuery = InventoryItems.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(FilterText))
                {
                    inventoryQuery = inventoryQuery.Where(i =>
                        (i.Name != null && i.Name.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                        (i.Code != null && i.Code.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                        (i.Category != null && i.Category.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                        (i.Branch != null && i.Branch.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                        (i.Location != null && i.Location.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                        (i.Status != null && i.Status.Contains(FilterText, StringComparison.OrdinalIgnoreCase))
                    );
                }

                foreach (var item in inventoryQuery)
                {
                    FilteredInventoryList.Add(item);
                }

                // 2. Filtrar Donaciones (Actualizado con las propiedades reales del DonationModel)
                FilteredDonationsList.Clear();
                var donationsQuery = DonationItems.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(FilterText))
                {
                    donationsQuery = donationsQuery.Where(d =>
                        (d.CompanyName != null && d.CompanyName.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                        (d.ProductName != null && d.ProductName.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                        (d.Presentation != null && d.Presentation.Contains(FilterText, StringComparison.OrdinalIgnoreCase)) ||
                        (d.Branch != null && d.Branch.Contains(FilterText, StringComparison.OrdinalIgnoreCase))
                    );
                }

                foreach (var item in donationsQuery)
                {
                    FilteredDonationsList.Add(item);
                }
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

        private void CargarDatosDiseno()
        {
            InventoryItems.Add(new InventoryItemModel
            {
                Code = "MED-101",
                Name = "Amoxicilina 250mg",
                Category = "Medicamento",
                Stock = 45,
                Unit = "Cajas",
                Branch = "Sede Veracruz",
                Status = "Disponible"
            });

            DonationItems.Add(new DonationModel
            {
                CompanyName = "Farmacia San Jerónimo",
                ProductName = "Paquetes de Gasas",
                Quantity = 100,
                Presentation = "Material Médico",
                Branch = "Sede Veracruz",
            });

            AplicarFiltros();
        }

        #endregion
    }
}