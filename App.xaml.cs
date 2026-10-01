using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using AMANC_Inventory.Shared.Interfaces;
using AMANC_Inventory.Shared.Services;
using AMANC_Inventory.Modules.Auth.Interfaces;
using AMANC_Inventory.Modules.Auth.Services;
using AMANC_Inventory.Modules.Auth.ViewModels;
using AMANC_Inventory.Inventory.Interfaces;
using AMANC_Inventory.Inventory.Services;
using AMANC_Inventory.Inventory.ViewModels;
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

            var services = new ServiceCollection();
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();

            // Inicia la ventana principal resolviendo todas sus dependencias automáticamente
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // 1. Inyección de Servicios (Singletons para mantener estado / Transients para ligeros)
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IProductService, ProductService>();
            services.AddTransient<IEmailService, EmailService>();

            // 2. Inyección de ViewModels
            services.AddTransient<InventoryViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<UserProfileViewModel>();

            // 3. Vistas
            services.AddTransient<MainWindow>();
        }
    }
}