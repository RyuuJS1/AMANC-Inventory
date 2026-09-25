using AMANC_Inventory.Helpers;
using AMANC_Inventory.Interfaces;
using AMANC_Inventory.Models;
using AMANC_Inventory.Services;
using AMANC_Inventory.Views;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace AMANC_Inventory.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        private readonly DispatcherTimer _temporizadorCodigo;
        private int _tiempoRestante;
        private string _codigoEsperado = "";
        private string _emailARestablecer = "";

        // Acción opcional para cerrar la ventana actual de Login
        public Action? RequestCloseAction { get; set; }

        public LoginViewModel()
        {
            _userService = new UserService();
            _authService = new AuthService();
            _emailService = new EmailService();

            _temporizadorCodigo = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _temporizadorCodigo.Tick += TemporizadorCodigo_Tick;

            SelectTabCommand = new RelayCommand(p => SelectTab(p?.ToString()));
            SubmitCommand = new RelayCommand(async p => await EjecutarAccionPrincipalAsync());
            ForgotPasswordCommand = new RelayCommand(async p => await SolicitudRestablecerPasswordAsync());
            CloseNotificationCommand = new RelayCommand(p => OcultarPanelCodigo());

            ActualizarEstadoPestañas();
        }

        #region Propiedades Formulario

        private string _nombre = "";
        public string Nombre { get => _nombre; set => SetProperty(ref _nombre, value?.ToUpper() ?? ""); }

        private string _email = "";
        public string Email { get => _email; set => SetProperty(ref _email, value); }

        private string _password = "";
        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    ValidarSeguridadPassword();
                    ValidarCoincidenciaPassword();
                }
            }
        }

        private string _confirmPassword = "";
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (SetProperty(ref _confirmPassword, value))
                {
                    ValidarCoincidenciaPassword();
                }
            }
        }

        private bool _esModoLogin = true;
        public bool EsModoLogin
        {
            get => _esModoLogin;
            set
            {
                if (SetProperty(ref _esModoLogin, value))
                {
                    OnPropertyChanged(nameof(EsModoRegistro));
                    ActualizarEstadoPestañas();
                }
            }
        }

        public bool EsModoRegistro => !EsModoLogin;

        private bool _esModoRestablecer;
        public bool EsModoRestablecer
        {
            get => _esModoRestablecer;
            set => SetProperty(ref _esModoRestablecer, value);
        }

        private string _textoBotonPrincipal = "Iniciar Sesión";
        public string TextoBotonPrincipal
        {
            get => _textoBotonPrincipal;
            set => SetProperty(ref _textoBotonPrincipal, value);
        }

        #endregion

        #region Validaciones y Visibilidad

        private string _nivelSeguridadTexto = "";
        public string NivelSeguridadTexto { get => _nivelSeguridadTexto; set => SetProperty(ref _nivelSeguridadTexto, value); }

        private Brush _nivelSeguridadColor = Brushes.Gray;
        public Brush NivelSeguridadColor { get => _nivelSeguridadColor; set => SetProperty(ref _nivelSeguridadColor, value); }

        private bool _nivelSeguridadVisible;
        public bool NivelSeguridadVisible { get => _nivelSeguridadVisible; set => SetProperty(ref _nivelSeguridadVisible, value); }

        private string _coincidenciaPasswordTexto = "";
        public string CoincidenciaPasswordTexto { get => _coincidenciaPasswordTexto; set => SetProperty(ref _coincidenciaPasswordTexto, value); }

        private Brush _coincidenciaPasswordColor = Brushes.Gray;
        public Brush CoincidenciaPasswordColor { get => _coincidenciaPasswordColor; set => SetProperty(ref _coincidenciaPasswordColor, value); }

        private bool _coincidenciaPasswordVisible;
        public bool CoincidenciaPasswordVisible { get => _coincidenciaPasswordVisible; set => SetProperty(ref _coincidenciaPasswordVisible, value); }

        #endregion

        #region Panel OTP

        private bool _isPanelCodigoVisible;
        public bool IsPanelCodigoVisible { get => _isPanelCodigoVisible; set => SetProperty(ref _isPanelCodigoVisible, value); }

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

        #endregion

        #region Comandos

        public ICommand SelectTabCommand { get; }
        public ICommand SubmitCommand { get; }
        public ICommand ForgotPasswordCommand { get; }
        public ICommand CloseNotificationCommand { get; }

        #endregion

        #region Lógica Principal

        private void SelectTab(string? tab)
        {
            EsModoRestablecer = false;
            EsModoLogin = tab == "Login";
            OcultarPanelCodigo();
        }

        private void ActualizarEstadoPestañas()
        {
            if (EsModoLogin)
            {
                NivelSeguridadVisible = false;
                CoincidenciaPasswordVisible = false;
                TextoBotonPrincipal = "Iniciar Sesión";
            }
            else
            {
                ValidarSeguridadPassword();
                ValidarCoincidenciaPassword();
                TextoBotonPrincipal = "Crear Cuenta";
            }
        }

        private void ValidarSeguridadPassword()
        {
            if (EsModoLogin || string.IsNullOrEmpty(Password)) { NivelSeguridadVisible = false; return; }
            NivelSeguridadVisible = true;
            int puntos = 0;
            if (Password.Length >= 8) puntos++;
            if (Regex.IsMatch(Password, @"[A-Z]")) puntos++;
            if (Regex.IsMatch(Password, @"[0-9]")) puntos++;
            if (Regex.IsMatch(Password, @"[\W_]")) puntos++;

            if (puntos <= 1) { NivelSeguridadTexto = "Nivel: Poco segura"; NivelSeguridadColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B6B")); }
            else if (puntos <= 3) { NivelSeguridadTexto = "Nivel: Intermedio"; NivelSeguridadColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD166")); }
            else { NivelSeguridadTexto = "Nivel: Segura"; NivelSeguridadColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#06D6A0")); }
        }

        private void ValidarCoincidenciaPassword()
        {
            if (EsModoLogin || string.IsNullOrEmpty(ConfirmPassword)) { CoincidenciaPasswordVisible = false; return; }
            CoincidenciaPasswordVisible = true;
            if (Password == ConfirmPassword)
            {
                CoincidenciaPasswordTexto = "Las contraseñas coinciden";
                CoincidenciaPasswordColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#06D6A0"));
            }
            else
            {
                CoincidenciaPasswordTexto = "Las contraseñas no coinciden";
                CoincidenciaPasswordColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B6B"));
            }
        }

        private bool EsCorreoValido(string email) => Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);

        private async Task SolicitudRestablecerPasswordAsync()
        {
            string email = Email.Trim();
            if (string.IsNullOrWhiteSpace(email) || !EsCorreoValido(email))
            {
                MessageBox.Show("Ingresa un correo electrónico válido.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!NetworkService.HasInternetConnection())
            {
                MessageBox.Show("Se requiere conexión a internet.", "Sin Conexión", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool existeCorreo = await _userService.EmailExistsAsync(email);
            if (!existeCorreo)
            {
                MessageBox.Show("El correo ingresado no se encuentra registrado.", "No Encontrado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _emailARestablecer = email;
            _codigoEsperado = new Random().Next(100000, 999999).ToString();
            EsModoRestablecer = true;

            bool enviado = await _emailService.EnviarCodigoRestablecimientoAsync(email, _codigoEsperado);
            if (enviado) MostrarPanelCodigo();
            else MessageBox.Show("No se pudo enviar el correo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private async Task EjecutarAccionPrincipalAsync()
        {
            string emailNorm = Email.Trim().ToLower();

            if (!NetworkService.HasInternetConnection())
            {
                MessageBox.Show("Se requiere conexión a internet.", "Sin Conexión", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(emailNorm) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Por favor, ingresa correo y contraseña.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (EsModoLogin)
            {
                TextoBotonPrincipal = "Verificando...";
                bool credencialesValidas = await _userService.ValidateCredentialsAsync(emailNorm, Password);

                if (!credencialesValidas)
                {
                    TextoBotonPrincipal = "Iniciar Sesión";
                    MessageBox.Show("Correo o contraseña incorrectos.", "Error de Autenticación", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var usuarioActual = await _userService.GetUserByEmailAsync(emailNorm);
                TextoBotonPrincipal = "Iniciar Sesión";

                if (usuarioActual == null)
                {
                    MessageBox.Show("No se encontraron los datos del usuario.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // VALIDACIÓN DE ESTATUS
                if (!usuarioActual.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"Acceso denegado. Estado de cuenta: {usuarioActual.Status}.", "Cuenta Deshabilitada", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }

                // SESIÓN CORRECTA
                usuarioActual.LastLogin = DateTime.UtcNow;
                await _userService.UpdateUserProfileAsync(usuarioActual);

                // Abrir la ventana de Inventarios (sin pasar parámetros)
                var inventoryWindow = new InventoryWindow(usuarioActual);
                inventoryWindow.Show();

                // Cerrar la ventana actual de Login
                RequestCloseAction?.Invoke();
            }
            else
            {
                // REGISTRO
                if (string.IsNullOrWhiteSpace(Nombre))
                {
                    MessageBox.Show("Por favor, ingresa tu nombre completo.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Password != ConfirmPassword)
                {
                    MessageBox.Show("Las contraseñas no coinciden.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                bool existe = await _userService.EmailExistsAsync(emailNorm);
                if (existe)
                {
                    MessageBox.Show("El correo ya está registrado.", "Correo Registrado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _codigoEsperado = new Random().Next(100000, 999999).ToString();
                bool correoEnviado = await _emailService.EnviarCodigoConfirmacionAsync(emailNorm, _codigoEsperado);

                if (correoEnviado) MostrarPanelCodigo();
                else MessageBox.Show("No se pudo enviar el correo de verificación.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Gestión Código OTP

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

        private async Task ValidarCodigoIngresadoAsync(string codigoIngresado)
        {
            if (codigoIngresado != _codigoEsperado)
            {
                MessageBox.Show("El código ingresado es incorrecto.", "Código Incorrecto", MessageBoxButton.OK, MessageBoxImage.Error);
                LimpiarGuionesCodigo();
                return;
            }

            OcultarPanelCodigo();

            if (EsModoRestablecer)
            {
                EsModoRestablecer = false;
                bool enviado = await _authService.SendPasswordResetEmailAsync(_emailARestablecer);

                if (enviado)
                    MessageBox.Show("¡Código verificado! Revisa tu correo para restablecer tu contraseña.", "Enlace Enviado", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // MODO REGISTRO: Cambiamos 'Pending' por 'Active' temporalmente para pruebas
            var nuevoUsuario = new UserModel
            {
                Name = Nombre.Trim(),
                Email = Email.Trim().ToLower(),
                Status = "Active", // <--- CAMBIO AQUÍ (Antes decía "Pending")
                Role = "Volunteer",
                IsProfileComplete = false
            };

            bool registrado = await _userService.RegisterUserAsync(nuevoUsuario, Password);

            if (registrado)
            {
                MessageBox.Show($"¡Cuenta de {nuevoUsuario.Name} registrada y activada con éxito!\n\nYa puedes iniciar sesión directamente.", "Registro Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                Nombre = ""; Email = ""; Password = ""; ConfirmPassword = "";
                EsModoLogin = true;
            }
            else
            {
                MessageBox.Show("No se pudo completar el registro.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LimpiarGuionesCodigo()
        {
            Digito1 = ""; Digito2 = ""; Digito3 = "";
            Digito4 = ""; Digito5 = ""; Digito6 = "";
        }

        #endregion
    }
}