using ElectrumMobileXRC.Controls;

namespace ElectrumMobileXRC.Pages
{
    public partial class NetworkPage : GradientContentPage
    {
        private bool _initialized = false;

        public NetworkPage()
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
