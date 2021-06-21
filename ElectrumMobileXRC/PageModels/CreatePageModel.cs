using ElectrumMobileXRC.Resources;
using ElectrumMobileXRC.Services;
using FreshMvvm;
using NBitcoin;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using Xamarin.Forms;

namespace ElectrumMobileXRC.PageModels
{
    public class CreatePageModel : FreshBasePageModel
    {
        public ICommand GenerateButtonCommand { get; set; }
        public ICommand CreateButtonCommand { get; set; }
        
        [Required]
        public string Seed { get; set; }

        [Required]
        public string Passphrase { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        private ConfigDbService _configDb;

        public CreatePageModel()
        {
            Seed = string.Empty;

            _configDb = new ConfigDbService();

            GenerateButtonCommand = new Command(async () =>
            {
                Mnemonic mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve);
                Seed = mnemonic.ToString();
            });

            CreateButtonCommand = new Command(async (object data) =>
            {
                HideErrorLabels();
                if (IsFormValid())
                {
                    await CoreMethods.PushPageModel<MainPageModel>();
                } 
                else
                {
                    await CoreMethods.DisplayAlert("Please to fill all fields.", "", "Ok");
                }
            });
        }

        private void HideErrorLabels()
        {
            var objUserNameError = CurrentPage.FindByName<Label>("UserNameError");
            objUserNameError.IsVisible = false;
            var objPasswordError = CurrentPage.FindByName<Label>("PasswordError");
            objPasswordError.IsVisible = false;
            var objSeedError = CurrentPage.FindByName<Label>("SeedError");
            objSeedError.IsVisible = false;
            var objPassphraseError = CurrentPage.FindByName<Label>("PassphraseError");
            objPassphraseError.IsVisible = false;
        }

        private bool IsFormValid()
        {
            var isValid = true;

            if (string.IsNullOrEmpty(UserName))
            {
                var objUserNameError = CurrentPage.FindByName<Label>("UserNameError");
                objUserNameError.Text = string.Format(SharedResource.Error_FieldRequired, "UserName");
                objUserNameError.IsVisible = true;
                isValid = false;
            }

            if (string.IsNullOrEmpty(Password))
            {
                var objUserNameError = CurrentPage.FindByName<Label>("PasswordError");
                objUserNameError.Text = string.Format(SharedResource.Error_FieldRequired, "Password");
                objUserNameError.IsVisible = true;
                isValid = false;
            }

            if (string.IsNullOrEmpty(Seed))
            {
                var objUserNameError = CurrentPage.FindByName<Label>("SeedError");
                objUserNameError.Text = string.Format(SharedResource.Error_FieldRequired, "Seed");
                objUserNameError.IsVisible = true;
                isValid = false;
            }

            if (string.IsNullOrEmpty(Passphrase))
            {
                var objUserNameError = CurrentPage.FindByName<Label>("PassphraseError");
                objUserNameError.Text = string.Format(SharedResource.Error_FieldRequired, "Passphrase");
                objUserNameError.IsVisible = true;
                isValid = false;
            }

            return isValid;
        }
    }
}
