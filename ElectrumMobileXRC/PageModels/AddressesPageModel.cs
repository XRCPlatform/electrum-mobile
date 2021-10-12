using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using FreshMvvm;
using ElectrumMobileXRC.Models;
using Xamarin.Forms;
using Xamarin.Essentials;
using ElectrumMobileXRC.Services;
using WalletProvider;

namespace ElectrumMobileXRC.PageModels
{
    public class AddressesPageModel : BasePageModel
    {
        private ObservableCollection<AddressItemModel> _receivingAddresses = new ObservableCollection<AddressItemModel>();
        public ObservableCollection<AddressItemModel> ReceivingAddresses
        {
            get
            {
                return _receivingAddresses;
            }
            set
            {
                _receivingAddresses = value;
            }
        }

        private ObservableCollection<AddressItemModel> _changeAddresses = new ObservableCollection<AddressItemModel>();
        public ObservableCollection<AddressItemModel> ChangeAddresses
        {
            get
            {
                return _changeAddresses;
            }
            set
            {
                _changeAddresses = value;
            }
        }

        public ICommand BackButtonCommand { get; set; }
        public ICommand MenuButtonCommand { get; set; }
        public ICommand CopyButtonCommand { get; set; }

        private ConfigDbService _configDb;

        public AddressesPageModel()
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

                        foreach (var address in deserializedWallet.ReceivingAddresses)
                        {
                            var addItem = new AddressItemModel();
                            addItem.Balance = 2;
                            addItem.Address = address.Address;
                            addItem.TxCount = address.Transactions.Count;
                            ReceivingAddresses.Add(addItem);
                        }

                        foreach (var address in deserializedWallet.ChangeAddresses)
                        {
                            var addItem = new AddressItemModel();
                            addItem.Balance = 2;
                            addItem.Address = address.Address;
                            addItem.TxCount = address.Transactions.Count;
                            ChangeAddresses.Add(addItem);
                        }
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
