using AMANC_Inventory.Helpers;
using AMANC_Inventory.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AMANC_Inventory.ViewModels
{
    public class InventoryViewModel : ViewModelBase
    {
        private UserModel? _usuarioActual;
        private bool _isLoading;
        private bool _isEditing;
        private string _searchText = "";

        // Navegación de Pestañas (0 = Consultas, 1 = Registros)
        private int _selectedTab = 0;

        // Campos para el Header y Perfil
        private string _nombreUsuario = "Usuario";
        private string _userInitials = "U";
        private bool _hasProfilePicture;
        private bool _hasNoProfilePicture = true;
        private bool _isProfileDetailsOpen;

        public InventoryViewModel()
        {
            Items = new ObservableCollection<string>();

            // Inicialización de comandos
            LoadItemsCommand = new RelayCommand(async _ => await CargarInventarioAsync());
            AddItemCommand = new RelayCommand(_ => AgregarItem());
            CancelEditCommand = new RelayCommand(_ => CancelarEdicion());
            SelectTabCommand = new RelayCommand(p => SeleccionarPestana(p));
            AbrirPerfilCommand = new RelayCommand(_ => AbrirPerfil());
            CerrarPerfilCommand = new RelayCommand(_ => CerrarPerfil());
        }

        #region Propiedades

        public ObservableCollection<string> Items { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        // --- Control de Pestañas (Consultas / Registros) ---

        public int SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (SetProperty(ref _selectedTab, value))
                {
                    OnPropertyChanged(nameof(IsTabConsultVisible));
                    OnPropertyChanged(nameof(IsTabRegisterVisible));
                }
            }
        }

        public bool IsTabConsultVisible => SelectedTab == 0;
        public bool IsTabRegisterVisible => SelectedTab == 1;

        // --- Header y Perfil ---

        public string NombreUsuario
        {
            get => _nombreUsuario;
            set => SetProperty(ref _nombreUsuario, value);
        }

        public string UserInitials
        {
            get => _userInitials;
            set => SetProperty(ref _userInitials, value);
        }

        public bool HasProfilePicture
        {
            get => _hasProfilePicture;
            set => SetProperty(ref _hasProfilePicture, value);
        }

        public bool HasNoProfilePicture
        {
            get => _hasNoProfilePicture;
            set => SetProperty(ref _hasNoProfilePicture, value);
        }

        public bool IsProfileDetailsOpen
        {
            get => _isProfileDetailsOpen;
            set => SetProperty(ref _isProfileDetailsOpen, value);
        }

        #endregion

        #region Comandos

        public ICommand LoadItemsCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand SelectTabCommand { get; }
        public ICommand AbrirPerfilCommand { get; }
        public ICommand CerrarPerfilCommand { get; }

        #endregion

        #region Métodos

        public void SeleccionarPestana(object? parameter)
        {
            if (parameter != null && int.TryParse(parameter.ToString(), out int tabIndex))
            {
                SelectedTab = tabIndex;
            }
        }

        public void AbrirPerfil()
        {
            IsProfileDetailsOpen = true;
        }

        public void CerrarPerfil()
        {
            IsProfileDetailsOpen = false;
        }

        /// <summary>
        /// Inicializa los datos del usuario logueado y desencadena la carga del inventario.
        /// </summary>
        public void InicializarUsuario(UserModel usuario)
        {
            _usuarioActual = usuario;

            if (usuario != null)
            {
                NombreUsuario = !string.IsNullOrEmpty(usuario.Name) ? usuario.Name : "Usuario";
                UserInitials = !string.IsNullOrEmpty(usuario.UserInitials) ? usuario.UserInitials : "U";
                HasProfilePicture = !string.IsNullOrEmpty(usuario.ProfilePictureBase64);
                HasNoProfilePicture = !HasProfilePicture;
            }

            _ = CargarInventarioAsync();
        }

        private async Task CargarInventarioAsync()
        {
            IsLoading = true;
            await Task.Delay(300); // Simulación de carga
            IsLoading = false;
        }

        private void AgregarItem()
        {
            IsEditing = true;
        }

        private void CancelarEdicion()
        {
            IsEditing = false;
        }

        #endregion
    }
}