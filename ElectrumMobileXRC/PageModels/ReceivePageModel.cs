using FreshMvvm;
using Xamarin.Forms;
using QRCoder;
using System.IO;
using System.Windows.Input;
using Xamarin.Essentials;

namespace ElectrumMobileXRC.PageModels
{
    public class ReceivePageModel : FreshBasePageModel
    {
        public ICommand BackButtonCommand { get; set; }
        public ICommand MenuButtonCommand { get; set; }
        public ICommand CopyButtonCommand { get; set; }


        public ImageSource QrCodeImage { get; set; }
        public string Address { get; set; }

        public ReceivePageModel()
        {
            Address = "TX7WVvmDtvMrx293ksM8io9Y15hdZUPPTq";

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(string.Format("xrc:{0}", Address), QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qRCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeBytes = qRCode.GetGraphic(20);
            QrCodeImage = ImageSource.FromStream(() => new MemoryStream(qrCodeBytes));

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
        }
    }
}
