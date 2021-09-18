using ElectrumMobileXRC.Controls;

namespace ElectrumMobileXRC.Pages
{
    public partial class AddressesPage : GradientContentPage
    {

        private bool _initialized = false;

        public AddressesPage()
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
