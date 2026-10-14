using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Trips.Interfaces;
using AMANC_Inventory.Trips.Models;
using AMANC_Inventory.Trips.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AMANC_Inventory.Trips.ViewModels
{
    public class TripRegisterViewModel : ViewModelBase
    {
        private readonly ITripService _tripService;
        private readonly Func<Task>? _onSaveSuccess;

        // Propiedades del Formulario
        private string? _newTripChild;
        public string? NewTripChild { get => _newTripChild; set => SetProperty(ref _newTripChild, value); }

        private string? _newTripGuardian;
        public string? NewTripGuardian { get => _newTripGuardian; set => SetProperty(ref _newTripGuardian, value); }

        private string? _newOrigin;
        public string? NewOrigin { get => _newOrigin; set => SetProperty(ref _newOrigin, value); }

        private string? _newDestination;
        public string? NewDestination { get => _newDestination; set => SetProperty(ref _newDestination, value); }

        private string? _newTransportType;
        public string? NewTransportType { get => _newTransportType; set => SetProperty(ref _newTransportType, value); }

        private DateTime? _newDepartureDate = DateTime.Now;
        public DateTime? NewDepartureDate { get => _newDepartureDate; set => SetProperty(ref _newDepartureDate, value); }

        private DateTime? _newReturnDate = DateTime.Now;
        public DateTime? NewReturnDate { get => _newReturnDate; set => SetProperty(ref _newReturnDate, value); }

        private string? _newTripStatus;
        public string? NewTripStatus { get => _newTripStatus; set => SetProperty(ref _newTripStatus, value); }

        private string? _newTripNotes;
        public string? NewTripNotes { get => _newTripNotes; set => SetProperty(ref _newTripNotes, value); }

        // Colecciones para Combos
        public ObservableCollection<string> TransportTypesList { get; } = new();
        public ObservableCollection<string> TripStatusList { get; } = new();

        // Comandos
        public ICommand SaveTripCommand { get; }
        public ICommand ResetFormCommand { get; }

        public TripRegisterViewModel(
            ITripService? tripService = null,
            Func<Task>? onSaveSuccess = null)
        {
            _tripService = tripService ?? new TripService();
            _onSaveSuccess = onSaveSuccess;

            LoadCatalogDefaults();

            SaveTripCommand = new RelayCommand(async _ => await SaveTripAsync());
            ResetFormCommand = new RelayCommand(_ => ResetForm());
        }

        private void LoadCatalogDefaults()
        {
            TransportTypesList.Clear();
            TransportTypesList.Add("Autobús");
            TransportTypesList.Add("Avión");
            TransportTypesList.Add("Particular");

            TripStatusList.Clear();
            TripStatusList.Add("Programado");
            TripStatusList.Add("En Proceso");
            TripStatusList.Add("Completado");
            TripStatusList.Add("Cancelado");
        }

        public void ResetForm()
        {
            NewTripChild = string.Empty;
            NewTripGuardian = string.Empty;
            NewOrigin = string.Empty;
            NewDestination = string.Empty;
            NewTransportType = null;
            NewDepartureDate = DateTime.Now;
            NewReturnDate = DateTime.Now;
            NewTripStatus = null;
            NewTripNotes = string.Empty;
        }

        public async Task SaveTripAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTripChild) || string.IsNullOrWhiteSpace(NewOrigin) || string.IsNullOrWhiteSpace(NewDestination))
            {
                MessageBox.Show("Por favor, ingrese el paciente, origen y destino del viaje.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var trip = new TripModel
            {
                ChildName = NewTripChild.Trim(),
                GuardianName = NewTripGuardian ?? string.Empty,
                Origin = NewOrigin.Trim(),
                Destination = NewDestination.Trim(),
                TransportType = NewTransportType ?? string.Empty,
                DepartureDate = NewDepartureDate ?? DateTime.Now,
                ArrivalDate = NewReturnDate ?? DateTime.Now,
                Status = NewTripStatus ?? "Programado",
                Notes = NewTripNotes ?? string.Empty
            };

            bool success = await _tripService.SaveTripAsync(trip);
            if (success)
            {
                MessageBox.Show("Traslado registrado correctamente en Firebase.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetForm();
                if (_onSaveSuccess != null) await _onSaveSuccess.Invoke();
            }
            else
            {
                MessageBox.Show("Error al guardar el traslado en la base de datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}