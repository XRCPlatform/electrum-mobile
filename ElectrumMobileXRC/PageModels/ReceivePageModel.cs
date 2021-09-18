using FreshMvvm;
using Xamarin.Forms;
using QRCoder;
using System.IO;

namespace ElectrumMobileXRC.PageModels
{
    public class ReceivePageModel : FreshBasePageModel
    {
        public ImageSource QrCodeImage { get; set; }

        public ReceivePageModel()
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode("The text which should be encoded.", QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qRCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeBytes = qRCode.GetGraphic(20);
            QrCodeImage = ImageSource.FromStream(() => new MemoryStream(qrCodeBytes));
        }
    }
}
