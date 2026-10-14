using AMANC_Inventory.Inventory.ViewModels;
using AMANC_Inventory.Users.Services;
using AMANC_Inventory.Users.ViewModels;
using AMANC_Inventory.Shared.Services;
using AMANC_Inventory.Users.Models;
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
            _mainViewModel.CerrarPerfil(); // Asegura que el estado en InventoryViewModel inicie en false

            // Función local para manejar el cierre de sesión
            void CerrarSesion()
            {
                var loginWindow = new MainWindow();
                loginWindow.Show();
                this.Close();
            }

            // 2. Crear ViewModel del perfil
            var profileVM = new UserProfileViewModel(usuarioLogueado, userService, emailService);
            profileVM.IsProfileDetailsOpen = false; // Asegura que el perfil inicie CERRADO al iniciar sesión

            // --- SINCRONIZACIÓN BIDI RECCIONAL DE APERTURA DE PERFIL ---
            // Cuando el Header (InventoryViewModel) abre el perfil, notifica a profileVM
            _mainViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(InventoryViewModel.IsProfileDetailsOpen))
                {
                    profileVM.IsProfileDetailsOpen = _mainViewModel.IsProfileDetailsOpen;
                }
            };

            // Cuando profileVM se cierra (vía botón "Volver al Inventario"), notifica a InventoryViewModel
            profileVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(UserProfileViewModel.IsProfileDetailsOpen))
                {
                    _mainViewModel.IsProfileDetailsOpen = profileVM.IsProfileDetailsOpen;
                }
            };

            // Asignar acciones de retorno y cierre de sesión
            profileVM.OnReturnToInventoryRequested = () =>
            {
                profileVM.IsProfileDetailsOpen = false;
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
                    newProfileVM.IsProfileDetailsOpen = false;

                    _mainViewModel.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(InventoryViewModel.IsProfileDetailsOpen))
                        {
                            newProfileVM.IsProfileDetailsOpen = _mainViewModel.IsProfileDetailsOpen;
                        }
                    };

                    newProfileVM.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(UserProfileViewModel.IsProfileDetailsOpen))
                        {
                            _mainViewModel.IsProfileDetailsOpen = newProfileVM.IsProfileDetailsOpen;
                        }
                    };

                    newProfileVM.OnReturnToInventoryRequested = () => newProfileVM.IsProfileDetailsOpen = false;
                    newProfileVM.OnLogoutRequested = CerrarSesion;

                    UserProfileViewControl.DataContext = newProfileVM;
                };

                CompleteOverlayControl.DataContext = overlayVM;
                CompleteOverlayControl.Visibility = Visibility.Visible;
            }
        }
    }
}