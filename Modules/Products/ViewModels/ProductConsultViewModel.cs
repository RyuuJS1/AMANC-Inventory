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
            _ = EnsureDataLoadedAsync();
        }

        /// <summary>
        /// Carga los datos si no han sido cargados o si la colección actual está vacía.
        /// </summary>
        public async Task EnsureDataLoadedAsync()
        {
            if (_isLoaded && Items.Any()) return;
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
                    if (products != null && products.Any())
                    {
                        foreach (var p in products)
                        {
                            Items.Add(p);
                        }
                        _isLoaded = true; // Solo marca como cargado si hubo éxito
                    }
                    else
                    {
                        _isLoaded = false; // Permite reintentar si no devolvió registros
                    }

                    ApplyFilter(string.Empty);
                });
            }
            catch (Exception ex)
            {
                _isLoaded = false; // Restablece para reintentar en el siguiente cambio de pestaña
                ExecuteOnUIThread(() =>
                {
                    MessageBox.Show($"Error al consultar productos desde Firebase:\n{ex.Message}",
                                    "Error de Carga", MessageBoxButton.OK, MessageBoxImage.Error);
                });
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
                    query = query.Where(i =>
                        (!string.IsNullOrEmpty(i.Name) && i.Name.Contains(cleanFilter, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(i.SKU) && i.SKU.Contains(cleanFilter, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(i.Barcode) && i.Barcode.Contains(cleanFilter, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(i.Category) && i.Category.Contains(cleanFilter, StringComparison.OrdinalIgnoreCase))
                    );
                }

                foreach (var item in query)
                {
                    FilteredItems.Add(item);
                }

                OnPropertyChanged(nameof(FilteredItems));
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