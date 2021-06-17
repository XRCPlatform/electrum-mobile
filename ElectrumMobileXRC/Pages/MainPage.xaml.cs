﻿using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;
using FFImageLoading.Forms;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using ElectrumMobileXRC.Services;
using ElectrumMobileXRC.Models;
using Newtonsoft.Json;
using ElectrumMobileXRC.Controls;

namespace ElectrumMobileXRC.Pages
{
    public partial class MainPage : GradientContentPage
    {
        private readonly ScreenMetrics _metrics;
        private readonly int _formsWidth;
        private readonly int _formsHeight;

        private bool _initialized = false;
        private bool _starsAdded = false;
        private List<VisualElement> _stars = new List<VisualElement>();
  
        public MainPage()
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

                await Task.WhenAll(
                    WalletNameLabel.TranslateTo(_formsWidth, 0, 0, null),
                    ElectrumMobileXRCLabel.TranslateTo(_formsWidth, 0, 0, null),
                    CalculateButton.TranslateTo(0, _formsHeight, 0, null),
                     Card.TranslateTo(_formsWidth, 0, 0, null)
                );

                await Task.WhenAll(
                    WalletNameLabel.TranslateTo(0, 0, 400, Easing.CubicInOut),
                    ElectrumMobileXRCLabel.TranslateTo(0, 0, 450, Easing.CubicInOut),
                    Card.TranslateTo(0, 0, 500, Easing.CubicInOut),
                    CalculateButton.TranslateTo(0, 0, 550, Easing.CubicInOut)
                );

                _initialized = true;


                var stest = new TxDbService();
                var sclass = new TxModel();
                sclass.Hash = "xxxx";
                var id = await stest.AddCoffee(sclass);

                Console.WriteLine(id);
                var ssss = await stest.GetAll();

                var exp = JsonConvert.SerializeObject(ssss);

                Console.WriteLine(exp);
            }
        }
    }
}
