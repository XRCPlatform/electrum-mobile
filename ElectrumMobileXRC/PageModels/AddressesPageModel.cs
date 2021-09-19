using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using FreshMvvm;
using ElectrumMobileXRC.Models;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace ElectrumMobileXRC.PageModels
{
    public class AddressesPageModel : FreshBasePageModel
    {
        private ObservableCollection<AddressItemModel> _addresses = new ObservableCollection<AddressItemModel>();
        public ObservableCollection<AddressItemModel> Addresses
        {
            get
            {
                return _addresses;
            }
            set
            {
                _addresses = value;
            }
        }

        public ICommand BackButtonCommand { get; set; }
        public ICommand MenuButtonCommand { get; set; }
        public ICommand CopyButtonCommand { get; set; }

        public AddressesPageModel()
        {
            var addItem = new AddressItemModel();
            addItem.Balance = 2;
            addItem.Address = "TX7WVvmDtvMrx293ksM8io9Y15hdZUPPTq";
            Addresses.Add(addItem);

            addItem = new AddressItemModel();
            addItem.Balance = 200.00001;
            addItem.Address = "TGu88zPeBZ3A6E5ESvQPkiX5Hyn5oJhUtG";
            Addresses.Add(addItem);


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
