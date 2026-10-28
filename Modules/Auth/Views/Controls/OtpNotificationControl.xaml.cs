using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using AMANC_Inventory.Modules.Auth.ViewModels;

namespace AMANC_Inventory.Modules.Auth.Views.Controls
{
    public partial class OtpNotificationControl : UserControl
    {
        private bool _isDragging;
        private Point _anchorPoint;
        private Point _currentTransformPosition;

        public OtpNotificationControl()
        {
            InitializeComponent();
        }

        #region Eventos de Arrastre (Movimiento dentro de la Ventana)

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement element) return;

            _isDragging = true;
            Window parentWindow = Window.GetWindow(this);

            if (parentWindow != null)
            {
                _anchorPoint = e.GetPosition(parentWindow);
                _currentTransformPosition = new Point(transform.X, transform.Y);
                element.CaptureMouse();
            }
        }

        private void Header_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            Window parentWindow = Window.GetWindow(this);
            if (parentWindow == null) return;

            Point currentMousePosition = e.GetPosition(parentWindow);
            Vector delta = currentMousePosition - _anchorPoint;

            double targetX = _currentTransformPosition.X + delta.X;
            double targetY = _currentTransformPosition.Y + delta.Y;

            // LÍMITES: Evitar que el panel salga de los bordes de la ventana
            double maxShiftX = Math.Max(0, (parentWindow.ActualWidth - PanelNotificacion.ActualWidth) / 2.0);
            double maxShiftY = Math.Max(0, (parentWindow.ActualHeight - PanelNotificacion.ActualHeight) / 2.0);

            // Restringir dentro de los ejes X y Y
            transform.X = Math.Max(-maxShiftX, Math.Min(maxShiftX, targetX));
            transform.Y = Math.Max(-maxShiftY, Math.Min(maxShiftY, targetY));
        }

        private void Header_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                if (sender is FrameworkElement element)
                {
                    element.ReleaseMouseCapture();
                }
            }
        }

        #endregion

        #region Eventos de Redimensionamiento (Resize Grip)

        private void ResizeGrip_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double currentWidth = double.IsNaN(PanelNotificacion.Width) ? PanelNotificacion.ActualWidth : PanelNotificacion.Width;
            double currentHeight = double.IsNaN(PanelNotificacion.Height) ? PanelNotificacion.ActualHeight : PanelNotificacion.Height;

            double newWidth = currentWidth + e.HorizontalChange;
            double newHeight = currentHeight + e.VerticalChange;

            // Respeta los límites MinWidth/MaxWidth y MinHeight/MaxHeight configurados en el XAML
            PanelNotificacion.Width = Math.Max(PanelNotificacion.MinWidth, Math.Min(PanelNotificacion.MaxWidth, newWidth));
            PanelNotificacion.Height = Math.Max(PanelNotificacion.MinHeight, Math.Min(PanelNotificacion.MaxHeight, newHeight));
        }

        #endregion

        #region Navegación y Manejo de Dígitos OTP

        private void TxtDigito_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox tb) return;

            if (e.Key == Key.Delete)
            {
                if (DataContext is LoginViewModel vm)
                {
                    vm.LimpiarGuionesCodigo();
                }
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

        #endregion
    }
}