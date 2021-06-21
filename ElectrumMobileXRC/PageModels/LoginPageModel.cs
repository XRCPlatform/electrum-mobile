using ElectrumMobileXRC.Resources;
using FreshMvvm;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using Xamarin.Forms;

namespace ElectrumMobileXRC.PageModels
{
    public class LoginPageModel : FreshBasePageModel
    {
        public ICommand LoginButtonCommand { get; set; }

        [Required]
        public string UserName { get; set; }
        
        [Required]
        public string Password { get; set; }

        public LoginPageModel()
        {
            LoginButtonCommand = new Command(async () =>
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
    }
}

