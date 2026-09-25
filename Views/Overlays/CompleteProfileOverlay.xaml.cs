using AMANC_Inventory.Models;
using AMANC_Inventory.Services;
using AMANC_Inventory.ViewModels;
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