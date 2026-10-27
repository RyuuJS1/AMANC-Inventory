using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Patients.Interfaces;
using AMANC_Inventory.Patients.Models;
using AMANC_Inventory.Patients.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AMANC_Inventory.Patients.ViewModels
{
    public class PatientConsultViewModel : ViewModelBase
    {
        private readonly IPatientService _patientService;
        private bool _isLoaded;
        private bool _isLoading;
        private string _filterText = string.Empty;

        public ObservableCollection<PatientModel> PatientItems { get; } = new();
        public ObservableCollection<PatientModel> FilteredPatientsList { get; } = new();

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (SetProperty(ref _filterText, value))
                {
                    ApplyFilter(value);
                }
            }
        }

        public ICommand LoadDataCommand { get; }
        public ICommand ClearFilterCommand { get; }

        public PatientConsultViewModel(IPatientService? patientService = null)
        {
            _patientService = patientService ?? new PatientService();

            LoadDataCommand = new RelayCommand(async _ => await ForceReloadAsync());
            ClearFilterCommand = new RelayCommand(_ => FilterText = string.Empty);
        }

        public async Task EnsureDataLoadedAsync()
        {
            if (_isLoaded) return;
            await ForceReloadAsync();
        }

        public async Task ForceReloadAsync()
        {
            IsLoading = true;
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();

                ExecuteOnUIThread(() =>
                {
                    PatientItems.Clear();
                    if (patients != null)
                    {
                        foreach (var item in patients)
                        {
                            PatientItems.Add(item);
                        }
                    }
                    ApplyFilter(FilterText);
                });

                _isLoaded = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error al cargar beneficiarios]: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void ApplyFilter(string filterText)
        {
            ExecuteOnUIThread(() =>
            {
                FilteredPatientsList.Clear();
                var query = PatientItems.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(filterText))
                {
                    string cleanFilter = filterText.Trim();
                    query = query.Where(p =>
                        (!string.IsNullOrEmpty(p.ChildName) && p.ChildName.StartsWith(cleanFilter, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(p.CancerType) && p.CancerType.StartsWith(cleanFilter, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(p.City) && p.City.StartsWith(cleanFilter, StringComparison.OrdinalIgnoreCase))
                    );
                }

                foreach (var item in query)
                {
                    FilteredPatientsList.Add(item);
                }
            });
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