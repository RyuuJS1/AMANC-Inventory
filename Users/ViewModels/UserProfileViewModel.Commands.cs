using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Users.Models;
using System.Threading.Tasks;
using System.Windows;

namespace AMANC_Inventory.Users.ViewModels
{
    public partial class UserProfileViewModel
    {
        private void InicializarComandos()
        {
            CloseProfileCommand = new RelayCommand(_ => CerrarPerfil());
            ChangeProfilePictureCommand = new RelayCommand(_ => SelectPicture());
            RemoveProfilePictureCommand = new RelayCommand(async _ => await RemovePictureAsync());
            EditProfileCommand = new RelayCommand(_ => StartEditing());
            CancelEditCommand = new RelayCommand(_ => CancelEditing());
            SaveChangesCommand = new RelayCommand(async _ => await SaveChangesAsync());
            ChangePasswordCommand = new RelayCommand(async _ => await RequestPasswordResetAsync());
            LogoutCommand = new RelayCommand(_ => Logout());
        }

        private void CerrarPerfil()
        {
            IsProfileDetailsOpen = false;
            OnReturnToInventoryRequested?.Invoke();
        }

        private async Task SaveChangesAsync()
        {
            _currentUser.ProfilePictureBase64 = ProfilePictureBase64;

            bool ok = await _userService.UpdateUserProfileAsync(_currentUser);
            if (ok)
            {
                IsEditing = false;
                _userBackup = null;

                OnPropertyChanged(nameof(ProfileImageSource));
                OnPropertyChanged(nameof(HasProfilePicture));
                OnPropertyChanged(nameof(HasNoProfilePicture));

                MessageBox.Show("Perfil y foto de perfil guardados con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("No se pudieron guardar los cambios. Verifique su conexión a internet.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Logout()
        {
            MessageBoxResult result = MessageBox.Show(
                "¿Estás seguro de que deseas cerrar sesión?",
                "Cerrar Sesión",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                OnLogoutRequested?.Invoke();
            }
        }

        private void StartEditing()
        {
            _userBackup = new UserModel
            {
                Name = _currentUser.Name,
                Phone = _currentUser.Phone,
                Branch = _currentUser.Branch,
                Department = _currentUser.Department,
                City = _currentUser.City,
                BirthDate = _currentUser.BirthDate,
                EmergencyContactName = _currentUser.EmergencyContactName,
                EmergencyContactRelationship = _currentUser.EmergencyContactRelationship,
                EmergencyPhone = _currentUser.EmergencyPhone,
                Availability = _currentUser.Availability,
                Skills = _currentUser.Skills,
                ShirtSize = _currentUser.ShirtSize,
                ProfilePictureBase64 = ProfilePictureBase64
            };

            _entryTimeBackup = _entryTime;
            _exitTimeBackup = _exitTime;

            IsEditing = true;
        }

        private void CancelEditing()
        {
            if (_userBackup != null)
            {
                _currentUser.Name = _userBackup.Name;
                _currentUser.Phone = _userBackup.Phone;
                _currentUser.Branch = _userBackup.Branch;
                _currentUser.Department = _userBackup.Department;
                _currentUser.City = _userBackup.City;
                _currentUser.BirthDate = _userBackup.BirthDate;
                _currentUser.EmergencyContactName = _userBackup.EmergencyContactName;
                _currentUser.EmergencyContactRelationship = _userBackup.EmergencyContactRelationship;
                _currentUser.EmergencyPhone = _userBackup.EmergencyPhone;
                _currentUser.Availability = _userBackup.Availability;
                _currentUser.Skills = _userBackup.Skills;
                _currentUser.ShirtSize = _userBackup.ShirtSize;

                _entryTime = _entryTimeBackup;
                _exitTime = _exitTimeBackup;
                ProfilePictureBase64 = _userBackup.ProfilePictureBase64;

                RefreshAllProperties();
            }

            IsEditing = false;
        }

        private void RefreshAllProperties()
        {
            OnPropertyChanged(nameof(NombreUsuario));
            OnPropertyChanged(nameof(TelefonoUsuario));
            OnPropertyChanged(nameof(Branch));
            OnPropertyChanged(nameof(Department));
            OnPropertyChanged(nameof(City));
            OnPropertyChanged(nameof(BirthDate));
            OnPropertyChanged(nameof(EmergencyContactName));
            OnPropertyChanged(nameof(EmergencyContactRelationship));
            OnPropertyChanged(nameof(EmergencyContactPhone));
            OnPropertyChanged(nameof(EntryTime));
            OnPropertyChanged(nameof(ExitTime));
            OnPropertyChanged(nameof(Availability));
            OnPropertyChanged(nameof(Skills));
            OnPropertyChanged(nameof(ShirtSize));
            OnPropertyChanged(nameof(UserInitials));
        }

        private async Task RequestPasswordResetAsync()
        {
            string correo = CorreoUsuario?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(correo))
            {
                MessageBox.Show("No se encontró un correo electrónico asociado a este perfil.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool enviado = await _authService.SendPasswordResetEmailAsync(correo);

            if (enviado)
            {
                MessageBox.Show($"Se han enviado las instrucciones de restablecimiento al correo:\n{correo}\n\nRevisa tu bandeja de entrada o la carpeta de SPAM.",
                                "Correo Enviado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Ocurrió un error al intentar enviar el correo. Verifica tu conexión e intenta nuevamente.",
                                "Error de Envío", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}