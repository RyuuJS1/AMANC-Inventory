using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Donations.ViewModels;
using AMANC_Inventory.Inventory.Interfaces;
using AMANC_Inventory.Inventory.Models;
using AMANC_Inventory.Inventory.Services;
using AMANC_Inventory.Users.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        private DonationRegisterViewModel? _donationRegisterViewModel;

        private UserModel? _currentUser;
        private string? _userDefaultBranch;

        // --- SKU: Opcional (Mayúsculas, números y guión) ---
        private string? _newSKU;
        public string? NewSKU
        {
            get => _newSKU;
            set
            {
                if (value == null) { SetProperty(ref _newSKU, value); return; }
                string sanitized = Regex.Replace(value, @"[^a-zA-Z0-9-]", "").ToUpper();
                SetProperty(ref _newSKU, sanitized);
            }
        }

        // --- Código de Barras: OBLIGATORIO (Permite cualquier longitud de dígitos) ---
        private string? _newBarcode;
        public string? NewBarcode
        {
            get => _newBarcode;
            set
            {
                if (value == null) { SetProperty(ref _newBarcode, value); return; }
                string sanitized = Regex.Replace(value, @"[^\d]", "");
                SetProperty(ref _newBarcode, sanitized);
            }
        }

        // --- Nombre: OBLIGATORIO y automáticamente en MAYÚSCULAS ---
        private string? _newName;
        public string? NewName
        {
            get => _newName;
            set
            {
                if (value == null) { SetProperty(ref _newName, value); return; }
                SetProperty(ref _newName, value.ToUpper());
            }
        }

        // --- Categoría: OBLIGATORIA ---
        private string? _newCategory;
        public string? NewCategory { get => _newCategory; set => SetProperty(ref _newCategory, value); }

        // --- Stock Inicial: OBLIGATORIO ---
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

        // --- Stock Mínimo: OPCIONAL ---
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

        // --- Unidad de Medida (Presentación y Especificación) ---
        private string? _newUnit = "Pzas";
        public string? NewUnit { get => _newUnit; set => SetProperty(ref _newUnit, value); }

        private string? _newUnitDetail;
        public string? NewUnitDetail { get => _newUnitDetail; set => SetProperty(ref _newUnitDetail, value); }

        // --- Valor Estimado Unitario ---
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

        // --- Estatus: "Disponible" o "Agotado" (Default "Disponible") ---
        private string? _newStatus = "Disponible";
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
            Func<Task>? onSaveSuccess = null,
            DonationRegisterViewModel? donationRegisterViewModel = null)
        {
            _productService = productService ?? new ProductService();
            _onSaveSuccess = onSaveSuccess;
            _donationRegisterViewModel = donationRegisterViewModel;

            LoadCatalogDefaults();

            SaveProductCommand = new RelayCommand(async _ => await SaveProductAsync());
            ResetFormCommand = new RelayCommand(_ => ResetForm());
        }

        public void SetDonationRegisterViewModel(DonationRegisterViewModel donationRegister)
        {
            _donationRegisterViewModel = donationRegister;
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
            CategoriesList.Add("Limpieza");

            UnitsList.Clear();
            UnitsList.Add("Pzas");
            UnitsList.Add("Caja");
            UnitsList.Add("ml");
            UnitsList.Add("L");
            UnitsList.Add("g");
            UnitsList.Add("kg");
            UnitsList.Add("Frasco");
            UnitsList.Add("Paquete");
            UnitsList.Add("Lata");

            StatusList.Clear();
            StatusList.Add("Disponible");
            StatusList.Add("Agotado");
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
            NewUnit = "Pzas";
            NewUnitDetail = string.Empty;
            NewLocation = string.Empty;
            NewBranchId = _userDefaultBranch;
            NewStatus = "Disponible";
            NewExpirationDate = null;
        }

        public async Task SaveProductAsync()
        {
            // 1. Validar Código de Barras (Obligatorio)
            if (string.IsNullOrWhiteSpace(NewBarcode))
            {
                MessageBox.Show("Por favor, ingrese o escanee el Código de Barras.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Validar Nombre del Producto (Obligatorio)
            if (string.IsNullOrWhiteSpace(NewName))
            {
                MessageBox.Show("Por favor, ingrese el Nombre del Producto.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Validar Categoría (Obligatoria)
            if (string.IsNullOrWhiteSpace(NewCategory))
            {
                MessageBox.Show("Por favor, seleccione la Categoría del producto.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 4. Validar Stock Inicial (Obligatorio)
            if (string.IsNullOrWhiteSpace(NewStock) || !int.TryParse(NewStock, out int stockResult))
            {
                MessageBox.Show("Por favor, ingrese una cantidad válida para el Stock Inicial.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int.TryParse(NewMinStock, out int minStockResult);

            string cleanEstimatedValue = Regex.Replace(NewEstimatedValue ?? "", @"[^\d]", "");
            decimal.TryParse(cleanEstimatedValue, out decimal estimatedValueResult);

            string finalUnit = !string.IsNullOrWhiteSpace(NewUnitDetail)
                ? $"{NewUnitDetail.Trim()} {NewUnit}"
                : NewUnit ?? "Pzas";

            var newItem = new ProductModel
            {
                SKU = NewSKU?.Trim().ToUpper() ?? string.Empty,
                Barcode = NewBarcode.Trim(),
                BranchId = NewBranchId?.Trim() ?? _userDefaultBranch ?? "Sede Central",
                Name = NewName.Trim().ToUpper(),
                Category = NewCategory,
                Stock = stockResult,
                MinStock = minStockResult,
                Unit = finalUnit,
                EstimatedValue = estimatedValueResult,
                IsDonated = NewIsDonated,
                Location = NewLocation?.Trim() ?? string.Empty,
                Status = NewStatus ?? "Disponible",
                ExpirationDate = NewExpirationDate,
                CreatedAt = DateTime.UtcNow
            };

            bool success = await _productService.SaveProductAsync(newItem);
            if (success)
            {
                MessageBox.Show("Producto registrado correctamente en el inventario.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetForm();

                // RECARGA EN TIEMPO REAL: Refresca el catálogo del módulo de donaciones al instante
                if (_donationRegisterViewModel != null)
                {
                    await _donationRegisterViewModel.LoadExistingProductsAsync();
                }

                // RECARGA EN TIEMPO REAL: Refresca las tablas de consulta de productos
                if (_onSaveSuccess != null)
                {
                    await _onSaveSuccess.Invoke();
                }
            }
            else
            {
                MessageBox.Show("Error al guardar el producto en la base de datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}