using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Patients.Interfaces;
using AMANC_Inventory.Patients.Models;
using AMANC_Inventory.Patients.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AMANC_Inventory.Patients.ViewModels
{
    public class PatientRegisterViewModel : ViewModelBase
    {
        private readonly IPatientService _patientService;
        private readonly Func<Task>? _onSaveSuccess;

        // Propiedades del Formulario
        private string? _newChildName;
        public string? NewChildName { get => _newChildName; set => SetProperty(ref _newChildName, value); }

        private string? _newAge;
        public string? NewAge { get => _newAge; set => SetProperty(ref _newAge, value); }

        private string? _newCurp;
        public string? NewCurp { get => _newCurp; set => SetProperty(ref _newCurp, value); }

        private string? _newCancerType;
        public string? NewCancerType { get => _newCancerType; set => SetProperty(ref _newCancerType, value); }

        private string? _newGuardians;
        public string? NewGuardians { get => _newGuardians; set => SetProperty(ref _newGuardians, value); }

        private string? _newRelationship;
        public string? NewRelationship { get => _newRelationship; set => SetProperty(ref _newRelationship, value); }

        private string? _newPhones;
        public string? NewPhones { get => _newPhones; set => SetProperty(ref _newPhones, value); }

        private string? _newEmails;
        public string? NewEmails { get => _newEmails; set => SetProperty(ref _newEmails, value); }

        private string? _newCity;
        public string? NewCity { get => _newCity; set => SetProperty(ref _newCity, value); }

        private string? _newState;
        public string? NewState { get => _newState; set => SetProperty(ref _newState, value); }

        private string? _newPatientStatus;
        public string? NewPatientStatus { get => _newPatientStatus; set => SetProperty(ref _newPatientStatus, value); }

        private string? _newPatientNotes;
        public string? NewPatientNotes { get => _newPatientNotes; set => SetProperty(ref _newPatientNotes, value); }

        // Colecciones para Combos
        public ObservableCollection<string> PatientStatusList { get; } = new();

        // Comandos
        public ICommand SavePatientCommand { get; }
        public ICommand ResetFormCommand { get; }

        public PatientRegisterViewModel(
            IPatientService? patientService = null,
            Func<Task>? onSaveSuccess = null)
        {
            _patientService = patientService ?? new PatientService();
            _onSaveSuccess = onSaveSuccess;

            LoadCatalogDefaults();

            SavePatientCommand = new RelayCommand(async _ => await SavePatientAsync());
            ResetFormCommand = new RelayCommand(_ => ResetForm());
        }

        private void LoadCatalogDefaults()
        {
            PatientStatusList.Clear();
            PatientStatusList.Add("Activo");
            PatientStatusList.Add("En Tratamiento");
            PatientStatusList.Add("Inactivo");
        }

        public void ResetForm()
        {
            NewChildName = string.Empty;
            NewAge = string.Empty;
            NewCurp = string.Empty;
            NewCancerType = string.Empty;
            NewGuardians = string.Empty;
            NewRelationship = string.Empty;
            NewPhones = string.Empty;
            NewEmails = string.Empty;
            NewCity = string.Empty;
            NewState = string.Empty;
            NewPatientStatus = null;
            NewPatientNotes = string.Empty;
        }

        public async Task SavePatientAsync()
        {
            if (string.IsNullOrWhiteSpace(NewChildName) || string.IsNullOrWhiteSpace(NewCancerType))
            {
                MessageBox.Show("Por favor, ingrese el nombre del beneficiario y el diagnóstico.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Convierte el string del formulario a int
            int.TryParse(NewAge, out int age);

            var patient = new PatientModel
            {
                ChildName = NewChildName.Trim(),
                Age = NewAge?.Trim() ?? string.Empty,
                Curp = NewCurp ?? string.Empty,
                CancerType = NewCancerType.Trim(),
                Guardians = NewGuardians ?? string.Empty,
                Relationship = NewRelationship ?? string.Empty,
                Phones = NewPhones ?? string.Empty,
                Emails = NewEmails ?? string.Empty,
                City = NewCity ?? string.Empty,
                State = NewState ?? string.Empty,
                Status = NewPatientStatus ?? "Activo",
                Notes = NewPatientNotes ?? string.Empty
            };

            bool success = await _patientService.SavePatientAsync(patient);
            if (success)
            {
                MessageBox.Show("Beneficiario registrado correctamente en Firebase.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                ResetForm();
                if (_onSaveSuccess != null) await _onSaveSuccess.Invoke();
            }
            else
            {
                MessageBox.Show("Error al guardar el beneficiario en la base de datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}