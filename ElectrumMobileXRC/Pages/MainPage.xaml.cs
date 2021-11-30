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
        private bool _initialized = false;

        public MainPage()
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
