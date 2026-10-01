using System.Windows.Controls;
using AMANC_Inventory.Inventory.ViewModels;

namespace AMANC_Inventory.Views
{
    public partial class InventoryConsultView : UserControl
    {
        public InventoryConsultView()
        {
            InitializeComponent();

            if (DataContext == null)
            {
                DataContext = new InventoryConsultViewModel();
            }
        }
    }
}