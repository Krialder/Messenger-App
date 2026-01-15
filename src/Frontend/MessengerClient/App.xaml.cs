using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MessengerClient.Services;
using MessengerClient.ViewModels;
using MessengerClient.Views;
using MessengerClient.Data;
using Refit;
using System.Windows;

namespace MessengerClient
{
    public partial class App : Application
    {
        public IServiceProvider? ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            LocalDbContext dbContext = ServiceProvider.GetRequiredService<LocalDbContext>();
            dbContext.Database.EnsureCreated();

            Window loginWindow = new Window
            {
                Content = new LoginView
                {
                    DataContext = ServiceProvider.GetRequiredService<LoginViewModel>()
                },
                Title = "Secure Messenger - Login",
                Width = 500,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };
            loginWindow.Show();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            string gatewayUrl = "https://localhost:7001";

            // Fixed: Correct Refit service registration for .NET 8
            services.AddHttpClient();
            
            services.AddRefitClient<IAuthApiService>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(gatewayUrl));

            services.AddRefitClient<IMessageApiService>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(gatewayUrl));

            services.AddRefitClient<IUserApiService>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(gatewayUrl));

            services.AddRefitClient<IFileApiService>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(gatewayUrl));

            services.AddRefitClient<ICryptoApiService>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri(gatewayUrl));

            services.AddSingleton(provider => new SignalRService(gatewayUrl));
            services.AddSingleton<LocalCryptoService>();
            services.AddSingleton<LocalStorageService>();

            services.AddDbContext<LocalDbContext>(options =>
                options.UseSqlite("Data Source=messenger.db"));

            services.AddTransient<LoginViewModel>();
            services.AddTransient<RegisterViewModel>();
            services.AddTransient<ChatViewModel>();
            services.AddTransient<ContactsViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<MainViewModel>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SignalRService? signalR = ServiceProvider?.GetService<SignalRService>();
            if (signalR != null && signalR.IsConnected)
            {
                signalR.DisconnectAsync().GetAwaiter().GetResult();
            }

            LocalCryptoService? crypto = ServiceProvider?.GetService<LocalCryptoService>();
            crypto?.ClearMasterKey();

            base.OnExit(e);
        }
    }
}
