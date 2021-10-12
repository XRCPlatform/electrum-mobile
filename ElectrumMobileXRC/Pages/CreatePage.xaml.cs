using ElectrumMobileXRC.Controls;

namespace ElectrumMobileXRC.Pages
{
    public partial class CreatePage : GradientContentPage
    {
        private bool _initialized = false;

        public CreatePage()
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
