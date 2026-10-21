using AMANC_Inventory.Core.Architecture;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AMANC_Inventory.Inventory.ViewModels
{
    public partial class InventoryViewModel
    {
        private int _selectedTab = 0; // 0 = Consulta, 1 = Registro
        private int _selectedSubTab = 0; // 0=Productos, 1=Despensas, 2=Donaciones, 3=Pacientes, 4=Traslados, 5=Usuarios
        private string _filterText = string.Empty;

        public ICommand SelectMainTabCommand { get; private set; } = null!;
        public ICommand SelectTabCommand { get; private set; } = null!;
        public ICommand SelectSubTabCommand { get; private set; } = null!;
        public ICommand ClearFilterCommand { get; private set; } = null!;

        private void InicializarComandosTabsYFiltros()
        {
            SelectMainTabCommand = new RelayCommand(p => SeleccionarPestanaPrincipal(p));
            SelectTabCommand = SelectMainTabCommand;
            SelectSubTabCommand = new RelayCommand(p => SeleccionarSubPestana(p));
            ClearFilterCommand = new RelayCommand(_ => FilterText = string.Empty);
        }

        #region Propiedades de Navegación de Pestañas

        public int SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (SetProperty(ref _selectedTab, value))
                {
                    OnPropertyChanged(nameof(IsTabConsultVisible));
                    OnPropertyChanged(nameof(IsTabRegisterVisible));
                }
            }
        }

        public int SelectedSubTab
        {
            get => _selectedSubTab;
            set
            {
                if (SetProperty(ref _selectedSubTab, value))
                {
                    NotificarCambioSubPestanas();
                    _ = CargarPestanaActivaAsync();
                }
            }
        }

        public bool IsTabConsultVisible => SelectedTab == 0;
        public bool IsTabRegisterVisible => SelectedTab == 1;

        public bool IsInventoryTabSelected
        {
            get => SelectedSubTab == 0;
            set { if (value) SelectedSubTab = 0; }
        }

        public bool IsPantriesTabSelected
        {
            get => SelectedSubTab == 1;
            set { if (value) SelectedSubTab = 1; }
        }

        public bool IsDonationsTabSelected
        {
            get => SelectedSubTab == 2;
            set { if (value) SelectedSubTab = 2; }
        }

        public bool IsPatientsTabSelected
        {
            get => SelectedSubTab == 3;
            set { if (value) SelectedSubTab = 3; }
        }

        public bool IsTripsTabSelected
        {
            get => SelectedSubTab == 4;
            set { if (value) SelectedSubTab = 4; }
        }

        public bool IsUsersTabSelected
        {
            get => SelectedSubTab == 5;
            set { if (value) SelectedSubTab = 5; }
        }

        private void NotificarCambioSubPestanas()
        {
            OnPropertyChanged(nameof(IsInventoryTabSelected));
            OnPropertyChanged(nameof(IsPantriesTabSelected));
            OnPropertyChanged(nameof(IsDonationsTabSelected));
            OnPropertyChanged(nameof(IsPatientsTabSelected));
            OnPropertyChanged(nameof(IsTripsTabSelected));
            OnPropertyChanged(nameof(IsUsersTabSelected));
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (SetProperty(ref _filterText, value))
                {
                    AplicarFiltros(value);
                }
            }
        }

        public void SeleccionarPestanaPrincipal(object? parameter)
        {
            if (parameter != null && int.TryParse(parameter.ToString(), out int tabIndex))
            {
                SelectedTab = tabIndex;
            }
        }

        public void SeleccionarSubPestana(object? parameter)
        {
            if (parameter != null && int.TryParse(parameter.ToString(), out int subIndex))
            {
                SelectedSubTab = subIndex;
            }
        }

        private async Task CargarPestanaActivaAsync()
        {
            switch (SelectedSubTab)
            {
                case 0: await ProductConsult.EnsureDataLoadedAsync(); break;
                case 1: await PantryConsult.EnsureDataLoadedAsync(); break;
                case 2: await DonationConsult.EnsureDataLoadedAsync(); break;
                case 3: await PatientConsult.EnsureDataLoadedAsync(); break;
                case 4: await TripConsult.EnsureDataLoadedAsync(); break;
                case 5: await UserConsult.EnsureDataLoadedAsync(); break;
            }
        }

        private void AplicarFiltros(string text)
        {
            ProductConsult.ApplyFilter(text);
            PantryConsult.ApplyFilter(text);
            DonationConsult.ApplyFilter(text);
            PatientConsult.ApplyFilter(text);
            TripConsult.ApplyFilter(text);
            UserConsult.ApplyFilter(text);
        }

        #endregion

        #region Permisos por Rol
        public bool CanAccessDonations => CurrentUser?.CanAccessDonations ?? true;
        public bool CanAccessPatients => CurrentUser?.CanAccessPatients ?? true;
        public bool CanAccessTrips => CurrentUser?.CanAccessTrips ?? true;
        public bool CanAccessUsers => CurrentUser?.CanAccessUsers ?? true;
        #endregion
    }
}