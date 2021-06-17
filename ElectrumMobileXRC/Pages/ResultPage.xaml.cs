using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ElectrumMobileXRC.Controls;
using FFImageLoading.Forms;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ElectrumMobileXRC.Pages
{
    public partial class ResultPage : GradientContentPage
    {
        private bool _initialized = false;
        private bool _starsAdded = false;
        private List<VisualElement> _stars = new List<VisualElement>();

        public ResultPage()
        {
            InitializeComponent();

            SubTitleLabel.TranslateTo(1000, 0, 0, null);
            TitleLabel.TranslateTo(1000, 0, 0, null);
            MoonBoy.TranslateTo(0, 1000, 0, null);
            Card.TranslateTo(1000, 0, 0, null);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!_initialized)
            {
                await Task.WhenAll(
                    SubTitleLabel.TranslateTo(0, 0, 400, Easing.CubicInOut),
                    TitleLabel.TranslateTo(0, 0, 450, Easing.CubicInOut),
                    Card.TranslateTo(0, 0, 500, Easing.CubicInOut),
                    MoonBoy.TranslateTo(0, 0, 550, Easing.CubicInOut)
                );

                _initialized = true;
            }
        }
    }
}
