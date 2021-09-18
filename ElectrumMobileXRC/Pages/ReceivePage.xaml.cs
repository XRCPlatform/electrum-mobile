using ElectrumMobileXRC.Controls;
using QRCoder;
using System.IO;
using Xamarin.Forms;

namespace ElectrumMobileXRC.Pages
{
    public partial class ReceivePage : GradientContentPage
    {

        private bool _initialized = false;

        public ReceivePage()
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
