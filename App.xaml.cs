using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;

// Módulos Compartidos
using AMANC_Inventory.Shared.Interfaces;
using AMANC_Inventory.Shared.Services;

// Módulo Autenticación
using AMANC_Inventory.Modules.Auth.Interfaces;
using AMANC_Inventory.Modules.Auth.Services;
using AMANC_Inventory.Modules.Auth.ViewModels;

// Módulo Inventario / Productos
using AMANC_Inventory.Inventory.Interfaces;
using AMANC_Inventory.Inventory.Services;
using AMANC_Inventory.Inventory.ViewModels;

// Módulo Despensas
using AMANC_Inventory.Pantries.Interfaces;
using AMANC_Inventory.Pantries.Services;
using AMANC_Inventory.Pantries.ViewModels;

// Módulo Donaciones
using AMANC_Inventory.Donations.Interfaces;
using AMANC_Inventory.Donations.Services;
using AMANC_Inventory.Donations.ViewModels;

// Módulo Pacientes / Beneficiarios
using AMANC_Inventory.Patients.Interfaces;
using AMANC_Inventory.Patients.Services;
using AMANC_Inventory.Patients.ViewModels;

// Módulo Traslados / Viajes
using AMANC_Inventory.Trips.Interfaces;
using AMANC_Inventory.Trips.Services;
using AMANC_Inventory.Trips.ViewModels;

// Módulo Usuarios
using AMANC_Inventory.Users.Interfaces;
using AMANC_Inventory.Users.Services;
using AMANC_Inventory.Users.ViewModels;

namespace AMANC_Inventory
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Configurar captura global de errores para estabilidad
            RegisterGlobalExceptionHandling();

            // 2. Configurar el contenedor de dependencias
            var services = new ServiceCollection();
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();

            // 3. Iniciar la ventana principal
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // -----------------------------------------------------------------
            // 1. Servicios (Singletons para estado global / Transients para tareas cortas)
            // -----------------------------------------------------------------
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IProductService, ProductService>();
            services.AddSingleton<IPantryService, PantryService>();
            services.AddSingleton<IDonationService, DonationService>();
            services.AddSingleton<IPatientService, PatientService>();
            services.AddSingleton<ITripService, TripService>();

            services.AddTransient<IEmailService, EmailService>();

            // -----------------------------------------------------------------
            // 2. ViewModels Principales y Modulares
            // -----------------------------------------------------------------
            services.AddTransient<InventoryViewModel>();

            // Sub-ViewModels de Consulta
            services.AddTransient<ProductConsultViewModel>();
            services.AddTransient<PantryConsultViewModel>();
            services.AddTransient<DonationConsultViewModel>();
            services.AddTransient<PatientConsultViewModel>();
            services.AddTransient<TripConsultViewModel>();
            services.AddTransient<UserConsultViewModel>();

            // Sub-ViewModels de Registro
            services.AddTransient<ProductRegisterViewModel>();
            services.AddTransient<PantryRegisterViewModel>();
            services.AddTransient<DonationRegisterViewModel>();
            services.AddTransient<PatientRegisterViewModel>();
            services.AddTransient<TripRegisterViewModel>();

            // ViewModels de Autenticación y Usuario
            services.AddTransient<LoginViewModel>();
            services.AddTransient<UserProfileViewModel>();

            // -----------------------------------------------------------------
            // 3. Vistas Principales
            // -----------------------------------------------------------------
            services.AddTransient<MainWindow>();
        }

        private void RegisterGlobalExceptionHandling()
        {
            // Captura errores no controlados en el hilo de la interfaz de usuario (UI)
            DispatcherUnhandledException += (sender, args) =>
            {
                System.Diagnostics.Debug.WriteLine($"[Error no controlado en UI]: {args.Exception.Message}");

                MessageBox.Show(
                    $"Ocurrió un error no esperado en la aplicación:\n\n{args.Exception.Message}",
                    "Error de Sistema",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                args.Handled = true; // Evita el colapso (crash) repentino de la app
            };

            // Captura errores no observados en Tareas Asíncronas (Task/async)
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                System.Diagnostics.Debug.WriteLine($"[Error en Task asíncrona]: {args.Exception.Message}");
                args.SetObserved();
            };
        }
    }
}