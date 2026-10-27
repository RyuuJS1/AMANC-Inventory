using System;

namespace AMANC_Inventory.Users.ViewModels
{
    public partial class UserProfileViewModel
    {
        public string NombreUsuario
        {
            get => _currentUser.Name ?? string.Empty;
            set { _currentUser.Name = value; OnPropertyChanged(); OnPropertyChanged(nameof(UserInitials)); }
        }

        public string CorreoUsuario => _currentUser.Email ?? string.Empty;

        public string TelefonoUsuario
        {
            get => _currentUser.Phone ?? string.Empty;
            set
            {
                string formatted = FormatPhoneNumber(value);
                if (_currentUser.Phone != formatted)
                {
                    _currentUser.Phone = formatted;
                    OnPropertyChanged();
                }
            }
        }

        public string EmergencyRelationship
        {
            get => EmergencyContactRelationship;
            set => EmergencyContactRelationship = value;
        }

        public string EmergencyPhone
        {
            get => EmergencyContactPhone;
            set => EmergencyContactPhone = value;
        }

        public string RolUsuario => _currentUser.Role ?? "Voluntario";

        public string UserInitials => _currentUser.UserInitials ?? "U";

        public string Branch
        {
            get => _currentUser.Branch ?? string.Empty;
            set { _currentUser.Branch = value; OnPropertyChanged(); }
        }

        public string Department
        {
            get => _currentUser.Department ?? string.Empty;
            set { _currentUser.Department = value; OnPropertyChanged(); }
        }

        public string City
        {
            get => _currentUser.City ?? string.Empty;
            set { _currentUser.City = value; OnPropertyChanged(); }
        }

        public DateTime? BirthDate
        {
            get => _currentUser.BirthDate;
            set { _currentUser.BirthDate = value; OnPropertyChanged(); }
        }

        public string EmergencyContactName
        {
            get => _currentUser.EmergencyContactName ?? string.Empty;
            set { _currentUser.EmergencyContactName = value; OnPropertyChanged(); }
        }

        public string EmergencyContactRelationship
        {
            get => _currentUser.EmergencyContactRelationship ?? string.Empty;
            set { _currentUser.EmergencyContactRelationship = value; OnPropertyChanged(); }
        }

        public string EmergencyContactPhone
        {
            get => _currentUser.EmergencyPhone ?? string.Empty;
            set
            {
                string formatted = FormatPhoneNumber(value);
                if (_currentUser.EmergencyPhone != formatted)
                {
                    _currentUser.EmergencyPhone = formatted;
                    OnPropertyChanged();
                }
            }
        }

        private string _entryTime = "00:00";
        public string EntryTime
        {
            get => _entryTime;
            set
            {
                if (SetProperty(ref _entryTime, value))
                {
                    UpdateAvailability();
                }
            }
        }

        private string _exitTime = "00:00";
        public string ExitTime
        {
            get => _exitTime;
            set
            {
                if (SetProperty(ref _exitTime, value))
                {
                    UpdateAvailability();
                }
            }
        }

        public string Availability
        {
            get => _currentUser.Availability ?? string.Empty;
            set { _currentUser.Availability = value; OnPropertyChanged(); }
        }

        public string Skills
        {
            get => _currentUser.Skills ?? string.Empty;
            set { _currentUser.Skills = value; OnPropertyChanged(); }
        }

        public string ShirtSize
        {
            get => _currentUser.ShirtSize ?? string.Empty;
            set { _currentUser.ShirtSize = value; OnPropertyChanged(); }
        }
    }
}