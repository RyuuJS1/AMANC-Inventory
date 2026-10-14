using AMANC_Inventory.Core.Architecture;
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
        private readonly Func<Task>? _onSaveSuccess;

        private UserModel? _currentUser;
        private string? _userDefaultBranch;

        // Propiedades del Formulario
        private string? _newPantryDestination;
        public string? NewPantryDestination { get => _newPantryDestination; set => SetProperty(ref _newPantryDestination, value); }

        private string? _newPantryProduct;
        public string? NewPantryProduct { get => _newPantryProduct; set => SetProperty(ref _newPantryProduct, value); }

        private string? _newPantryQuantity;
        public string? NewPantryQuantity { get => _newPantryQuantity; set => SetProperty(ref _newPantryQuantity, value); }

        private string? _newPantryUnit;
        public string? NewPantryUnit { get => _newPantryUnit; set => SetProperty(ref _newPantryUnit, value); }

        private DateTime? _newPantryDate = DateTime.Now;
        public DateTime? NewPantryDate { get => _newPantryDate; set => SetProperty(ref _newPantryDate, value); }

        private string? _newPantryNotes;
        public string? NewPantryNotes { get => _newPantryNotes; set => SetProperty(ref _newPantryNotes, value); }

        // Colecciones para Combos
        public ObservableCollection<string> DestinationsList { get; } = new();
        public ObservableCollection<string> UnitsList { get; } = new();

        // Comandos
        public ICommand SavePantryCommand { get; }
        public ICommand ResetFormCommand { get; }

        public PantryRegisterViewModel(
            IPantryService? pantryService = null,
            IPatientService? patientService = null,
            Func<Task>? onSaveSuccess = null)
        {
            _pantryService = pantryService ?? new PantryService();
            _patientService = patientService ?? new PatientService();
            _onSaveSuccess = onSaveSuccess;

            LoadCatalogDefaults();
            _ = LoadDestinationsAsync();

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

        private void LoadCatalogDefaults()
        {
            UnitsList.Clear();
            UnitsList.Add("Pza");
            UnitsList.Add("Caja");
            UnitsList.Add("Kg");
            UnitsList.Add("Litro");
            UnitsList.Add("Paquete");
        }

        public async Task LoadDestinationsAsync()
        {
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();

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
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error al cargar niños para el combo de despensas]: {ex.Message}");
            }
        }

        public void ResetForm()
        {
            NewPantryDestination = null;
            NewPantryProduct = string.Empty;
            NewPantryQuantity = string.Empty;
            NewPantryUnit = null;
            NewPantryDate = DateTime.Now;
            NewPantryNotes = string.Empty;
        }

        public async Task SavePantryAsync()
        {
            if (string.IsNullOrWhiteSpace(NewPantryDestination) || string.IsNullOrWhiteSpace(NewPantryProduct))
            {
                MessageBox.Show("Por favor, ingrese el destino/beneficiario y el producto entregado.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int.TryParse(NewPantryQuantity, out int quantity);

            var pantry = new PantryModel
            {
                Destination = NewPantryDestination.Trim(),
                ProductName = NewPantryProduct.Trim(),
                Quantity = quantity > 0 ? quantity : 1,
                Unit = NewPantryUnit ?? "Pza",
                DeliveryDate = NewPantryDate ?? DateTime.Now,
                DeliveredBy = _currentUser?.Name ?? "Sistema",
                Branch = _userDefaultBranch ?? "Sede Central",
                Notes = NewPantryNotes ?? string.Empty
            };

            bool success = await _pantryService.SavePantryAsync(pantry);
            if (success)
            {
                MessageBox.Show("Entrega de despensa registrada correctamente en Firebase.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetForm();
                if (_onSaveSuccess != null) await _onSaveSuccess.Invoke();
            }
            else
            {
                MessageBox.Show("Error al guardar la despensa en la base de datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}