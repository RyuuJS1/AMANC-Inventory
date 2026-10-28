using AMANC_Inventory.Modules.Auth.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AMANC_Inventory
{
    public partial class MainWindow : Window
    {
        private readonly LoginViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();

            // Asignamos la acción para que el ViewModel solicite cerrar esta ventana
            _viewModel.RequestCloseAction = () => this.Close();

            // Suscripción para limpiar las PasswordBox al cambiar de pestaña
            _viewModel.OnTabChanged = LimpiarCamposPassword;

            DataContext = _viewModel;
        }

        // =========================================================
        // VALIDACIÓN DE CORREO (SIN ESPACIOS)
        // =========================================================

        private void TxtEmail_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true; // Cancela el pulso de la tecla Espacio
            }
        }

        private void TxtEmail_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Contains(" "))
            {
                e.Handled = true; // Cancela si se intenta pegar/escribir un espacio
            }
        }

        // =========================================================
        // VALIDACIÓN DE TELEFONO (SIN ESPACIOS)
        // =========================================================

        private void TxtPhone_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                // Al bajar o hacer clic en el campo, mueve el cursor al final del número
                tb.CaretIndex = tb.Text.Length;
            }
        }

        private void TxtPhone_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb && tb.Text.StartsWith("+"))
            {
                if (tb.CaretIndex == 0)
                {
                    tb.CaretIndex = tb.Text.Length;
                }
            }
        }

        // =========================================================
        // MANEJO DE CONTRASEÑAS
        // =========================================================

        private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                _viewModel.Password = passwordBox.Password;
            }
        }

        private void TxtConfirmPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                _viewModel.ConfirmPassword = passwordBox.Password;
            }
        }

        public void LimpiarCamposPassword()
        {
            TxtPassword.Password = string.Empty;
            TxtConfirmPassword.Password = string.Empty;
        }
    }
}