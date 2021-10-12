using ElectrumMobileXRC.Resources;
using ElectrumMobileXRC.Services;
using FreshMvvm;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using WalletProvider;
using Xamarin.Forms;

namespace ElectrumMobileXRC.PageModels
{
    public class LoginPageModel : BasePageModel
    {
        public ICommand LoginButtonCommand { get; set; }
        public ICommand ForgotPasswordButtonCommand { get; set; }

        [Required]
        public string UserName { get; set; }
        
        [Required]
        public string Password { get; set; }

        private ConfigDbService _configDb;

        public LoginPageModel()
        {
            _configDb = new ConfigDbService();

            LoginButtonCommand = new Command(async () =>
            {
                HideErrorLabels();
                if (IsFormValid())
                {
                    ValidateUser(UserName, Password);
                }
                else
                {
                    await CoreMethods.DisplayAlert("Please to fill all fields.", "", "Ok");
                }
            });

            ForgotPasswordButtonCommand = new Command(async () =>
            {
                await CoreMethods.DisplayAlert("Your XRC are stored on blockchain. You can recreate your wallet if you have your seed, password and passphase after reinstallation of this application.", "", "Ok");
            });
        }

        private void HideErrorLabels()
        {
            var objUserNameError = CurrentPage.FindByName<Label>("UserNameError");
            objUserNameError.IsVisible = false;
            var objPasswordError = CurrentPage.FindByName<Label>("PasswordError");
            objPasswordError.IsVisible = false;
            var objLoginError = CurrentPage.FindByName<Label>("LoginError");
            objLoginError.IsVisible = false;
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

            return isValid;
        }

        private async void ValidateUser(string userName, string password)
        {
            var walletManager = new WalletManager();

            var serializedWallet = await _configDb.Get(DbConfiguration.CFG_WALLETMETADATA);
            if ((serializedWallet != null) && (!string.IsNullOrEmpty(serializedWallet.Value)))
            {
                var deserializedWallet = walletManager.DeserializeWalletMetadata(serializedWallet.Value);

                if (walletManager.IsPasswordUserValid(deserializedWallet, userName, password))
                {
                    SetValidUser(UserName);
                    await CoreMethods.PushPageModel<MainPageModel>();
                } 
                else
                {
                    var objLoginError = CurrentPage.FindByName<Label>("LoginError");
                    objLoginError.Text = SharedResource.Error_WrongLogin;
                    objLoginError.IsVisible = true;
                }
            }
            else
            {
                await CoreMethods.PushPageModel<CreatePageModel>();
            }
        }
    }
}

