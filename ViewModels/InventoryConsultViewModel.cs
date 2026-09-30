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
    public class InventoryConsultViewModel : ViewModelBase
    {
        private readonly IProductService? _productService;
        private bool _isLoading;
        private string _searchText = string.Empty;
        private string _filterText = string.Empty;

        // Pestañas de Consulta
        private bool _isInventoryTabSelected = true;
        private bool _isDonationsTabSelected;
        private bool _isPatientsTabSelected;
        private bool _isTripsTabSelected;
        private bool _isUsersTabSelected;

        public InventoryConsultViewModel() : this(null)
        {
        }

        public InventoryConsultViewModel(IProductService? productService)
        {
            _productService = productService;

            InventoryItems = new ObservableCollection<InventoryItemModel>();
            FilteredInventoryList = new ObservableCollection<InventoryItemModel>();
            FilteredDonationsList = new ObservableCollection<object>();
            FilteredPatientsList = new ObservableCollection<object>();
            FilteredTripsList = new ObservableCollection<object>();
            FilteredUsersList = new ObservableCollection<UserModel>();

            // Comandos
            LoadItemsCommand = new RelayCommand(async _ => await LoadProductsAsync());
            SelectTabCommand = new RelayCommand(p => SeleccionarPestana(p));
            ClearFilterCommand = new RelayCommand(_ => FilterText = string.Empty);

            _ = LoadProductsAsync();
        }

        #region Propiedades

        public ObservableCollection<InventoryItemModel> InventoryItems { get; }
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

        // --- Estados de las Pestañas ---

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

        #endregion
    }
}