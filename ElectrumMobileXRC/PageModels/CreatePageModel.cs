using ElectrumMobileXRC.Resources;
using ElectrumMobileXRC.Services;
using FreshMvvm;
using NBitcoin;
using System;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using WalletProvider;
using WalletProvider.Entities;
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

        [Required]
        public int Type { get; set; }

        private ConfigDbService _configDb;

        private enum WalletImportType
        {
            NewWallet = 0,
            ImportElectrumRhodium = 1,
            ImportOldWebWallet = 2,
            ImportOldWebWalletBase64 = 3
        }

        public CreatePageModel()
        {
            Seed = string.Empty;

            _configDb = new ConfigDbService();

            GenerateButtonCommand = new Command(async () =>
            {
                var mnemonic = new MnemonicElectrum(Wordlist.English, WordCount.Twelve);
                Seed = mnemonic.ToString();
            });

            CreateButtonCommand = new Command(async (object data) =>
            {
                HideErrorLabels();

                var objActivityLayout = CurrentPage.FindByName<StackLayout>("ActivityLayout");
                objActivityLayout.IsVisible = true;
                var objSelectionLayout = CurrentPage.FindByName<StackLayout>("SelectionLayout");
                objSelectionLayout.IsVisible = false;
                var objGenerateButton = CurrentPage.FindByName<Button>("GenerateButton");
                objGenerateButton.IsEnabled = false;
                var objCreateButton = CurrentPage.FindByName<Button>("CreateButton");
                objCreateButton.IsEnabled = false;

                var walletManager = new WalletManager();

                if (IsFormValid())
                {
                    var walletMetadata = new WalletMetadata();

                    switch ((WalletImportType)Type)
                    {
                        case WalletImportType.ImportElectrumRhodium:
                           
                            IsFormElectrumSeedValid();
                            walletMetadata = walletManager.ImportElectrumWallet(Password, UserName, Seed, Passphrase);

                            break;
                        
                        case WalletImportType.ImportOldWebWallet:
                        
                            IsFormSeedValid();
                            IsFormPassphaseValid();
                            walletMetadata = walletManager.ImportWallet(Password, UserName, Seed, Passphrase);

                            break;
                        
                        case WalletImportType.ImportOldWebWalletBase64:
                         
                            IsFormSeedValid();
                            IsFormPassphaseValid();
                            walletMetadata = walletManager.ImportWebWalletBase64(Password, UserName, Seed, 1539810400, Passphrase);

                            break;

                        default: //WalletImportType.NewWallet
                            
                            IsFormSeedValid();
                            IsFormPassphaseValid();

                            var xx = Type;

                            walletManager.CreateElectrumWallet(Password, UserName, Seed, Passphrase);
                            // await CoreMethods.PushPageModel<MainPageModel>();

                            break;
                    }
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

            return isValid;
        }

        private bool IsFormSeedValid()
        {
            var isValid = true;

            try
            {
                Mnemonic mnemonic = new Mnemonic(Seed, null);
            }
            catch (NotSupportedException e)
            {
                var objUserNameError = CurrentPage.FindByName<Label>("SeedError");
                objUserNameError.Text = string.Format(SharedResource.Error_FieldContainsUnsupported, "Seed");
                objUserNameError.IsVisible = true;
                isValid = false;
            }
            catch (Exception e)
            {
                var objUserNameError = CurrentPage.FindByName<Label>("SeedError");
                objUserNameError.Text = string.Format(e.Message, "Seed");
                objUserNameError.IsVisible = true;
                isValid = false;
            }

            return isValid;
        }

        private bool IsFormElectrumSeedValid()
        {
            var isValid = true;

            try
            {
                MnemonicElectrum mnemonic = new MnemonicElectrum(Seed, null);
            }
            catch (NotSupportedException e)
            {
                var objUserNameError = CurrentPage.FindByName<Label>("SeedError");
                objUserNameError.Text = string.Format(SharedResource.Error_FieldContainsUnsupported, "Seed");
                objUserNameError.IsVisible = true;
                isValid = false;
            }
            catch (Exception e)
            {
                var objUserNameError = CurrentPage.FindByName<Label>("SeedError");
                objUserNameError.Text = string.Format(e.Message, "Seed");
                objUserNameError.IsVisible = true;
                isValid = false;
            }

            return isValid;
        }

        private bool IsFormPassphaseValid()
        {
            var isValid = true;

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
