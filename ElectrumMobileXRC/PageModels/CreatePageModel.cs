using FreshMvvm;
using NBitcoin;
using System.Windows.Input;
using Xamarin.Forms;

namespace ElectrumMobileXRC.PageModels
{
    public class CreatePageModel : FreshBasePageModel
    {
        public ICommand GenerateButtonCommand { get; set; }
        public ICommand CreateButtonCommand { get; set; }

        public string Seed { get; set; }

        public string Passphrase { get; set; }

        public CreatePageModel()
        {
            Seed = string.Empty;

            GenerateButtonCommand = new Command(async () =>
            {
                Mnemonic mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve);

                Seed = mnemonic.ToString();
            });

            CreateButtonCommand = new Command(async () =>
            {
                await CoreMethods.PushPageModel<MainPageModel>();
            });
        }
    }
}
