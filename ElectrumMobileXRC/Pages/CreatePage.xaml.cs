using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;
using FFImageLoading.Forms;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using ElectrumMobileXRC.Controls;

namespace ElectrumMobileXRC.Pages
{
    public partial class CreatePage : GradientContentPage
    {
        private readonly ScreenMetrics _metrics;
        private readonly int _formsWidth;
        private readonly int _formsHeight;

        private bool _initialized = false;
        private bool _starsAdded = false;
        private List<VisualElement> _stars = new List<VisualElement>();

        public CreatePage()
        {
            InitializeComponent();

            _metrics = DeviceDisplay.ScreenMetrics;
            _formsWidth = Convert.ToInt32(_metrics.Width / _metrics.Density);
            _formsHeight = Convert.ToInt32(_metrics.Height / _metrics.Density);
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
