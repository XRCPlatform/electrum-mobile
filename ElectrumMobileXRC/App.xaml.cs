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

        public App()
        {
            InitializeComponent();

            MainPage = new FreshMvvm.FreshNavigationContainer(FreshMvvm.FreshPageModelResolver.ResolvePageModel<MainPageModel>())
            {
                BarBackgroundColor = Color.FromHex("#301536"),
                BarTextColor = Color.White
            };

            _configDb = new ConfigDbService();
        }

        protected override async void OnStart()
        {
            var createPage = FreshPageModelResolver.ResolvePageModel<CreatePageModel>(null);

            var walletInit = await _configDb.Get(DbConfiguration.CFG_WALLETINIT);
            if ((walletInit == null) || (string.IsNullOrEmpty(walletInit.Value)))
            {
                App.Current.MainPage = createPage;
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
