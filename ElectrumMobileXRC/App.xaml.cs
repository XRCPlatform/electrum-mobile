using System;
using System.IO;
using ElectrumMobileXRC.PageModels;
using ElectrumMobileXRC.Services;
using FreshMvvm;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace ElectrumMobileXRC
{
    public partial class App : Application
    {
        private ConfigDbService _configDb;
        private DbWalletHelper _walletDbHelper;

        public App()
        {
            InitializeComponent();

            Current.MainPage = new FreshNavigationContainer(FreshPageModelResolver.ResolvePageModel<LoadingPageModel>());
            _configDb = new ConfigDbService();
            _walletDbHelper = new DbWalletHelper(_configDb);
        }

        protected override async void OnStart()
        {
            await _walletDbHelper.LoadFromDbAsync();
            if (!_walletDbHelper.IsWalletInit)
            {
                Current.MainPage = new FreshNavigationContainer(FreshPageModelResolver.ResolvePageModel<CreatePageModel>());
            }
            else
            {
                Current.MainPage = new FreshNavigationContainer(FreshPageModelResolver.ResolvePageModel<LoginPageModel>());
            }
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
