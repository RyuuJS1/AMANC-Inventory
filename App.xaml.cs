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

// Módulo Inventario
using AMANC_Inventory.Inventory.Interfaces;
using AMANC_Inventory.Inventory.Services;
using AMANC_Inventory.Inventory.ViewModels;

// Módulo Donaciones
using AMANC_Inventory.Donations.Interfaces;
using AMANC_Inventory.Donations.Services;

// Módulo Pacientes
using AMANC_Inventory.Patients.Services;

// Módulo Traslados / Viajes
using AMANC_Inventory.Trips.Services;

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
            services.AddSingleton<IDonationService, DonationService>();
            services.AddSingleton<IPatientService, PatientService>();
            services.AddSingleton<ITripService, TripService>();

            services.AddTransient<IEmailService, EmailService>();

            // -----------------------------------------------------------------
            // 2. ViewModels (Transient para instanciar nuevos al navegar)
            // -----------------------------------------------------------------
            services.AddTransient<InventoryViewModel>();
            services.AddTransient<InventoryConsultViewModel>();
            services.AddTransient<InventoryRegisterViewModel>();
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