using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Core.Utilities;
using AMANC_Inventory.Modules.Auth.Interfaces;
using AMANC_Inventory.Modules.Auth.Services;
using AMANC_Inventory.Shared.Interfaces;
using AMANC_Inventory.Shared.Services;
using AMANC_Inventory.Users.Interfaces;
using AMANC_Inventory.Users.Models;
using AMANC_Inventory.Users.Services;
using AMANC_Inventory.Views;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace AMANC_Inventory.Modules.Auth.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        private string _emailARestablecer = "";

        public Action? RequestCloseAction { get; set; }
        public Action? OnTabChanged { get; set; }

        public ICommand SelectTabCommand { get; }
        public ICommand SubmitCommand { get; }
        public ICommand ForgotPasswordCommand { get; }
        public ICommand CloseNotificationCommand { get; }

        public LoginViewModel()
            : this(new UserService(), new AuthService(), new EmailService())
        {
        }

        public LoginViewModel(IUserService userService, IAuthService authService, IEmailService emailService)
        {
            _userService = userService;
            _authService = authService;
            _emailService = emailService;

            _temporizadorCodigo = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _temporizadorCodigo.Tick += TemporizadorCodigo_Tick;

            SelectTabCommand = new RelayCommand(p => SelectTab(p?.ToString()));
            SubmitCommand = new RelayCommand(async _ => await EjecutarAccionPrincipalAsync());
            ForgotPasswordCommand = new RelayCommand(async _ => await SolicitudRestablecerPasswordAsync());
            CloseNotificationCommand = new RelayCommand(_ => OcultarPanelCodigo());

            ActualizarEstadoPestañas();
        }

        private void SelectTab(string? tab)
        {
            EsModoRestablecer = false;
            EsModoLogin = tab == "Login";

            LimpiarCamposFormulario();
            OcultarPanelCodigo();
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

            if (EsModoLogin)
            {
                if (string.IsNullOrWhiteSpace(emailNorm) || string.IsNullOrWhiteSpace(Password))
                {
                    MessageBox.Show("Por favor, ingresa tu correo y contraseña.", "Campos Incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

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

                if (!usuarioActual.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show($"Acceso denegado. Estado de cuenta: {usuarioActual.Status}.", "Cuenta Deshabilitada", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }

                usuarioActual.LastLogin = DateTime.UtcNow;
                await _userService.UpdateUserProfileAsync(usuarioActual);

                var inventoryWindow = new InventoryWindow(usuarioActual);
                inventoryWindow.Show();

                RequestCloseAction?.Invoke();
            }
            else
            {
                // =========================================================
                // VALIDACIÓN ESTRICTA DE TODOS LOS CAMPOS PARA CREAR CUENTA
                // =========================================================
                if (string.IsNullOrWhiteSpace(Nombre))
                {
                    MessageBox.Show("Por favor, ingresa tu nombre completo.", "Campo Obligatorio", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Phone) || Regex.Replace(Phone, @"[^\d]", "").Length < 10)
                {
                    MessageBox.Show("Por favor, ingresa un número de teléfono válido (mínimo 10 dígitos).", "Campo Obligatorio", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Branch))
                {
                    MessageBox.Show("Por favor, selecciona una Sede / Sucursal.", "Campo Obligatorio", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Department))
                {
                    MessageBox.Show("Por favor, selecciona un Departamento.", "Campo Obligatorio", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(emailNorm) || !EsCorreoValido(emailNorm))
                {
                    MessageBox.Show("Por favor, ingresa un correo electrónico válido.", "Campo Obligatorio", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Password) || Password.Length <= 6)
                {
                    MessageBox.Show("La contraseña debe tener más de 6 dígitos.", "Campo Obligatorio", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Password != ConfirmPassword)
                {
                    MessageBox.Show("Las contraseñas no coinciden.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                bool existe = await _userService.EmailExistsAsync(emailNorm);
                if (existe)
                {
                    MessageBox.Show("El correo electrónico ya se encuentra registrado.", "Correo Registrado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _codigoEsperado = new Random().Next(100000, 999999).ToString();
                bool correoEnviado = await _emailService.EnviarCodigoConfirmacionAsync(emailNorm, _codigoEsperado);

                if (correoEnviado) MostrarPanelCodigo();
                else MessageBox.Show("No se pudo enviar el correo de verificación.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ValidarCodigoIngresadoAsync(string codigoIngresado)
        {
            try
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

                // REGISTRO CON DATOS OPERATIVOS COMPLETOS
                var nuevoUsuario = new UserModel
                {
                    Name = Nombre.Trim(),
                    Email = Email.Trim().ToLower(),
                    Phone = Phone.Trim(),
                    Branch = Branch,
                    Department = Department,
                    Status = "Active",
                    Role = "Volunteer",
                    IsProfileComplete = true
                };

                bool registrado = await _userService.RegisterUserAsync(nuevoUsuario, Password);

                if (registrado)
                {
                    MessageBox.Show($"¡Cuenta de {nuevoUsuario.Name} registrada con éxito!\n\nYa puedes iniciar sesión en el sistema.", "Registro Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);

                    LimpiarCamposFormulario();
                    EsModoLogin = true;
                }
                else
                {
                    MessageBox.Show("No se pudo completar el registro.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                string mensajeError = ex.Message.Contains("EMAIL_EXISTS", StringComparison.OrdinalIgnoreCase) ||
                                     ex.Message.Contains("EmailExists", StringComparison.OrdinalIgnoreCase)
                    ? "El correo electrónico ya se encuentra registrado en el sistema."
                    : $"Ocurrió un error durante el registro: {ex.Message}";

                MessageBox.Show(mensajeError, "Error de Registro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}