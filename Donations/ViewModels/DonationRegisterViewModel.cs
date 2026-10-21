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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AMANC_Inventory.Donations.ViewModels
{
    public class DonationRegisterViewModel : ViewModelBase
    {
        private readonly IDonationService _donationService;
        private readonly IProductService _productService;
        private readonly Func<Task>? _onSaveSuccess;

        private UserModel? _currentUser;
        private string? _userDefaultBranch;

        // --- Datos del Donante ---
        private string? _newDonorCompany;
        public string? NewDonorCompany { get => _newDonorCompany; set => SetProperty(ref _newDonorCompany, value); }

        private string? _newDonorType = "Empresa";
        public string? NewDonorType { get => _newDonorType; set => SetProperty(ref _newDonorType, value); }

        private DateTime? _newDonationDate = DateTime.Now;
        public DateTime? NewDonationDate { get => _newDonationDate; set => SetProperty(ref _newDonationDate, value); }

        private string? _newReceiptFolio;
        public string? NewReceiptFolio { get => _newReceiptFolio; set => SetProperty(ref _newReceiptFolio, value); }

        private string? _newBranchId;
        public string? NewBranchId { get => _newBranchId; set => SetProperty(ref _newBranchId, value); }

        // --- Buscador y Producto Seleccionado ---
        private string _productSearchText = string.Empty;
        public string ProductSearchText
        {
            get => _productSearchText;
            set
            {
                if (SetProperty(ref _productSearchText, value))
                {
                    FilterProducts(value);
                }
            }
        }

        private ProductModel? _selectedProduct;
        public ProductModel? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetProperty(ref _selectedProduct, value) && value != null)
                {
                    // 1. Mantener el nombre del producto en el campo de búsqueda
                    _productSearchText = value.Name;
                    OnPropertyChanged(nameof(ProductSearchText));

                    // 2. Cargar unidad y valor estimado
                    TempPresentation = value.Unit;
                    TempUnitPrice = value.EstimatedValue.ToString("C");

                    // 3. Cargar automáticamente la cantidad (Stock registrado) sin escribir
                    TempQuantity = value.Stock > 0 ? value.Stock.ToString() : "1";
                }
            }
        }

        private string? _tempPresentation;
        public string? TempPresentation { get => _tempPresentation; set => SetProperty(ref _tempPresentation, value); }

        private string? _tempQuantity;
        public string? TempQuantity { get => _tempQuantity; set => SetProperty(ref _tempQuantity, value); }

        private string? _tempUnitPrice;
        public string? TempUnitPrice { get => _tempUnitPrice; set => SetProperty(ref _tempUnitPrice, value); }

        // Colecciones de Productos
        public ObservableCollection<ProductModel> AllProductsList { get; } = new();
        public ObservableCollection<ProductModel> FilteredProductsList { get; } = new();
        public ObservableCollection<DonationItemModel> DonationItemsList { get; } = new();

        public decimal TotalDonationAmount => DonationItemsList.Sum(i => i.TotalAmount);

        public ObservableCollection<string> DonorTypesList { get; } = new();
        public ObservableCollection<string> BranchesList { get; } = new();

        // Comandos
        public ICommand AddItemCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand SaveDonationCommand { get; }
        public ICommand ResetFormCommand { get; }

        public DonationRegisterViewModel(
            IDonationService? donationService = null,
            IProductService? productService = null,
            Func<Task>? onSaveSuccess = null)
        {
            _donationService = donationService ?? new DonationService();
            _productService = productService ?? new ProductService();
            _onSaveSuccess = onSaveSuccess;

            LoadCatalogDefaults();
            _ = LoadExistingProductsAsync();

            AddItemCommand = new RelayCommand(_ => AddItemToList());
            RemoveItemCommand = new RelayCommand(param => RemoveItemFromList(param as DonationItemModel));
            SaveDonationCommand = new RelayCommand(async _ => await SaveDonationAsync());
            ResetFormCommand = new RelayCommand(_ => ResetForm());
        }

        public async Task LoadExistingProductsAsync()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                AllProductsList.Clear();
                FilteredProductsList.Clear();

                if (products != null)
                {
                    foreach (var p in products)
                    {
                        AllProductsList.Add(p);
                        FilteredProductsList.Add(p);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar catálogo de productos en donaciones: {ex.Message}");
            }
        }

        private void FilterProducts(string searchText)
        {
            FilteredProductsList.Clear();
            var query = AllProductsList.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                string clean = searchText.Trim();
                query = query.Where(p =>
                    (!string.IsNullOrEmpty(p.Name) && p.Name.Contains(clean, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.SKU) && p.SKU.Contains(clean, StringComparison.OrdinalIgnoreCase))
                );
            }

            foreach (var p in query)
            {
                FilteredProductsList.Add(p);
            }
        }

        public void SetCurrentUser(UserModel? user)
        {
            _currentUser = user;
            if (user != null && !string.IsNullOrWhiteSpace(user.Branch))
            {
                _userDefaultBranch = user.Branch;
                NewBranchId = user.Branch;
            }
        }

        private void LoadCatalogDefaults()
        {
            DonorTypesList.Clear();
            DonorTypesList.Add("Particular");
            DonorTypesList.Add("Empresa");
            DonorTypesList.Add("Fundación");

            BranchesList.Clear();
            BranchesList.Add("Veracruz");
            BranchesList.Add("CDMX");
            BranchesList.Add("Sede Central");
            BranchesList.Add("Sucursal Norte");
            BranchesList.Add("Sucursal Sur");
        }

        private void AddItemToList()
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Por favor, busque y seleccione un producto existente del catálogo.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TempQuantity, out int qty) || qty <= 0)
            {
                MessageBox.Show("Ingrese una cantidad válida mayor a cero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var item = new DonationItemModel
            {
                ProductId = SelectedProduct.Id,
                ProductName = SelectedProduct.Name,
                Presentation = SelectedProduct.Unit,
                Quantity = qty,
                UnitPrice = SelectedProduct.EstimatedValue
            };

            DonationItemsList.Add(item);
            OnPropertyChanged(nameof(TotalDonationAmount));

            // Limpiar selección temporal
            SelectedProduct = null;
            ProductSearchText = string.Empty;
            TempPresentation = string.Empty;
            TempUnitPrice = string.Empty;
            TempQuantity = string.Empty;
        }

        private void RemoveItemFromList(DonationItemModel? item)
        {
            if (item != null && DonationItemsList.Contains(item))
            {
                DonationItemsList.Remove(item);
                OnPropertyChanged(nameof(TotalDonationAmount));
            }
        }

        public void ResetForm()
        {
            NewDonorCompany = string.Empty;
            NewDonorType = "Empresa";
            NewDonationDate = DateTime.Now;
            NewReceiptFolio = string.Empty;
            NewBranchId = _userDefaultBranch;

            SelectedProduct = null;
            ProductSearchText = string.Empty;
            TempPresentation = string.Empty;
            TempUnitPrice = string.Empty;
            TempQuantity = string.Empty;

            DonationItemsList.Clear();
            OnPropertyChanged(nameof(TotalDonationAmount));
        }

        public async Task SaveDonationAsync()
        {
            if (string.IsNullOrWhiteSpace(NewDonorCompany))
            {
                MessageBox.Show("Por favor, ingrese el nombre del donante o empresa.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DonationItemsList.Any())
            {
                MessageBox.Show("Debe seleccionar al menos un producto existente para la donación.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var donation = new DonationModel
            {
                CompanyName = NewDonorCompany.Trim(),
                DonorType = NewDonorType ?? "Empresa",
                DonationDate = NewDonationDate ?? DateTime.Now,
                Branch = NewBranchId ?? _userDefaultBranch ?? string.Empty,
                ReceiptFolio = NewReceiptFolio?.Trim() ?? string.Empty,
                Items = DonationItemsList.ToList()
            };

            bool success = await _donationService.SaveDonationAsync(donation);
            if (success)
            {
                MessageBox.Show("Donación registrada correctamente y stock actualizado.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetForm();
                if (_onSaveSuccess != null) await _onSaveSuccess.Invoke();
            }
            else
            {
                MessageBox.Show("Error al registrar la donación.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}