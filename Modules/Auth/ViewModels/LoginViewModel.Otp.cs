using System;
using System.Windows;
using System.Windows.Threading;

namespace AMANC_Inventory.Modules.Auth.ViewModels
{
    public partial class LoginViewModel
    {
        private readonly DispatcherTimer _temporizadorCodigo;
        private int _tiempoRestante;
        private string _codigoEsperado = "";

        private bool _isPanelCodigoVisible;
        public bool IsPanelCodigoVisible
        {
            get => _isPanelCodigoVisible;
            set => SetProperty(ref _isPanelCodigoVisible, value);
        }

        private string _digito1 = ""; public string Digito1 { get => _digito1; set { SetProperty(ref _digito1, value); IntentarValidarCodigo(); } }
        private string _digito2 = ""; public string Digito2 { get => _digito2; set { SetProperty(ref _digito2, value); IntentarValidarCodigo(); } }
        private string _digito3 = ""; public string Digito3 { get => _digito3; set { SetProperty(ref _digito3, value); IntentarValidarCodigo(); } }
        private string _digito4 = ""; public string Digito4 { get => _digito4; set { SetProperty(ref _digito4, value); IntentarValidarCodigo(); } }
        private string _digito5 = ""; public string Digito5 { get => _digito5; set { SetProperty(ref _digito5, value); IntentarValidarCodigo(); } }
        private string _digito6 = ""; public string Digito6 { get => _digito6; set { SetProperty(ref _digito6, value); IntentarValidarCodigo(); } }

        private int _barraTiempoMaximo = 300;
        public int BarraTiempoMaximo { get => _barraTiempoMaximo; set => SetProperty(ref _barraTiempoMaximo, value); }

        private int _barraTiempoValor = 300;
        public int BarraTiempoValor { get => _barraTiempoValor; set => SetProperty(ref _barraTiempoValor, value); }

        private string _tiempoRestanteTexto = "05:00";
        public string TiempoRestanteTexto { get => _tiempoRestanteTexto; set => SetProperty(ref _tiempoRestanteTexto, value); }

        private void MostrarPanelCodigo()
        {
            LimpiarGuionesCodigo();
            _tiempoRestante = 300;
            BarraTiempoMaximo = 300;
            BarraTiempoValor = 300;
            TiempoRestanteTexto = TimeSpan.FromSeconds(_tiempoRestante).ToString(@"mm\:ss");
            IsPanelCodigoVisible = true;
            _temporizadorCodigo.Start();
        }

        public void OcultarPanelCodigo()
        {
            _temporizadorCodigo.Stop();
            IsPanelCodigoVisible = false;
        }

        private void TemporizadorCodigo_Tick(object? sender, EventArgs e)
        {
            _tiempoRestante--;
            BarraTiempoValor = _tiempoRestante;
            TiempoRestanteTexto = TimeSpan.FromSeconds(Math.Max(0, _tiempoRestante)).ToString(@"mm\:ss");

            if (_tiempoRestante <= 0)
            {
                OcultarPanelCodigo();
                EsModoRestablecer = false;
                MessageBox.Show("El código ha expirado.", "Código Expirado", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void IntentarValidarCodigo()
        {
            string codigoIngresado = $"{Digito1}{Digito2}{Digito3}{Digito4}{Digito5}{Digito6}";
            if (codigoIngresado.Length == 6)
            {
                _ = ValidarCodigoIngresadoAsync(codigoIngresado);
            }
        }

        public void LimpiarGuionesCodigo()
        {
            Digito1 = ""; Digito2 = ""; Digito3 = "";
            Digito4 = ""; Digito5 = ""; Digito6 = "";
        }
    }
}