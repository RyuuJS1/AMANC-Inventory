using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using AMANC_Inventory.Core.Architecture;

namespace AMANC_Inventory.Inventory.ViewModels
{
    public class InventoryRegisterViewModel : INotifyPropertyChanged
    {
        // -------------------------------------------------------------
        // ESTADOS DE LAS PESTAÑAS (TAB SELECTION)
        // -------------------------------------------------------------
        private bool _isInventoryTabSelected = true;
        public bool IsInventoryTabSelected
        {
            get => _isInventoryTabSelected;
            set
            {
                if (_isInventoryTabSelected != value)
                {
                    _isInventoryTabSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isDonationsTabSelected;
        public bool IsDonationsTabSelected
        {
            get => _isDonationsTabSelected;
            set
            {
                if (_isDonationsTabSelected != value)
                {
                    _isDonationsTabSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isPatientsTabSelected;
        public bool IsPatientsTabSelected
        {
            get => _isPatientsTabSelected;
            set
            {
                if (_isPatientsTabSelected != value)
                {
                    _isPatientsTabSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isTripsTabSelected;
        public bool IsTripsTabSelected
        {
            get => _isTripsTabSelected;
            set
            {
                if (_isTripsTabSelected != value)
                {
                    _isTripsTabSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        // -------------------------------------------------------------
        // DECLARACIÓN DE COMANDOS (ICommand)
        // -------------------------------------------------------------
        public ICommand ResetFormCommand { get; }
        public ICommand SaveItemCommand { get; }
        public ICommand SaveDonationCommand { get; }
        public ICommand SavePatientCommand { get; }
        public ICommand SaveTripCommand { get; }

        // -------------------------------------------------------------
        // CONSTRUCTOR
        // -------------------------------------------------------------
        public InventoryRegisterViewModel()
        {
            // Pestaña inicial por defecto
            IsInventoryTabSelected = true;

            // Instanciación utilizando tu RelayCommand
            // - ResetForm es sincrónico (Action<object?>)
            // - Guardados aprovechan el sobrecarga asíncrona (Func<object?, Task>)
            ResetFormCommand = new RelayCommand(_ => ResetForm());
            SaveItemCommand = new RelayCommand(async _ => await SaveItemAsync());
            SaveDonationCommand = new RelayCommand(async _ => await SaveDonationAsync());
            SavePatientCommand = new RelayCommand(async _ => await SavePatientAsync());
            SaveTripCommand = new RelayCommand(async _ => await SaveTripAsync());
        }

        // -------------------------------------------------------------
        // MÉTODOS DE LÓGICA DE NEGOCIO
        // -------------------------------------------------------------
        private void ResetForm()
        {
            // Lógica para limpiar las cajas de texto de la pestaña activa
        }

        private async Task SaveItemAsync()
        {
            // Tu código asíncrono para guardar en BD/repositorio
            await Task.Delay(500); // Ejemplo de operación asíncrona
        }

        private async Task SaveDonationAsync()
        {
            await Task.Delay(500);
        }

        private async Task SavePatientAsync()
        {
            await Task.Delay(500);
        }

        private async Task SaveTripAsync()
        {
            await Task.Delay(500);
        }

        // -------------------------------------------------------------
        // IMPLEMENTACIÓN DE INotifyPropertyChanged
        // -------------------------------------------------------------
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}