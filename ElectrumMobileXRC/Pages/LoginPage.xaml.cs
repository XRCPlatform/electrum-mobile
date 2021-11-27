using ElectrumMobileXRC.Controls;

namespace ElectrumMobileXRC.Pages
{
    public partial class LoginPage : GradientContentPage
    {
        private bool _initialized = false;

        public LoginPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!_initialized)
            {
                _initialized = true;
            }
        }
    }
}
