using AMANC_Inventory.Users.Services;
using AMANC_Inventory.Users.Models;
using AMANC_Inventory.Users.ViewModels;
using System.Windows.Controls;

namespace AMANC_Inventory.Views.Overlays
{
    public partial class CompleteProfileOverlay : UserControl
    {
        public CompleteProfileOverlay()
        {
            InitializeComponent();
        }

        public CompleteProfileOverlay(UserModel usuarioActual) : this()
        {
            DataContext = new CompleteProfileOverlayViewModel(usuarioActual, new UserService());
        }
    }
}