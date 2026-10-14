using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Donations.Interfaces;
using AMANC_Inventory.Donations.Models;
using AMANC_Inventory.Donations.Services;
using AMANC_Inventory.Users.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AMANC_Inventory.Donations.ViewModels
{
    public class DonationRegisterViewModel : ViewModelBase
    {
        private readonly IDonationService _donationService;
        private readonly Func<Task>? _onSaveSuccess;

        private UserModel? _currentUser;
        private string? _userDefaultBranch;

        // Propiedades del Formulario
        private string? _newDonorCompany;
        public string? NewDonorCompany { get => _newDonorCompany; set => SetProperty(ref _newDonorCompany, value); }

        private string? _newDonorType;
        public string? NewDonorType { get => _newDonorType; set => SetProperty(ref _newDonorType, value); }

        private DateTime? _newDonationDate = DateTime.Now;
        public DateTime? NewDonationDate { get => _newDonationDate; set => SetProperty(ref _newDonationDate, value); }

        private string? _newDonatedProduct;
        public string? NewDonatedProduct { get => _newDonatedProduct; set => SetProperty(ref _newDonatedProduct, value); }

        private string? _newPresentation;
        public string? NewPresentation { get => _newPresentation; set => SetProperty(ref _newPresentation, value); }

        private string? _newDonationQuantity;
        public string? NewDonationQuantity { get => _newDonationQuantity; set => SetProperty(ref _newDonationQuantity, value); }

        private string? _newUnitPrice;
        public string? NewUnitPrice { get => _newUnitPrice; set => SetProperty(ref _newUnitPrice, value); }

        private string? _newReceiptFolio;
        public string? NewReceiptFolio { get => _newReceiptFolio; set => SetProperty(ref _newReceiptFolio, value); }

        private string? _newDonationNotes;
        public string? NewDonationNotes { get => _newDonationNotes; set => SetProperty(ref _newDonationNotes, value); }

        private string? _newBranchId;
        public string? NewBranchId { get => _newBranchId; set => SetProperty(ref _newBranchId, value); }

        // Colecciones para Combos
        public ObservableCollection<string> DonorTypesList { get; } = new();
        public ObservableCollection<string> BranchesList { get; } = new();

        // Comandos
        public ICommand SaveDonationCommand { get; }
        public ICommand ResetFormCommand { get; }

        public DonationRegisterViewModel(
            IDonationService? donationService = null,
            Func<Task>? onSaveSuccess = null)
        {
            _donationService = donationService ?? new DonationService();
            _onSaveSuccess = onSaveSuccess;

            LoadCatalogDefaults();

            SaveDonationCommand = new RelayCommand(async _ => await SaveDonationAsync());
            ResetFormCommand = new RelayCommand(_ => ResetForm());
        }

        public void SetCurrentUser(UserModel? user)
        {
            _currentUser = user;
            if (user != null && !string.IsNullOrWhiteSpace(user.Branch))
            {
                _userDefaultBranch = user.Branch;
                NewBranchId = user.Branch;

                if (!BranchesList.Contains(user.Branch))
                {
                    BranchesList.Add(user.Branch);
                }
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

        public void ResetForm()
        {
            NewDonorCompany = string.Empty;
            NewDonorType = null;
            NewDonationDate = DateTime.Now;
            NewDonatedProduct = string.Empty;
            NewPresentation = string.Empty;
            NewDonationQuantity = string.Empty;
            NewUnitPrice = string.Empty;
            NewReceiptFolio = string.Empty;
            NewDonationNotes = string.Empty;
            NewBranchId = _userDefaultBranch;
        }

        public async Task SaveDonationAsync()
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
                Branch = NewBranchId ?? _userDefaultBranch ?? string.Empty,
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
    }
}