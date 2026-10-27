using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Patients.Interfaces;
using AMANC_Inventory.Patients.Models;
using AMANC_Inventory.Patients.Services;
using AMANC_Inventory.Trips.Interfaces;
using AMANC_Inventory.Trips.Models;
using AMANC_Inventory.Trips.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AMANC_Inventory.Trips.ViewModels
{
    public class TripRegisterViewModel : ViewModelBase
    {
        private readonly ITripService _tripService;
        private readonly IPatientService _patientService;
        private readonly Func<Task>? _onSaveSuccess;

        // Selección de Paciente Vinculado
        private PatientModel? _selectedPatient;
        public PatientModel? SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                if (SetProperty(ref _selectedPatient, value))
                {
                    if (value != null)
                    {
                        NewTripChild = value.ChildName;

                        // Autocompleta el nombre del acompañante si existen tutores registrados
                        if (value.Guardians != null && value.Guardians.Any())
                        {
                            NewTripGuardian = string.Join(" / ", value.Guardians.Select(g => g.Name));
                        }
                    }
                    else
                    {
                        NewTripChild = string.Empty;
                        NewTripGuardian = string.Empty;
                    }
                }
            }
        }

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

        private string? _newTripStatus = "Programado";
        public string? NewTripStatus { get => _newTripStatus; set => SetProperty(ref _newTripStatus, value); }

        private string? _newTripNotes;
        public string? NewTripNotes { get => _newTripNotes; set => SetProperty(ref _newTripNotes, value); }

        // Colecciones para Combos
        public ObservableCollection<PatientModel> PatientsList { get; } = new();
        public ObservableCollection<string> TransportTypesList { get; } = new();
        public ObservableCollection<string> TripStatusList { get; } = new();

        // Comandos
        public ICommand SaveTripCommand { get; }
        public ICommand ResetFormCommand { get; }

        public TripRegisterViewModel(
            ITripService? tripService = null,
            IPatientService? patientService = null,
            Func<Task>? onSaveSuccess = null)
        {
            _tripService = tripService ?? new TripService();
            _patientService = patientService ?? new PatientService();
            _onSaveSuccess = onSaveSuccess;

            LoadCatalogDefaults();
            _ = LoadPatientsAsync();

            SaveTripCommand = new RelayCommand(async _ => await SaveTripAsync());
            ResetFormCommand = new RelayCommand(_ => ResetForm());
        }

        public async Task LoadPatientsAsync()
        {
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();
                PatientsList.Clear();
                foreach (var patient in patients.OrderBy(p => p.ChildName))
                {
                    PatientsList.Add(patient);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Error al cargar pacientes en viajes]: {ex.Message}");
            }
        }

        private void LoadCatalogDefaults()
        {
            TransportTypesList.Clear();
            TransportTypesList.Add("Autobús");
            TransportTypesList.Add("Avión");
            TransportTypesList.Add("Particular");
            TransportTypesList.Add("Ambulancia / Vehículo AMANC");

            TripStatusList.Clear();
            TripStatusList.Add("Programado");
            TripStatusList.Add("En Proceso");
            TripStatusList.Add("Completado");
            TripStatusList.Add("Cancelado");
        }

        public void ResetForm()
        {
            SelectedPatient = null;
            NewTripChild = string.Empty;
            NewTripGuardian = string.Empty;
            NewOrigin = string.Empty;
            NewDestination = string.Empty;
            NewTransportType = null;
            NewDepartureDate = DateTime.Now;
            NewReturnDate = DateTime.Now;
            NewTripStatus = "Programado";
            NewTripNotes = string.Empty;
        }

        public async Task SaveTripAsync()
        {
            // 1. Validar Paciente
            if (SelectedPatient == null || string.IsNullOrWhiteSpace(NewTripChild))
            {
                MessageBox.Show("Por favor, seleccione un beneficiario/niño de la lista.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Validar Acompañante
            if (string.IsNullOrWhiteSpace(NewTripGuardian))
            {
                MessageBox.Show("Por favor, ingrese el nombre del acompañante o tutor.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Validar Origen y Destino
            if (string.IsNullOrWhiteSpace(NewOrigin))
            {
                MessageBox.Show("Por favor, ingrese la ciudad/lugar de origen.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(NewDestination))
            {
                MessageBox.Show("Por favor, ingrese el destino del traslado.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 4. Validar Medio de Transporte
            if (string.IsNullOrWhiteSpace(NewTransportType))
            {
                MessageBox.Show("Por favor, seleccione el medio de transporte.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 5. Validar Fechas
            if (!NewDepartureDate.HasValue)
            {
                MessageBox.Show("Por favor, seleccione la fecha de salida.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!NewReturnDate.HasValue)
            {
                MessageBox.Show("Por favor, seleccione la fecha estimada de regreso.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NewReturnDate.Value.Date < NewDepartureDate.Value.Date)
            {
                MessageBox.Show("La fecha estimada de regreso no puede ser anterior a la fecha de salida.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 6. Validar Estatus
            if (string.IsNullOrWhiteSpace(NewTripStatus))
            {
                MessageBox.Show("Por favor, seleccione el estatus de la solicitud.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var trip = new TripModel
            {
                ChildName = NewTripChild.Trim(),
                GuardianName = NewTripGuardian.Trim(),
                Origin = NewOrigin.Trim(),
                Destination = NewDestination.Trim(),
                TransportType = NewTransportType,
                DepartureDate = NewDepartureDate.Value,
                ArrivalDate = NewReturnDate.Value,
                Status = NewTripStatus,
                Notes = NewTripNotes ?? string.Empty
            };

            bool success = await _tripService.SaveTripAsync(trip);
            if (success)
            {
                MessageBox.Show("Traslado registrado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
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