using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Media;
using AMANC_Inventory.Core.Utilities;
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

        private string _phone = "";
        public string Phone
        {
            get => _phone;
            set
            {
                string formatted = FormatPhoneNumber(value);
                SetProperty(ref _phone, formatted);
            }
        }

        private string _branch = "";
        public string Branch
        {
            get => _branch;
            set => SetProperty(ref _branch, value);
        }

        private string _department = "";
        public string Department
        {
            get => _department;
            set => SetProperty(ref _department, value);
        }

        public ObservableCollection<string> Branches { get; } = ProfileCatalogs.GetBranches();
        public ObservableCollection<string> Departments { get; } = ProfileCatalogs.GetDepartments();

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
            Phone = "";
            Branch = "";
            Department = "";
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

        private string FormatPhoneNumber(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "+";

            // Extraer únicamente los dígitos
            string digits = Regex.Replace(input, @"[^\d]", "");

            if (digits.Length == 0) return "+";

            // Límite máximo de 15 dígitos
            if (digits.Length > 15) digits = digits.Substring(0, 15);

            int ccLength = 0;

            // Determinar la longitud del código de país según la cantidad total de dígitos y prefijos
            if (digits.Length > 10)
            {
                if (digits.StartsWith("1"))
                {
                    // EE.UU. / Canadá (1 dígito)
                    ccLength = 1;
                }
                else if (digits.Length == 13 || digits.Length == 15)
                {
                    // Códigos de país de 3 dígitos (ej. +529, +735)
                    ccLength = 3;
                }
                else
                {
                    // Códigos de país de 2 dígitos (ej. +52 México)
                    ccLength = 2;
                }
            }

            string cc = ccLength > 0 ? digits.Substring(0, ccLength) : "";
            string local = ccLength > 0 ? digits.Substring(ccLength) : digits;

            // Ensamblar prefijo '+' y Código de País
            string formatted = string.IsNullOrEmpty(cc) ? "+" : $"+{cc} ";

            if (local.Length > 0)
            {
                // Área / LADA (2 dígitos)
                string area = local.Substring(0, Math.Min(2, local.Length));
                formatted += area;

                // Bloque central (4 dígitos)
                if (local.Length > 2)
                {
                    string mid = local.Substring(2, Math.Min(4, local.Length - 2));
                    formatted += $" {mid}";

                    // Bloque final (4 dígitos)
                    if (local.Length > 6)
                    {
                        string end = local.Substring(6, Math.Min(4, local.Length - 6));
                        formatted += $" {end}";

                        // Extra (hasta 2 dígitos adicionales)
                        if (local.Length > 10)
                        {
                            string extra = local.Substring(10, Math.Min(2, local.Length - 10));
                            formatted += $" {extra}";
                        }
                    }
                }
            }

            return formatted;
        }
    }
}