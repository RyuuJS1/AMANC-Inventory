using AMANC_Inventory.ViewModels;
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

            DataContext = _viewModel;
        }

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

        private void TxtDigito_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox tb) return;

            if (e.Key == Key.Delete)
            {
                _viewModel.LimpiarGuionesCodigo();
                TxtDigito1.Focus();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Left)
            {
                if (tb.Name == "TxtDigito2") TxtDigito1.Focus();
                else if (tb.Name == "TxtDigito3") TxtDigito2.Focus();
                else if (tb.Name == "TxtDigito4") TxtDigito3.Focus();
                else if (tb.Name == "TxtDigito5") TxtDigito4.Focus();
                else if (tb.Name == "TxtDigito6") TxtDigito5.Focus();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Right)
            {
                if (tb.Name == "TxtDigito1") TxtDigito2.Focus();
                else if (tb.Name == "TxtDigito2") TxtDigito3.Focus();
                else if (tb.Name == "TxtDigito3") TxtDigito4.Focus();
                else if (tb.Name == "TxtDigito4") TxtDigito5.Focus();
                else if (tb.Name == "TxtDigito5") TxtDigito6.Focus();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Back && string.IsNullOrEmpty(tb.Text))
            {
                if (tb.Name == "TxtDigito2") { TxtDigito1.Clear(); TxtDigito1.Focus(); }
                else if (tb.Name == "TxtDigito3") { TxtDigito2.Clear(); TxtDigito2.Focus(); }
                else if (tb.Name == "TxtDigito4") { TxtDigito3.Clear(); TxtDigito3.Focus(); }
                else if (tb.Name == "TxtDigito5") { TxtDigito4.Clear(); TxtDigito4.Focus(); }
                else if (tb.Name == "TxtDigito6") { TxtDigito5.Clear(); TxtDigito5.Focus(); }
                e.Handled = true;
            }
        }

        private void TxtDigito_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox tb) return;

            if (tb.Text.Length == 1)
            {
                if (tb.Name == "TxtDigito1") TxtDigito2.Focus();
                else if (tb.Name == "TxtDigito2") TxtDigito3.Focus();
                else if (tb.Name == "TxtDigito3") TxtDigito4.Focus();
                else if (tb.Name == "TxtDigito4") TxtDigito5.Focus();
                else if (tb.Name == "TxtDigito5") TxtDigito6.Focus();
            }
        }
    }
}