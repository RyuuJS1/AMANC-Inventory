using System.Text.RegularExpressions;
using System.Windows.Media;

namespace AMANC_Inventory.Modules.Auth.Helpers
{
    public class PasswordValidationResult
    {
        public string Text { get; set; } = string.Empty;
        public Brush Color { get; set; } = Brushes.Gray;
        public bool IsVisible { get; set; }
    }

    public static class PasswordValidator
    {
        public static PasswordValidationResult EvaluateStrength(string password, bool isLoginMode)
        {
            if (isLoginMode || string.IsNullOrEmpty(password))
            {
                return new PasswordValidationResult { IsVisible = false };
            }

            if (password.Length <= 6)
            {
                return new PasswordValidationResult
                {
                    IsVisible = true,
                    Text = "La contraseña debe tener más de 6 dígitos",
                    Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B6B"))
                };
            }

            int points = 0;
            if (password.Length >= 8) points++;
            if (Regex.IsMatch(password, @"[A-Z]")) points++;
            if (Regex.IsMatch(password, @"[0-9]")) points++;
            if (Regex.IsMatch(password, @"[\W_]")) points++;

            if (points <= 1)
            {
                return new PasswordValidationResult
                {
                    IsVisible = true,
                    Text = "Nivel: Poco segura",
                    Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B6B"))
                };
            }
            if (points <= 3)
            {
                return new PasswordValidationResult
                {
                    IsVisible = true,
                    Text = "Nivel: Intermedio",
                    Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD166"))
                };
            }

            return new PasswordValidationResult
            {
                IsVisible = true,
                Text = "Nivel: Segura",
                Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#06D6A0"))
            };
        }

        public static PasswordValidationResult EvaluateMatch(string password, string confirmPassword, bool isLoginMode)
        {
            if (isLoginMode || string.IsNullOrEmpty(confirmPassword))
            {
                return new PasswordValidationResult { IsVisible = false };
            }

            if (password == confirmPassword)
            {
                return new PasswordValidationResult
                {
                    IsVisible = true,
                    Text = "Las contraseñas coinciden",
                    Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#06D6A0"))
                };
            }

            return new PasswordValidationResult
            {
                IsVisible = true,
                Text = "Las contraseñas no coinciden",
                Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B6B"))
            };
        }
    }
}