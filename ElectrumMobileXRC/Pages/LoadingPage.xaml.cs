using System;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Collections.Generic;
using ElectrumMobileXRC.Controls;

namespace ElectrumMobileXRC.Pages
{
    public partial class LoadingPage : GradientContentPage
    {

        private bool _initialized = false;

        public LoadingPage()
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
