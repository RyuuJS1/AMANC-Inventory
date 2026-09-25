using AMANC_Inventory.Helpers;
using AMANC_Inventory.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AMANC_Inventory.ViewModels
{
    public class InventoryViewModel : ViewModelBase
    {
        private UserModel? _usuarioActual;
        private bool _isLoading;
        private bool _isEditing;
        private string _searchText = "";

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

        // --- Propiedades agregadas para Header y Control de Perfil ---

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

        #endregion

        #region Métodos

        /// <summary>
        /// Inicializa los datos del usuario logueado y desencadena la carga del inventario.
        /// </summary>
        public void InicializarUsuario(UserModel usuario)
        {
            _usuarioActual = usuario;

            if (usuario != null)
            {
                // Si en tu UserModel la propiedad se llama Nombre/Apellido en lugar de Name, usa:
                // NombreUsuario = $"{usuario.Nombre} {usuario.Apellido}".Trim();
                NombreUsuario = !string.IsNullOrEmpty(usuario.Name) ? usuario.Name : "Usuario";

                // Si UserInitials no existe en tu UserModel, asigna una cadena por defecto o calcúlala
                UserInitials = !string.IsNullOrEmpty(usuario.UserInitials) ? usuario.UserInitials : "U";

                // Cambia ProfilePictureBase64 según el nombre exacto de la propiedad en tu UserModel
                // Ejemplos: usuario.ProfilePicturePath o usuario.FotoPerfil
                HasProfilePicture = !string.IsNullOrEmpty(usuario.ProfilePictureBase64);
                HasNoProfilePicture = !HasProfilePicture;
            }

            // Ejecuta la carga de datos
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