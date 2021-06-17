using FreshMvvm;
using System.Windows.Input;
using Xamarin.Forms;

namespace ElectrumMobileXRC.PageModels
{
    public class CreatePageModel : FreshBasePageModel
    {
        public ICommand GenerateButtonCommand { get; set; }
        public ICommand LoadButtonCommand { get; set; }

        public string Seed { get; set; }

        public string Passphrase { get; set; }

        public CreatePageModel()
        {
            Seed = string.Empty;

            GenerateButtonCommand = new Command(async () =>
            {
                Seed = "shop swap budget toilet riot swift loud mom grow venue census resemble squeeze champion ankle retire virus tooth huge oxygen section shove lab annual";
            });

            LoadButtonCommand = new Command(async () =>
            {
                await CoreMethods.PushPageModel<MainPageModel>();
            });
        }
    }
}
