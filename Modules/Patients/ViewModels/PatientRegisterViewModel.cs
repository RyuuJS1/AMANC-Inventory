using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Patients.Interfaces;
using AMANC_Inventory.Patients.Models;
using AMANC_Inventory.Patients.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AMANC_Inventory.Patients.ViewModels
{
    public class PatientRegisterViewModel : ViewModelBase
    {
        private readonly IPatientService _patientService;
        private readonly Func<Task>? _onSaveSuccess;

        // ==========================================
        // PROPIEDADES DEL BENEFICIARIO (NIÑO/A)
        // ==========================================
        private string? _newChildName;
        public string? NewChildName { get => _newChildName; set => SetProperty(ref _newChildName, value); }

        private string? _newAge;
        public string? NewAge { get => _newAge; set => SetProperty(ref _newAge, value); }

        private string? _newCurp;
        public string? NewCurp { get => _newCurp; set => SetProperty(ref _newCurp, value); }

        private string? _newCancerType;
        public string? NewCancerType { get => _newCancerType; set => SetProperty(ref _newCancerType, value); }

        private string? _newPatientStatus = "Activo";
        public string? NewPatientStatus { get => _newPatientStatus; set => SetProperty(ref _newPatientStatus, value); }

        private string? _newPatientNotes;
        public string? NewPatientNotes { get => _newPatientNotes; set => SetProperty(ref _newPatientNotes, value); }

        // ==========================================
        // CAMPOS TEMPORALES PARA AÑADIR TUTOR
        // ==========================================
        private string? _tempGuardianName;
        public string? TempGuardianName { get => _tempGuardianName; set => SetProperty(ref _tempGuardianName, value); }

        private string? _tempRelationship = "Padre/Madre";
        public string? TempRelationship { get => _tempRelationship; set => SetProperty(ref _tempRelationship, value); }

        private string? _tempPhone;
        public string? TempPhone { get => _tempPhone; set => SetProperty(ref _tempPhone, value); }

        private string? _tempEmail;
        public string? TempEmail { get => _tempEmail; set => SetProperty(ref _tempEmail, value); }

        // ==========================================
        // UBICACIÓN DEL BENEFICIARIO
        // ==========================================
        private string? _newCity;
        public string? NewCity { get => _newCity; set => SetProperty(ref _newCity, value); }

        private string? _newState;
        public string? NewState { get => _newState; set => SetProperty(ref _newState, value); }

        // ==========================================
        // COLECCIONES
        // ==========================================
        public ObservableCollection<GuardianModel> GuardiansList { get; } = new();
        public ObservableCollection<string> PatientStatusList { get; } = new();
        public ObservableCollection<string> RelationshipsList { get; } = new();

        // ==========================================
        // COMANDOS
        // ==========================================
        public ICommand AddGuardianCommand { get; }
        public ICommand RemoveGuardianCommand { get; }
        public ICommand SavePatientCommand { get; }
        public ICommand ResetFormCommand { get; }

        public PatientRegisterViewModel(
            IPatientService? patientService = null,
            Func<Task>? onSaveSuccess = null)
        {
            _patientService = patientService ?? new PatientService();
            _onSaveSuccess = onSaveSuccess;

            LoadCatalogDefaults();

            AddGuardianCommand = new RelayCommand(_ => AddGuardian());
            RemoveGuardianCommand = new RelayCommand(param => RemoveGuardian(param as GuardianModel));
            SavePatientCommand = new RelayCommand(async _ => await SavePatientAsync());
            ResetFormCommand = new RelayCommand(_ => ResetForm());
        }

        private void LoadCatalogDefaults()
        {
            PatientStatusList.Clear();
            PatientStatusList.Add("Activo");
            PatientStatusList.Add("En Tratamiento");
            PatientStatusList.Add("Inactivo");

            RelationshipsList.Clear();
            RelationshipsList.Add("Padre/Madre");
            RelationshipsList.Add("Tutor Legal");
            RelationshipsList.Add("Abuelo/a");
            RelationshipsList.Add("Hermano/a");
            RelationshipsList.Add("Otro");
        }

        // ==========================================
        // MÉTODOS DE GESTIÓN DE TUTORES
        // ==========================================
        private void AddGuardian()
        {
            if (string.IsNullOrWhiteSpace(TempGuardianName))
            {
                MessageBox.Show("Ingrese el nombre completo del tutor.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TempPhone))
            {
                MessageBox.Show("Ingrese un número telefónico de contacto para el tutor.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            GuardiansList.Add(new GuardianModel
            {
                Name = TempGuardianName.Trim(),
                Relationship = TempRelationship ?? "Padre/Madre",
                Phone = TempPhone.Trim(),
                Email = TempEmail?.Trim() ?? string.Empty
            });

            // Limpiar campos de captura de tutor
            TempGuardianName = string.Empty;
            TempPhone = string.Empty;
            TempEmail = string.Empty;
            TempRelationship = "Padre/Madre";
        }

        private void RemoveGuardian(GuardianModel? guardian)
        {
            if (guardian != null && GuardiansList.Contains(guardian))
            {
                GuardiansList.Remove(guardian);
            }
        }

        // ==========================================
        // REINICIO DE FORMULARIO
        // ==========================================
        public void ResetForm()
        {
            NewChildName = string.Empty;
            NewAge = string.Empty;
            NewCurp = string.Empty;
            NewCancerType = string.Empty;
            NewPatientStatus = "Activo";
            NewPatientNotes = string.Empty;

            TempGuardianName = string.Empty;
            TempRelationship = "Padre/Madre";
            TempPhone = string.Empty;
            TempEmail = string.Empty;

            GuardiansList.Clear();

            NewCity = string.Empty;
            NewState = string.Empty;
        }

        // ==========================================
        // GUARDADO DE BENEFICIARIO
        // ==========================================
        public async Task SavePatientAsync()
        {
            if (string.IsNullOrWhiteSpace(NewChildName) || string.IsNullOrWhiteSpace(NewCancerType))
            {
                MessageBox.Show("Por favor, ingrese el nombre del beneficiario y el diagnóstico.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!GuardiansList.Any())
            {
                MessageBox.Show("Debe agregar al menos un tutor a la lista antes de guardar.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var patient = new PatientModel
            {
                ChildName = NewChildName.Trim(),
                Age = NewAge?.Trim() ?? string.Empty,
                Curp = NewCurp?.Trim().ToUpper() ?? string.Empty,
                CancerType = NewCancerType.Trim(),
                Guardians = GuardiansList.ToList(),
                City = NewCity?.Trim() ?? string.Empty,
                State = NewState?.Trim() ?? string.Empty,
                Status = NewPatientStatus ?? "Activo",
                Notes = NewPatientNotes ?? string.Empty
            };

            bool success = await _patientService.SavePatientAsync(patient);
            if (success)
            {
                MessageBox.Show("Beneficiario registrado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
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