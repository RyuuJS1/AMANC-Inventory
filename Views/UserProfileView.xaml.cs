using AMANC_Inventory.Models;
using AMANC_Inventory.Services;
using AMANC_Inventory.ViewModels;
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
            DataContext = new UserProfileViewModel(usuarioActual, new UserService());
        }
    }
}