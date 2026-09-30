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

        public InventoryWindow(UserModel usuarioLogueado) : this()
        {
            InicializarPerfil(usuarioLogueado);
        }

        public void InicializarPerfil(UserModel usuarioLogueado)
        {
            var userService = new UserService();
            var emailService = new EmailService();

            // 1. Inyectar la información del usuario al ViewModel Principal
            _mainViewModel.InicializarUsuario(usuarioLogueado);

            // Función local para manejar el cierre de sesión
            void CerrarSesion()
            {
                var loginWindow = new AMANC_Inventory.MainWindow();
                loginWindow.Show();
                this.Close();
            }

            // 2. Crear ViewModel del perfil
            var profileVM = new UserProfileViewModel(usuarioLogueado, userService, emailService);

            // Asignar acciones
            profileVM.OnReturnToInventoryRequested = () =>
            {
                _mainViewModel.CerrarPerfil();
            };
            profileVM.OnLogoutRequested = CerrarSesion;

            UserProfileViewControl.DataContext = profileVM;

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

                    usuarioLogueado.IsProfileComplete = true;
                    _mainViewModel.InicializarUsuario(usuarioLogueado);

                    var newProfileVM = new UserProfileViewModel(usuarioLogueado, userService, emailService);
                    newProfileVM.OnReturnToInventoryRequested = () => _mainViewModel.CerrarPerfil();
                    newProfileVM.OnLogoutRequested = CerrarSesion;

                    UserProfileViewControl.DataContext = newProfileVM;
                };

                CompleteOverlayControl.DataContext = overlayVM;
                CompleteOverlayControl.Visibility = Visibility.Visible;
            }
        }
    }
}