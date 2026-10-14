using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Pantries.Interfaces;
using AMANC_Inventory.Pantries.Models;
using AMANC_Inventory.Pantries.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AMANC_Inventory.Pantries.ViewModels
{
    public class PantryConsultViewModel : ViewModelBase
    {
        private readonly IPantryService _pantryService;
        private bool _isLoaded;
        private bool _isLoading;
        private string _filterText = string.Empty;

        public ObservableCollection<PantryModel> PantryItems { get; } = new();
        public ObservableCollection<PantryModel> FilteredPantriesList { get; } = new();

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

        public PantryConsultViewModel(IPantryService? pantryService = null)
        {
            _pantryService = pantryService ?? new PantryService();

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
                var pantries = await _pantryService.GetAllPantriesAsync();

                ExecuteOnUIThread(() =>
                {
                    PantryItems.Clear();
                    if (pantries != null)
                    {
                        foreach (var item in pantries)
                        {
                            PantryItems.Add(item);
                        }
                    }
                    ApplyFilter(FilterText);
                });

                _isLoaded = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error al cargar despensas]: {ex.Message}");
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
                FilteredPantriesList.Clear();
                var query = PantryItems.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(filterText))
                {
                    string cleanFilter = filterText.Trim();
                    query = query.Where(p =>
                        (!string.IsNullOrEmpty(p.Destination) && p.Destination.StartsWith(cleanFilter, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(p.ProductName) && p.ProductName.StartsWith(cleanFilter, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(p.DeliveredBy) && p.DeliveredBy.StartsWith(cleanFilter, StringComparison.OrdinalIgnoreCase))
                    );
                }

                foreach (var item in query)
                {
                    FilteredPantriesList.Add(item);
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