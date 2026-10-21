using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Inventory.Interfaces;
using AMANC_Inventory.Inventory.Models;
using AMANC_Inventory.Inventory.Services;
using AMANC_Inventory.Users.Models;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
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

        // --- SKU: Mínimo 8, máximo 12 caracteres, mayúsculas, sin espacios ni símbolos (excepto guión) ---
        private string? _newSKU;
        public string? NewSKU
        {
            get => _newSKU;
            set
            {
                if (value == null) { SetProperty(ref _newSKU, value); return; }
                // Elimina lo no permitido y convierte automáticamente a MAYÚSCULAS
                string sanitized = Regex.Replace(value, @"[^a-zA-Z0-9-]", "").ToUpper();
                if (sanitized.Length > 12) sanitized = sanitized.Substring(0, 12);
                SetProperty(ref _newSKU, sanitized);
            }
        }

        // --- Código de Barras: Exactamente 13 números, sin espacios ni símbolos ---
        private string? _newBarcode;
        public string? NewBarcode
        {
            get => _newBarcode;
            set
            {
                if (value == null) { SetProperty(ref _newBarcode, value); return; }
                string sanitized = Regex.Replace(value, @"[^\d]", "");
                if (sanitized.Length > 13) sanitized = sanitized.Substring(0, 13);
                SetProperty(ref _newBarcode, sanitized);
            }
        }

        // --- Nombre: No permite escribir números ---
        private string? _newName;
        public string? NewName
        {
            get => _newName;
            set
            {
                if (value == null) { SetProperty(ref _newName, value); return; }
                string sanitized = Regex.Replace(value, @"[\d]", "");
                SetProperty(ref _newName, sanitized);
            }
        }

        private string? _newCategory;
        public string? NewCategory { get => _newCategory; set => SetProperty(ref _newCategory, value); }

        // --- Stock Inicial: Solo números ---
        private string? _newStock;
        public string? NewStock
        {
            get => _newStock;
            set
            {
                if (value == null) { SetProperty(ref _newStock, value); return; }
                string sanitized = Regex.Replace(value, @"[^\d]", "");
                SetProperty(ref _newStock, sanitized);
            }
        }

        // --- Stock Mínimo: Solo números ---
        private string? _newMinStock;
        public string? NewMinStock
        {
            get => _newMinStock;
            set
            {
                if (value == null) { SetProperty(ref _newMinStock, value); return; }
                string sanitized = Regex.Replace(value, @"[^\d]", "");
                SetProperty(ref _newMinStock, sanitized);
            }
        }

        private string? _newUnit;
        public string? NewUnit { get => _newUnit; set => SetProperty(ref _newUnit, value); }

        // --- Valor Estimado Unitario: Solo números con prefijo '$ ' ---
        private string? _newEstimatedValue;
        public string? NewEstimatedValue
        {
            get => _newEstimatedValue;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    SetProperty(ref _newEstimatedValue, string.Empty);
                    return;
                }

                string digitsOnly = Regex.Replace(value, @"[^\d]", "");
                if (!string.IsNullOrEmpty(digitsOnly))
                {
                    SetProperty(ref _newEstimatedValue, $"$ {digitsOnly}");
                }
                else
                {
                    SetProperty(ref _newEstimatedValue, string.Empty);
                }
            }
        }

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
            // Validar Nombre
            if (string.IsNullOrWhiteSpace(NewName))
            {
                MessageBox.Show("Por favor, ingrese el nombre del producto.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar SKU (Mínimo 8 y Máximo 12 caracteres)
            string sku = NewSKU?.Trim().ToUpper() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(sku) || sku.Length < 8 || sku.Length > 12)
            {
                MessageBox.Show("El SKU debe tener entre 8 y 12 caracteres (solo mayúsculas, números y guión '-').", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar Código de Barras (Exactamente 13 dígitos si se proporciona)
            string barcode = NewBarcode?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(barcode) && barcode.Length != 13)
            {
                MessageBox.Show("El Código de Barras debe contener exactamente 13 caracteres numéricos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar Stock Inicial
            if (string.IsNullOrWhiteSpace(NewStock) || !int.TryParse(NewStock, out int stockResult))
            {
                MessageBox.Show("Por favor, ingrese una cantidad válida para el Stock Inicial.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int.TryParse(NewMinStock, out int minStockResult);

            string cleanEstimatedValue = Regex.Replace(NewEstimatedValue ?? "", @"[^\d]", "");
            decimal.TryParse(cleanEstimatedValue, out decimal estimatedValueResult);

            var newItem = new ProductModel
            {
                SKU = sku,
                Barcode = string.IsNullOrWhiteSpace(barcode) ? null : barcode,
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