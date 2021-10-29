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
        private DbWalletHelper _walletDbHelper;

        public LoginPageModel()
        {
            _configDb = new ConfigDbService();
            _walletDbHelper = new DbWalletHelper(_configDb);

            LoginButtonCommand = new Command(async () =>
            {
                HideErrorLabels();
                if (IsFormValid())
                {
                    ValidateUserAsync(UserName, Password);
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

        private async void ValidateUserAsync(string userName, string password)
        {
            await _walletDbHelper.LoadFromDbAsync();
            if (_walletDbHelper.IsWalletInit) {

                var walletManager = new WalletManager(_walletDbHelper.SerializedWallet);
                if (walletManager.IsPasswordUserValid(walletManager.Wallet, userName, password))
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

