using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Inventory.Interfaces;
using AMANC_Inventory.Inventory.Models;
using AMANC_Inventory.Inventory.Services;
using AMANC_Inventory.Users.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AMANC_Inventory.Inventory.ViewModels
{
    public class ProductRegisterViewModel : ViewModelBase
    {
        private readonly IProductService _productService;
        private readonly Func<Task>? _onSaveSuccess;

        private UserModel? _currentUser;
        private string? _userDefaultBranch;

        // Propiedades del Formulario
        private string? _newSKU;
        public string? NewSKU { get => _newSKU; set => SetProperty(ref _newSKU, value); }

        private string? _newBarcode;
        public string? NewBarcode { get => _newBarcode; set => SetProperty(ref _newBarcode, value); }

        private string? _newName;
        public string? NewName { get => _newName; set => SetProperty(ref _newName, value); }

        private string? _newCategory;
        public string? NewCategory { get => _newCategory; set => SetProperty(ref _newCategory, value); }

        private string? _newStock;
        public string? NewStock { get => _newStock; set => SetProperty(ref _newStock, value); }

        private string? _newMinStock;
        public string? NewMinStock { get => _newMinStock; set => SetProperty(ref _newMinStock, value); }

        private string? _newUnit;
        public string? NewUnit { get => _newUnit; set => SetProperty(ref _newUnit, value); }

        private string? _newEstimatedValue;
        public string? NewEstimatedValue { get => _newEstimatedValue; set => SetProperty(ref _newEstimatedValue, value); }

        private bool _newIsDonated = true;
        public bool NewIsDonated { get => _newIsDonated; set => SetProperty(ref _newIsDonated, value); }

        private string? _newLocation;
        public string? NewLocation { get => _newLocation; set => SetProperty(ref _newLocation, value); }

        private string? _newBranchId;
        public string? NewBranchId { get => _newBranchId; set => SetProperty(ref _newBranchId, value); }

        private string? _newStatus = "Active";
        public string? NewStatus { get => _newStatus; set => SetProperty(ref _newStatus, value); }

        private DateTime? _newExpirationDate;
        public DateTime? NewExpirationDate { get => _newExpirationDate; set => SetProperty(ref _newExpirationDate, value); }

        // Colecciones para Combos
        public ObservableCollection<string> CategoriesList { get; } = new();
        public ObservableCollection<string> UnitsList { get; } = new();
        public ObservableCollection<string> BranchesList { get; } = new();
        public ObservableCollection<string> StatusList { get; } = new();

        // Comandos
        public ICommand SaveProductCommand { get; }
        public ICommand ResetFormCommand { get; }

        public ProductRegisterViewModel(
            IProductService? productService = null,
            Func<Task>? onSaveSuccess = null)
        {
            _productService = productService ?? new ProductService();
            _onSaveSuccess = onSaveSuccess;

            LoadCatalogDefaults();

            SaveProductCommand = new RelayCommand(async _ => await SaveProductAsync());
            ResetFormCommand = new RelayCommand(_ => ResetForm());
        }

        public void SetCurrentUser(UserModel? user)
        {
            _currentUser = user;
            if (user != null && !string.IsNullOrWhiteSpace(user.Branch))
            {
                SetUserBranch(user.Branch);
            }
        }

        public void SetUserBranch(string? branch)
        {
            if (string.IsNullOrWhiteSpace(branch)) return;

            _userDefaultBranch = branch;
            NewBranchId = branch;

            if (!BranchesList.Contains(branch))
            {
                BranchesList.Add(branch);
            }
        }

        private void LoadCatalogDefaults()
        {
            BranchesList.Clear();
            BranchesList.Add("Veracruz");
            BranchesList.Add("CDMX");
            BranchesList.Add("Sede Central");
            BranchesList.Add("Sucursal Norte");
            BranchesList.Add("Sucursal Sur");

            CategoriesList.Clear();
            CategoriesList.Add("Medicamentos");
            CategoriesList.Add("Alimentos / Suplementos");
            CategoriesList.Add("Higiene Personal");
            CategoriesList.Add("Ropa");
            CategoriesList.Add("Insumo Médico");

            UnitsList.Clear();
            UnitsList.Add("Pza");
            UnitsList.Add("Caja");
            UnitsList.Add("Kg");
            UnitsList.Add("Litro");
            UnitsList.Add("Frasco");
            UnitsList.Add("Paquete");

            StatusList.Clear();
            StatusList.Add("Active");
            StatusList.Add("Inactive");
            StatusList.Add("Out of Stock");
            StatusList.Add("Expired");
            StatusList.Add("Damaged");
        }

        public void ResetForm()
        {
            NewSKU = string.Empty;
            NewBarcode = string.Empty;
            NewName = string.Empty;
            NewCategory = null;
            NewStock = string.Empty;
            NewMinStock = string.Empty;
            NewEstimatedValue = string.Empty;
            NewIsDonated = true;
            NewUnit = null;
            NewLocation = string.Empty;
            NewBranchId = _userDefaultBranch;
            NewStatus = "Active";
            NewExpirationDate = null;
        }

        public async Task SaveProductAsync()
        {
            if (string.IsNullOrWhiteSpace(NewName))
            {
                MessageBox.Show("Por favor, ingrese al menos el nombre del producto.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int.TryParse(NewStock, out int stockResult);
            int.TryParse(NewMinStock, out int minStockResult);
            decimal.TryParse(NewEstimatedValue, out decimal estimatedValueResult);

            var newItem = new ProductModel
            {
                SKU = NewSKU?.Trim() ?? string.Empty,
                Barcode = string.IsNullOrWhiteSpace(NewBarcode) ? null : NewBarcode.Trim(),
                BranchId = NewBranchId?.Trim() ?? _userDefaultBranch ?? string.Empty,
                Name = NewName.Trim(),
                Category = NewCategory ?? string.Empty,
                Stock = stockResult,
                MinStock = minStockResult,
                Unit = NewUnit ?? string.Empty,
                EstimatedValue = estimatedValueResult,
                IsDonated = NewIsDonated,
                Location = NewLocation?.Trim() ?? string.Empty,
                Status = NewStatus ?? "Active",
                ExpirationDate = NewExpirationDate,
                CreatedAt = DateTime.UtcNow
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
    }
}