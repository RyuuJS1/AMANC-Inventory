using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Core.Utilities;
using AMANC_Inventory.Modules.Auth.Interfaces;
using AMANC_Inventory.Modules.Auth.Services;
using AMANC_Inventory.Shared.Interfaces;
using AMANC_Inventory.Users.Interfaces;
using AMANC_Inventory.Users.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AMANC_Inventory.Users.ViewModels
{
    public partial class UserProfileViewModel : ViewModelBase
    {
        private readonly UserModel _currentUser;
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        // Respaldo para cancelar edición
        private UserModel? _userBackup;
        private string _entryTimeBackup = "00:00";
        private string _exitTimeBackup = "00:00";

        // Delegados de navegación
        public Action? OnReturnToInventoryRequested { get; set; }
        public Action? OnLogoutRequested { get; set; }

        // Colecciones para Catálogos
        public ObservableCollection<string> Branches { get; }
        public ObservableCollection<string> Departments { get; }
        public ObservableCollection<string> Relationships { get; }
        public ObservableCollection<string> Hours { get; }
        public ObservableCollection<string> ShirtSizes { get; }
        public ObservableCollection<string> AreasOfInterest { get; }

        // Estado Visual de la Interfaz
        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        private bool _isProfileDetailsOpen = false;
        public bool IsProfileDetailsOpen
        {
            get => _isProfileDetailsOpen;
            set => SetProperty(ref _isProfileDetailsOpen, value);
        }

        // Declaración de Comandos
        public ICommand CloseProfileCommand { get; private set; } = null!;
        public ICommand ChangeProfilePictureCommand { get; private set; } = null!;
        public ICommand RemoveProfilePictureCommand { get; private set; } = null!;
        public ICommand EditProfileCommand { get; private set; } = null!;
        public ICommand SaveChangesCommand { get; private set; } = null!;
        public ICommand CancelEditCommand { get; private set; } = null!;
        public ICommand ChangePasswordCommand { get; private set; } = null!;
        public ICommand LogoutCommand { get; private set; } = null!;

        // Constructor Principal
        public UserProfileViewModel(UserModel user, IUserService userService, IAuthService authService)
        {
            _currentUser = user ?? new UserModel();
            _userService = userService;
            _authService = authService;

            // Cargar Catálogos
            Branches = ProfileCatalogs.GetBranches();
            Departments = ProfileCatalogs.GetDepartments();
            Relationships = ProfileCatalogs.GetRelationships();
            Hours = ProfileCatalogs.GetHours();
            ShirtSizes = ProfileCatalogs.GetSize();
            AreasOfInterest = ProfileCatalogs.GetAreasOfInterest();

            ProfilePictureBase64 = _currentUser.ProfilePictureBase64;
            ParseInitialAvailability();

            InicializarComandos();
        }

        // Sobrecargas de compatibilidad
        public UserProfileViewModel(UserModel user, IUserService userService, IEmailService emailService)
            : this(user, userService, new AuthService())
        {
        }

        public UserProfileViewModel(UserModel user, IUserService userService)
            : this(user, userService, new AuthService())
        {
        }
    }
}