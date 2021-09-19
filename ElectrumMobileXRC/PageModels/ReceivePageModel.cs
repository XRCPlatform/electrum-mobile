using FreshMvvm;
using Xamarin.Forms;
using QRCoder;
using System.IO;
using System.Windows.Input;

namespace ElectrumMobileXRC.PageModels
{
    public class ReceivePageModel : FreshBasePageModel
    {
        public ICommand BackButtonCommand { get; set; }
        public ICommand MenuButtonCommand { get; set; }

        public ImageSource QrCodeImage { get; set; }

        public ReceivePageModel()
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode("The text which should be encoded.", QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qRCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeBytes = qRCode.GetGraphic(20);
            QrCodeImage = ImageSource.FromStream(() => new MemoryStream(qrCodeBytes));

            BackButtonCommand = new Command(async () =>
            {
                await CoreMethods.PushPageModel<MainPageModel>();
            });

            MenuButtonCommand = new Command(async () =>
            {
                var actionSheet = await CoreMethods.DisplayActionSheet("Electrum Mobile XRC", "Back", null, "Addresses", "Network");

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
        }
    }
}
