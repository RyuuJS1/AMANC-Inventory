using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Donations.Interfaces;
using AMANC_Inventory.Donations.Models;
using AMANC_Inventory.Donations.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AMANC_Inventory.Donations.ViewModels
{
    public class DonationConsultViewModel : ViewModelBase
    {
        private readonly IDonationService _donationService;
        private bool _isLoaded;
        private bool _isLoading;
        private string _filterText = string.Empty;

        public ObservableCollection<DonationModel> DonationItems { get; } = new();
        public ObservableCollection<DonationModel> FilteredDonationsList { get; } = new();

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

        public DonationConsultViewModel(IDonationService? donationService = null)
        {
            _donationService = donationService ?? new DonationService();

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
                var donations = await _donationService.GetAllDonationsAsync();

                ExecuteOnUIThread(() =>
                {
                    DonationItems.Clear();
                    if (donations != null)
                    {
                        foreach (var item in donations)
                        {
                            DonationItems.Add(item);
                        }
                    }
                    ApplyFilter(FilterText);
                });

                _isLoaded = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error al cargar donaciones]: {ex.Message}");
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
                FilteredDonationsList.Clear();
                var query = DonationItems.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(filterText))
                {
                    string cleanFilter = filterText.Trim();
                    query = query.Where(d =>
                        (!string.IsNullOrEmpty(d.ProductName) && d.ProductName.StartsWith(cleanFilter, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(d.CompanyName) && d.CompanyName.StartsWith(cleanFilter, StringComparison.OrdinalIgnoreCase))
                    );
                }

                foreach (var item in query)
                {
                    FilteredDonationsList.Add(item);
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