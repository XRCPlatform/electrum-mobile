using FreshMvvm;
using Xamarin.Forms;
using QRCoder;
using System.IO;
using System.Windows.Input;
using Xamarin.Essentials;
using ElectrumMobileXRC.Services;
using WalletProvider;
using System.Linq;

namespace ElectrumMobileXRC.PageModels
{
    public class ReceivePageModel : BasePageModel
    {
        public ICommand BackButtonCommand { get; set; }
        public ICommand MenuButtonCommand { get; set; }
        public ICommand CopyButtonCommand { get; set; }

        public ImageSource QrCodeImage { get; set; }
        public string Address { get; set; }

        private ConfigDbService _configDb;

        public ReceivePageModel()
        {
            _configDb = new ConfigDbService();

            BackButtonCommand = new Command(async () =>
            {
                await CoreMethods.PushPageModel<MainPageModel>();
            });

            MenuButtonCommand = new Command(async () =>
            {
                var actionSheet = await CoreMethods.DisplayActionSheet("Electrum Mobile XRC", "Hide", null, "Addresses", "Network");

                switch (actionSheet)
                {
                    case "Addresses":
                        await CoreMethods.PushPageModel<AddressesPageModel>();

                        break;

                    case "Network":
                        await CoreMethods.PushPageModel<NetworkPageModel>();

                        break;
                }
            });

            CopyButtonCommand = new Command(async (value) =>
            {
                await Clipboard.SetTextAsync((string)value);
                if (Clipboard.HasText)
                {
                    var text = await Clipboard.GetTextAsync();

                    await CoreMethods.DisplayAlert("Success", string.Format("Your copied address is ({0})", (string)value), "OK");
                }
            });

            LoadWallet();
        }

        private async void LoadWallet()
        {
            if (!IsUserValid())
            {
                await CoreMethods.PushPageModel<LoginPageModel>();
            }
            else
            {
                var walletInit = await _configDb.Get(DbConfiguration.CFG_WALLETINIT);

                if ((walletInit == null) || (string.IsNullOrEmpty(walletInit.Value)) || walletInit.Value != DbConfiguration.CFG_TRUE)
                {
                    await CoreMethods.PushPageModel<CreatePageModel>();
                }
                else
                {
                    var walletManager = new WalletManager();

                    var serializedWallet = await _configDb.Get(DbConfiguration.CFG_WALLETMETADATA);
                    if ((serializedWallet != null) && (!string.IsNullOrEmpty(serializedWallet.Value)))
                    {
                        var deserializedWallet = walletManager.DeserializeWalletMetadata(serializedWallet.Value);

                        Address = deserializedWallet.ReceivingAddresses.First().Address;

                        QRCodeGenerator qrGenerator = new QRCodeGenerator();
                        QRCodeData qrCodeData = qrGenerator.CreateQrCode(string.Format("xrc:{0}", Address), QRCodeGenerator.ECCLevel.Q);
                        PngByteQRCode qRCode = new PngByteQRCode(qrCodeData);
                        byte[] qrCodeBytes = qRCode.GetGraphic(20);
                        QrCodeImage = ImageSource.FromStream(() => new MemoryStream(qrCodeBytes));
                    }
                    else
                    {
                        await CoreMethods.PushPageModel<CreatePageModel>();
                    }
                }
            }
        }
    }
}
