using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Collections.Generic;
using ElectrumMobileXRC.Services;
using ElectrumMobileXRC.Models;
using Newtonsoft.Json;
using ElectrumMobileXRC.Controls;

namespace ElectrumMobileXRC.Pages
{
    public partial class MainPage : GradientContentPage
    {
        private readonly DisplayInfo _metrics;
        private readonly int _formsWidth;
        private readonly int _formsHeight;

        private bool _initialized = false;
  
        private ConfigDbService _configDb;

        public MainPage()
        {
            InitializeComponent();

            _metrics = DeviceDisplay.MainDisplayInfo;
            _formsWidth = Convert.ToInt32(_metrics.Width / _metrics.Density);
            _formsHeight = Convert.ToInt32(_metrics.Height / _metrics.Density);

            _configDb = new ConfigDbService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!_initialized)
            {
                await Task.WhenAll(
                    WalletNameLabel.TranslateTo(_formsWidth, 0, 0, null),
                    ElectrumMobileXRCLabel.TranslateTo(_formsWidth, 0, 0, null),
                    SendButton.TranslateTo(0, _formsHeight, 0, null),
                    ReceiveButton.TranslateTo(0, _formsHeight, 0, null),
                    MenuButton.TranslateTo(0, _formsHeight, 0, null),
                     Card.TranslateTo(_formsWidth, 0, 0, null)
                );

                await Task.WhenAll(
                    WalletNameLabel.TranslateTo(0, 0, 400, Easing.CubicInOut),
                    ElectrumMobileXRCLabel.TranslateTo(0, 0, 450, Easing.CubicInOut),
                    Card.TranslateTo(0, 0, 500, Easing.CubicInOut),
                    SendButton.TranslateTo(0, 0, 550, Easing.CubicInOut),
                    ReceiveButton.TranslateTo(0, 0, 550, Easing.CubicInOut),
                    MenuButton.TranslateTo(0, 0, 550, Easing.CubicInOut)
                );

                _initialized = true;
            }
        }
    }
}
