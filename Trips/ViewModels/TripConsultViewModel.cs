using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Trips.Interfaces;
using AMANC_Inventory.Trips.Models;
using AMANC_Inventory.Trips.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AMANC_Inventory.Trips.ViewModels
{
    public class TripConsultViewModel : ViewModelBase
    {
        private readonly ITripService _tripService;
        private bool _isLoaded;
        private bool _isLoading;
        private string _filterText = string.Empty;

        public ObservableCollection<TripModel> TripItems { get; } = new();
        public ObservableCollection<TripModel> FilteredTripsList { get; } = new();

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

        public TripConsultViewModel(ITripService? tripService = null)
        {
            _tripService = tripService ?? new TripService();

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
                var trips = await _tripService.GetAllTripsAsync();

                ExecuteOnUIThread(() =>
                {
                    TripItems.Clear();
                    if (trips != null)
                    {
                        foreach (var item in trips)
                        {
                            TripItems.Add(item);
                        }
                    }
                    ApplyFilter(FilterText);
                });

                _isLoaded = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error al cargar traslados]: {ex.Message}");
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
                FilteredTripsList.Clear();
                var query = TripItems.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(filterText))
                {
                    string cleanFilter = filterText.Trim();
                    query = query.Where(t =>
                        (!string.IsNullOrEmpty(t.ChildName) && t.ChildName.StartsWith(cleanFilter, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(t.Origin) && t.Origin.StartsWith(cleanFilter, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(t.Destination) && t.Destination.StartsWith(cleanFilter, StringComparison.OrdinalIgnoreCase))
                    );
                }

                foreach (var item in query)
                {
                    FilteredTripsList.Add(item);
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