using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Inventory.Interfaces;
using AMANC_Inventory.Inventory.Models;
using AMANC_Inventory.Inventory.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AMANC_Inventory.Inventory.ViewModels
{
    public class ProductConsultViewModel : ViewModelBase
    {
        private readonly IProductService _productService;
        private bool _isLoaded;
        private bool _isLoading;

        public ObservableCollection<ProductModel> Items { get; } = new();
        public ObservableCollection<ProductModel> FilteredItems { get; } = new();

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ProductConsultViewModel(IProductService? productService = null)
        {
            _productService = productService ?? new ProductService();
        }

        /// <summary>
        /// Garantiza la carga perezosa solo la primera vez que se abre la pestaña.
        /// </summary>
        public async Task EnsureDataLoadedAsync()
        {
            if (_isLoaded) return;
            await ForceReloadAsync();
        }

        /// <summary>
        /// Fuerza la descarga y actualización de la lista de productos desde Firebase.
        /// </summary>
        public async Task ForceReloadAsync()
        {
            IsLoading = true;
            try
            {
                var products = await _productService.GetAllProductsAsync();

                ExecuteOnUIThread(() =>
                {
                    Items.Clear();
                    if (products != null)
                    {
                        foreach (var p in products)
                        {
                            Items.Add(p);
                        }
                    }
                    ApplyFilter(string.Empty);
                });

                _isLoaded = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Error al cargar productos]: {ex.Message}");
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
                FilteredItems.Clear();
                var query = Items.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(filterText))
                {
                    string cleanFilter = filterText.Trim();
                    query = query.Where(i => i.Name != null && i.Name.StartsWith(cleanFilter, StringComparison.OrdinalIgnoreCase));
                }

                foreach (var item in query)
                {
                    FilteredItems.Add(item);
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