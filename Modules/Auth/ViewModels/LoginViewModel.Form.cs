using System;
using System.Windows.Media;
using AMANC_Inventory.Modules.Auth.Helpers;

namespace AMANC_Inventory.Modules.Auth.ViewModels
{
    public partial class LoginViewModel
    {
        private string _nombre = "";
        public string Nombre
        {
            get => _nombre;
            set => SetProperty(ref _nombre, value?.ToUpper() ?? "");
        }

        private string _email = "";
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value?.ToLower().Replace(" ", "") ?? "");
        }

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

        #region Propiedades de Validación de Contraseña

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

        public void LimpiarCamposFormulario()
        {
            Nombre = "";
            Email = "";
            Password = "";
            ConfirmPassword = "";

            OnTabChanged?.Invoke();
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
            var result = PasswordValidator.EvaluateStrength(Password, EsModoLogin);
            NivelSeguridadVisible = result.IsVisible;
            if (result.IsVisible)
            {
                NivelSeguridadTexto = result.Text;
                NivelSeguridadColor = result.Color;
            }
        }

        private void ValidarCoincidenciaPassword()
        {
            var result = PasswordValidator.EvaluateMatch(Password, ConfirmPassword, EsModoLogin);
            CoincidenciaPasswordVisible = result.IsVisible;
            if (result.IsVisible)
            {
                CoincidenciaPasswordTexto = result.Text;
                CoincidenciaPasswordColor = result.Color;
            }
        }
    }
}