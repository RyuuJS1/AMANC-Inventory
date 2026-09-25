using AMANC_Inventory.Models;
using AMANC_Inventory.Services;
using AMANC_Inventory.ViewModels;
using System.Windows;

namespace AMANC_Inventory.Views
{
    public partial class InventoryWindow : Window
    {
        private InventoryViewModel _mainViewModel;

        public InventoryWindow()
        {
            InitializeComponent();
            _mainViewModel = new InventoryViewModel();
            DataContext = _mainViewModel;
        }

        public void InicializarPerfil(UserModel usuarioLogueado)
        {
            var userService = new UserService();

            // 1. Inyectar la información del usuario al ViewModel Principal de la Ventana
            _mainViewModel.InicializarUsuario(usuarioLogueado);

            // 2. Asignar ViewModel específico para la vista interna del perfil
            UserProfileViewControl.DataContext = new UserProfileViewModel(usuarioLogueado, userService);

            // 3. Control del Overlay de Primera Vez / Perfil Incompleto
            if (usuarioLogueado.IsProfileComplete)
            {
                CompleteOverlayControl.Visibility = Visibility.Collapsed;
            }
            else
            {
                var overlayVM = new CompleteProfileOverlayViewModel(usuarioLogueado, userService);

                overlayVM.OnProfileCompleted = () =>
                {
                    CompleteOverlayControl.Visibility = Visibility.Collapsed;

                    // Actualizar estado del usuario en todos los ViewModels
                    usuarioLogueado.IsProfileComplete = true;
                    _mainViewModel.InicializarUsuario(usuarioLogueado);
                    UserProfileViewControl.DataContext = new UserProfileViewModel(usuarioLogueado, userService);
                };

                CompleteOverlayControl.DataContext = overlayVM;
                CompleteOverlayControl.Visibility = Visibility.Visible;
            }
        }
    }
}