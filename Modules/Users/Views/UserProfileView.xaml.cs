using AMANC_Inventory.Users.Services;
using AMANC_Inventory.Users.Models;
using AMANC_Inventory.Users.ViewModels;
using AMANC_Inventory.Shared.Services;
using System.Windows.Controls;

namespace AMANC_Inventory.Views
{
    public partial class UserProfileView : UserControl
    {
        public UserProfileView()
        {
            InitializeComponent();
        }

        public UserProfileView(UserModel usuarioActual) : this()
        {
            DataContext = new UserProfileViewModel(usuarioActual, new UserService(), new EmailService());
        }
    }
}