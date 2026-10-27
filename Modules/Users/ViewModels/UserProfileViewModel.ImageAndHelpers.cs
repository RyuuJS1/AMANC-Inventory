using Microsoft.Win32;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AMANC_Inventory.Users.ViewModels
{
    public partial class UserProfileViewModel
    {
        private string? _profilePictureBase64;
        public string? ProfilePictureBase64
        {
            get => _profilePictureBase64;
            set
            {
                if (SetProperty(ref _profilePictureBase64, value))
                {
                    _currentUser.ProfilePictureBase64 = value;
                    OnPropertyChanged(nameof(HasProfilePicture));
                    OnPropertyChanged(nameof(HasNoProfilePicture));
                    OnPropertyChanged(nameof(ProfileImageSource));
                }
            }
        }

        public bool HasProfilePicture => ProfileImageSource != null;
        public bool HasNoProfilePicture => !HasProfilePicture;

        public BitmapImage? ProfileImageSource
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ProfilePictureBase64)) return null;

                try
                {
                    string base64Data = ProfilePictureBase64.Contains(",")
                        ? ProfilePictureBase64.Split(',')[1]
                        : ProfilePictureBase64;

                    byte[] binaryData = Convert.FromBase64String(base64Data);

                    using (MemoryStream ms = new MemoryStream(binaryData))
                    {
                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.StreamSource = ms;
                        bi.EndInit();
                        bi.Freeze();
                        return bi;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        private string CompressAndResizeImageToBase64(string filePath, int targetWidth = 256)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(filePath);
            bitmap.DecodePixelWidth = targetWidth;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            JpegBitmapEncoder encoder = new JpegBitmapEncoder { QualityLevel = 80 };
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        private async void SelectPicture()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Imágenes (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg" };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string base64Compressed = CompressAndResizeImageToBase64(openFileDialog.FileName);
                    ProfilePictureBase64 = base64Compressed;

                    bool guardado = await _userService.UpdateUserProfileAsync(_currentUser);

                    if (guardado)
                    {
                        MessageBox.Show("Foto de perfil actualizada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("La foto se cargó en pantalla pero no se pudo sincronizar con la base de datos.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al procesar la imagen: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task RemovePictureAsync()
        {
            ProfilePictureBase64 = null;
            _currentUser.ProfilePictureBase64 = null;

            OnPropertyChanged(nameof(ProfileImageSource));
            OnPropertyChanged(nameof(HasProfilePicture));
            OnPropertyChanged(nameof(HasNoProfilePicture));

            await _userService.UpdateUserProfileAsync(_currentUser);
        }

        private void UpdateAvailability()
        {
            Availability = $"{EntryTime} - {ExitTime}";
        }

        private void ParseInitialAvailability()
        {
            if (!string.IsNullOrWhiteSpace(_currentUser.Availability) && _currentUser.Availability.Contains('-'))
            {
                string[] parts = _currentUser.Availability.Split('-');
                if (parts.Length == 2)
                {
                    _entryTime = parts[0].Trim();
                    _exitTime = parts[1].Trim();
                }
            }
        }

        private string FormatPhoneNumber(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            bool hasPlus = input.StartsWith("+");
            string digits = Regex.Replace(input, @"[^\d]", "");

            if (digits.Length == 0) return hasPlus ? "+" : string.Empty;

            if (digits.Length > 13) digits = digits.Substring(0, 13);

            if (hasPlus || digits.Length > 10)
            {
                int ccLength = digits.Length > 10 ? digits.Length - 10 : Math.Min(2, digits.Length);
                if (ccLength > 3) ccLength = 3;

                string cc = digits.Substring(0, ccLength);
                string local = digits.Substring(ccLength);

                string formatted = $"+{cc}";

                if (local.Length > 0)
                {
                    string area = local.Substring(0, Math.Min(2, local.Length));
                    formatted += $" {area}";

                    if (local.Length > 2)
                    {
                        string mid = local.Substring(2, Math.Min(4, local.Length - 2));
                        formatted += $" {mid}";

                        if (local.Length > 6)
                        {
                            string end = local.Substring(6, Math.Min(4, local.Length - 6));
                            formatted += $" {end}";
                        }
                    }
                }
                return formatted;
            }
            else
            {
                string formatted = digits.Substring(0, Math.Min(2, digits.Length));

                if (digits.Length > 2)
                {
                    string mid = digits.Substring(2, Math.Min(4, digits.Length - 2));
                    formatted += $" {mid}";

                    if (digits.Length > 6)
                    {
                        string end = digits.Substring(6, Math.Min(4, digits.Length - 6));
                        formatted += $" {end}";
                    }
                }
                return formatted;
            }
        }
    }
}