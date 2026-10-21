using AMANC_Inventory.Core.Architecture;
using AMANC_Inventory.Donations.Interfaces;
using AMANC_Inventory.Donations.Models;
using AMANC_Inventory.Donations.ViewModels;
using AMANC_Inventory.Inventory.Interfaces;
using AMANC_Inventory.Inventory.Models;
using AMANC_Inventory.Pantries.Interfaces;
using AMANC_Inventory.Pantries.Models;
using AMANC_Inventory.Pantries.ViewModels;
using AMANC_Inventory.Patients.Interfaces;
using AMANC_Inventory.Patients.Models;
using AMANC_Inventory.Patients.ViewModels;
using AMANC_Inventory.Trips.Interfaces;
using AMANC_Inventory.Trips.Models;
using AMANC_Inventory.Trips.ViewModels;
using AMANC_Inventory.Users.Interfaces;
using AMANC_Inventory.Users.Models;
using AMANC_Inventory.Users.Services;
using AMANC_Inventory.Users.ViewModels;
using System;
using System.Collections.ObjectModel;

namespace AMANC_Inventory.Inventory.ViewModels
{
    public partial class InventoryViewModel : ViewModelBase
    {
        private readonly IUserService _userService;

        // SUB-VIEWMODELS DE CONSULTA
        public ProductConsultViewModel ProductConsult { get; }
        public PantryConsultViewModel PantryConsult { get; }
        public DonationConsultViewModel DonationConsult { get; }
        public PatientConsultViewModel PatientConsult { get; }
        public TripConsultViewModel TripConsult { get; }
        public UserConsultViewModel UserConsult { get; }

        // SUB-VIEWMODELS DE REGISTRO
        public ProductRegisterViewModel ProductRegister { get; }
        public PantryRegisterViewModel PantryRegister { get; }
        public DonationRegisterViewModel DonationRegister { get; }
        public PatientRegisterViewModel PatientRegister { get; }
        public TripRegisterViewModel TripRegister { get; }

        // PROXIES DE LISTAS PARA BINDING CON LA VISTA INVENTORYCONSULTVIEW.XAML
        public ObservableCollection<ProductModel> FilteredInventoryList => ProductConsult.FilteredItems;
        public ObservableCollection<PantryModel> FilteredPantriesList => PantryConsult.FilteredPantriesList;
        public ObservableCollection<DonationModel> FilteredDonationsList => DonationConsult.FilteredDonationsList;
        public ObservableCollection<PatientModel> FilteredPatientsList => PatientConsult.FilteredPatientsList;
        public ObservableCollection<TripModel> FilteredTripsList => TripConsult.FilteredTripsList;
        public ObservableCollection<UserModel> FilteredUsersList => UserConsult.FilteredUsersList;

        public InventoryViewModel() : this(null, null, null, null, null, null)
        {
        }

        public InventoryViewModel(
            IProductService? productService = null,
            IPantryService? pantryService = null,
            IDonationService? donationService = null,
            IPatientService? patientService = null,
            ITripService? tripService = null,
            IUserService? userService = null)
        {
            _userService = userService ?? new UserService();

            // 1. Instanciar Sub-ViewModels de Consulta
            ProductConsult = new ProductConsultViewModel(productService);
            PantryConsult = new PantryConsultViewModel(pantryService);
            DonationConsult = new DonationConsultViewModel(donationService);
            PatientConsult = new PatientConsultViewModel(patientService);
            TripConsult = new TripConsultViewModel(tripService);
            UserConsult = new UserConsultViewModel(_userService);

            // 2. Instanciar Sub-ViewModels de Registro
            ProductRegister = new ProductRegisterViewModel(
                productService,
                async () => await ProductConsult.ForceReloadAsync());

            PantryRegister = new PantryRegisterViewModel(
                pantryService,
                patientService,
                async () => await PantryConsult.ForceReloadAsync());

            DonationRegister = new DonationRegisterViewModel(
            donationService,
            productService,
            () => DonationConsult.ForceReloadAsync());

            PatientRegister = new PatientRegisterViewModel(
                patientService,
                async () =>
                {
                    await PatientConsult.ForceReloadAsync();
                    await PantryRegister.LoadDestinationsAsync();
                });

            TripRegister = new TripRegisterViewModel(
                tripService,
                async () => await TripConsult.ForceReloadAsync());

            // Catálogos
            Relationships = new ObservableCollection<string> { "Padre/Madre", "Cónyuge", "Hermano/a", "Hijo/a", "Tutor", "Otro" };
            Branches = new ObservableCollection<string> { "Sede Central", "Veracruz", "Xalapa", "Córdoba" };
            Departments = new ObservableCollection<string> { "Administración", "Inventario", "Trabajo Social", "Logística", "Sistemas" };

            // Inicializar Comandos
            InicializarComandosTabsYFiltros();
            InicializarComandosPerfil();

            _ = ProductConsult.EnsureDataLoadedAsync();
        }

        public void InicializarUsuario(UserModel usuario)
        {
            CurrentUser = usuario;

            if (usuario != null)
            {
                NombreUsuario = !string.IsNullOrEmpty(usuario.Name) ? usuario.Name : "Usuario";
                UserInitials = !string.IsNullOrEmpty(usuario.UserInitials) ? usuario.UserInitials : "U";
                HasProfilePicture = !string.IsNullOrEmpty(usuario.ProfilePictureBase64);
                HasNoProfilePicture = !HasProfilePicture;

                ProductRegister.SetCurrentUser(usuario);
                PantryRegister.SetCurrentUser(usuario);
                DonationRegister.SetCurrentUser(usuario);
            }
        }
    }
}