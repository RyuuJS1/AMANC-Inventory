using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Inventory.Interfaces;
using AMANC_Inventory.Inventory.Models;
using AMANC_Inventory.Pantries.Interfaces;
using AMANC_Inventory.Pantries.Models;
using AMANC_Inventory.Pantries.Services;
using AMANC_Inventory.Patients.Interfaces;
using AMANC_Inventory.Patients.Services;
using AMANC_Inventory.Users.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AMANC_Inventory.Pantries.ViewModels
{
    public class PantryRegisterViewModel : ViewModelBase
    {
        private readonly IPantryService _pantryService;
        private readonly IPatientService _patientService;
        private readonly IProductService _productService;
        private readonly Func<Task>? _onSaveSuccess;

        private UserModel? _currentUser;
        private string? _userDefaultBranch;

        // ==========================================
        // SECCIÓN 1: DATOS DE ENTREGA Y FIRMA
        // ==========================================
        private string? _newPantryDestination;
        public string? NewPantryDestination { get => _newPantryDestination; set => SetProperty(ref _newPantryDestination, value); }

        private DateTime? _newPantryDate = DateTime.Now;
        public DateTime? NewPantryDate { get => _newPantryDate; set => SetProperty(ref _newPantryDate, value); }

        private string? _signatureBase64 = string.Empty;
        public string? SignatureBase64
        {
            get => _signatureBase64;
            set
            {
                if (SetProperty(ref _signatureBase64, value))
                {
                    OnPropertyChanged(nameof(HasSignature));
                    OnPropertyChanged(nameof(SignatureStatusText));
                }
            }
        }

        public bool HasSignature => !string.IsNullOrWhiteSpace(SignatureBase64);
        public string SignatureStatusText => HasSignature ? "✓ Firma Capturada" : "✍ Capturar Firma Electrónica";

        private string? _newPantryNotes;
        public string? NewPantryNotes { get => _newPantryNotes; set => SetProperty(ref _newPantryNotes, value); }

        // ==========================================
        // SECCIÓN 2: BUSCADOR DE PRODUCTOS Y CAMPOS TEMPORALES
        // ==========================================
        private ProductModel? _selectedProduct;
        public ProductModel? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetProperty(ref _selectedProduct, value))
                {
                    if (value != null)
                    {
                        TempUnit = !string.IsNullOrWhiteSpace(value.Unit) ? value.Unit : "Pza";
                        TempUnitPrice = value.EstimatedValue > 0 ? value.EstimatedValue.ToString("C2") : "$0.00";
                        TempQuantity = "1";
                    }
                    else
                    {
                        TempUnit = string.Empty;
                        TempUnitPrice = string.Empty;
                        TempQuantity = string.Empty;
                    }
                }
            }
        }

        private string? _tempUnit;
        public string? TempUnit { get => _tempUnit; set => SetProperty(ref _tempUnit, value); }

        private string? _tempUnitPrice;
        public string? TempUnitPrice { get => _tempUnitPrice; set => SetProperty(ref _tempUnitPrice, value); }

        private string? _tempQuantity = "1";
        public string? TempQuantity { get => _tempQuantity; set => SetProperty(ref _tempQuantity, value); }

        private decimal _totalPantryAmount;
        public decimal TotalPantryAmount
        {
            get => _totalPantryAmount;
            set => SetProperty(ref _totalPantryAmount, value);
        }

        // ==========================================
        // COLECCIONES Y COMANDOS
        // ==========================================
        public ObservableCollection<string> DestinationsList { get; } = new();
        public ObservableCollection<ProductModel> FilteredProductsList { get; } = new();
        public ObservableCollection<PantryItemModel> PantryItemsList { get; } = new();

        public ICommand AddItemCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand CaptureSignatureCommand { get; }
        public ICommand SavePantryCommand { get; }
        public ICommand ResetFormCommand { get; }

        public PantryRegisterViewModel(
            IPantryService? pantryService = null,
            IPatientService? patientService = null,
            IProductService? productService = null,
            Func<Task>? onSaveSuccess = null)
        {
            _pantryService = pantryService ?? new PantryService();
            _patientService = patientService ?? new PatientService();
            _productService = productService ?? new Inventory.Services.ProductService();
            _onSaveSuccess = onSaveSuccess;

            _ = LoadDestinationsAsync();
            _ = LoadExistingProductsAsync();

            AddItemCommand = new RelayCommand(_ => AddItem());
            RemoveItemCommand = new RelayCommand(param => RemoveItem(param as PantryItemModel));
            CaptureSignatureCommand = new RelayCommand(_ => CaptureSignature());
            SavePantryCommand = new RelayCommand(async _ => await SavePantryAsync());
            ResetFormCommand = new RelayCommand(_ => ResetForm());
        }

        public void SetCurrentUser(UserModel? user)
        {
            _currentUser = user;
            if (user != null && !string.IsNullOrWhiteSpace(user.Branch))
            {
                _userDefaultBranch = user.Branch;
            }
        }

        public async Task LoadDestinationsAsync()
        {
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();

                ExecuteOnUIThread(() =>
                {
                    DestinationsList.Clear();
                    if (patients != null && patients.Any())
                    {
                        var names = patients
                            .Where(p => !string.IsNullOrWhiteSpace(p.ChildName))
                            .Select(p => p.ChildName.Trim())
                            .Distinct()
                            .OrderBy(name => name);

                        foreach (var name in names)
                        {
                            DestinationsList.Add(name);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error al cargar beneficiarios]: {ex.Message}");
            }
        }

        public async Task LoadExistingProductsAsync()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();

                ExecuteOnUIThread(() =>
                {
                    FilteredProductsList.Clear();
                    if (products != null && products.Any())
                    {
                        foreach (var product in products.OrderBy(p => p.Name))
                        {
                            FilteredProductsList.Add(product);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error al cargar productos en despensas]: {ex.Message}");
            }
        }

        private void AddItem()
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Por favor, seleccione un producto del catálogo registrado.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TempQuantity, out int qty) || qty <= 0)
            {
                MessageBox.Show("Ingrese una cantidad válida mayor a cero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newItem = new PantryItemModel
            {
                ProductId = SelectedProduct.Id,
                ProductName = SelectedProduct.Name,
                Unit = !string.IsNullOrWhiteSpace(SelectedProduct.Unit) ? SelectedProduct.Unit : "Pza",
                Quantity = qty,
                UnitPrice = SelectedProduct.EstimatedValue
            };

            PantryItemsList.Add(newItem);
            RecalculateTotal();

            SelectedProduct = null;
            TempQuantity = "1";
        }

        private void RemoveItem(PantryItemModel? item)
        {
            if (item != null && PantryItemsList.Contains(item))
            {
                PantryItemsList.Remove(item);
                RecalculateTotal();
            }
        }

        private void RecalculateTotal()
        {
            TotalPantryAmount = PantryItemsList.Sum(i => i.Subtotal);
        }

        private void CaptureSignature()
        {
            if (HasSignature)
            {
                var result = MessageBox.Show("Ya existe una firma registrada. ¿Desea borrarla y volver a firmar?", "Firma Electrónica", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    SignatureBase64 = string.Empty;
                }
                return;
            }

            SignatureBase64 = $"SIG_{DateTime.Now:yyyyMMddHHmmss}_ACCEPTED";
            MessageBox.Show("Firma electrónica registrada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ResetForm()
        {
            NewPantryDestination = null;
            NewPantryDate = DateTime.Now;
            SignatureBase64 = string.Empty;
            NewPantryNotes = string.Empty;

            SelectedProduct = null;
            TempQuantity = "1";
            TempUnit = string.Empty;
            TempUnitPrice = string.Empty;

            PantryItemsList.Clear();
            TotalPantryAmount = 0;
        }

        public async Task SavePantryAsync()
        {
            if (string.IsNullOrWhiteSpace(NewPantryDestination))
            {
                MessageBox.Show("Por favor, seleccione un beneficiario o ingrese un destino / zona.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!PantryItemsList.Any())
            {
                MessageBox.Show("Debe agregar al menos un producto a la despensa antes de guardar.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var pantry = new PantryModel
            {
                Destination = NewPantryDestination.Trim(),
                DeliveryDate = NewPantryDate ?? DateTime.Now,
                DeliveredBy = _currentUser?.Name ?? "Sistema",
                Branch = _userDefaultBranch ?? "Sede Central",
                SignatureBase64 = SignatureBase64 ?? string.Empty,
                Notes = NewPantryNotes ?? string.Empty,
                Items = PantryItemsList.ToList()
            };

            bool success = await _pantryService.SavePantryAsync(pantry);
            if (success)
            {
                MessageBox.Show("Entrega de despensa registrada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetForm();
                if (_onSaveSuccess != null) await _onSaveSuccess.Invoke();
            }
            else
            {
                MessageBox.Show("Error al guardar la despensa en la base de datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
    }
}