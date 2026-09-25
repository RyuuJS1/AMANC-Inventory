using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AMANC_Inventory.Helpers;
using AMANC_Inventory.Interfaces;
using AMANC_Inventory.Models;
using Microsoft.Win32;

namespace AMANC_Inventory.ViewModels
{
    public class CompleteProfileOverlayViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        private readonly UserModel _currentUser;

        public Action? OnProfileCompleted { get; set; }

        public ObservableCollection<string> Branches { get; }
        public ObservableCollection<string> Departments { get; }

        public string UserInitials => _currentUser.UserInitials;

        private string? _profilePictureBase64;
        public string? ProfilePictureBase64
        {
            get => _profilePictureBase64;
            set
            {
                if (SetProperty(ref _profilePictureBase64, value))
                {
                    OnPropertyChanged(nameof(HasProfilePicture));
                    OnPropertyChanged(nameof(HasNoProfilePicture));
                    OnPropertyChanged(nameof(ProfileImageSource));
                }
            }
        }

        public bool HasProfilePicture => !string.IsNullOrEmpty(ProfilePictureBase64);
        public bool HasNoProfilePicture => !HasProfilePicture;

        public BitmapImage? ProfileImageSource
        {
            get
            {
                if (string.IsNullOrEmpty(ProfilePictureBase64)) return null;
                try
                {
                    byte[] binaryData = Convert.FromBase64String(ProfilePictureBase64);
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = new MemoryStream(binaryData);
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.EndInit();
                    return bi;
                }
                catch { return null; }
            }
        }

        public string Phone { get => _currentUser.Phone; set { _currentUser.Phone = value; OnPropertyChanged(); } }
        public string EmergencyPhone { get => _currentUser.EmergencyPhone; set { _currentUser.EmergencyPhone = value; OnPropertyChanged(); } }
        public string EmergencyContactName { get => _currentUser.EmergencyContactName; set { _currentUser.EmergencyContactName = value; OnPropertyChanged(); } }
        public string Branch { get => _currentUser.Branch; set { _currentUser.Branch = value; OnPropertyChanged(); } }
        public string Department { get => _currentUser.Department; set { _currentUser.Department = value; OnPropertyChanged(); } }
        public string City { get => _currentUser.City; set { _currentUser.City = value; OnPropertyChanged(); } }
        public DateTime? BirthDate { get => _currentUser.BirthDate; set { _currentUser.BirthDate = value; OnPropertyChanged(); } }

        public ICommand SelectPictureCommand { get; }
        public ICommand SaveProfileCommand { get; }

        public CompleteProfileOverlayViewModel(UserModel user, IUserService userService)
        {
            _currentUser = user;
            _userService = userService;

            Branches = ProfileCatalogs.GetBranches();
            Departments = ProfileCatalogs.GetDepartments();

            ProfilePictureBase64 = _currentUser.ProfilePictureBase64;

            SelectPictureCommand = new RelayCommand(_ => SelectPicture());
            SaveProfileCommand = new RelayCommand(async _ => await SaveProfileAsync());
        }

        private void SelectPicture()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Archivos de imagen (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                byte[] imageBytes = File.ReadAllBytes(openFileDialog.FileName);
                ProfilePictureBase64 = Convert.ToBase64String(imageBytes);
            }
        }

        private async System.Threading.Tasks.Task SaveProfileAsync()
        {
            if (string.IsNullOrWhiteSpace(Phone) ||
                string.IsNullOrWhiteSpace(EmergencyPhone) ||
                string.IsNullOrWhiteSpace(EmergencyContactName) ||
                string.IsNullOrWhiteSpace(Branch) ||
                string.IsNullOrWhiteSpace(Department) ||
                string.IsNullOrWhiteSpace(City) ||
                BirthDate == null)
            {
                MessageBox.Show("Por favor, llena todos los campos obligatorios.", "Perfil Incompleto", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _currentUser.ProfilePictureBase64 = ProfilePictureBase64;
            _currentUser.IsProfileComplete = true;

            bool success = await _userService.UpdateUserProfileAsync(_currentUser);

            if (success)
            {
                MessageBox.Show("¡Perfil completado exitosamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                OnProfileCompleted?.Invoke();
            }
            else
            {
                MessageBox.Show("Hubo un error al guardar los datos. Intenta nuevamente.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}