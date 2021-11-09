using ElectrumMobileXRC.Models;
using ElectrumMobileXRC.Services;
using FreshMvvm;
using NBitcoin;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using WalletProvider;
using Xamarin.Forms;

namespace ElectrumMobileXRC.PageModels
{
    public class SendPageModel : BasePageModel
    {
        public ICommand BackButtonCommand { get; set; }
        public ICommand MenuButtonCommand { get; set; }
        public ICommand SendButtonCommand { get; set; }
        public ICommand CheckBoxCommand { get; set; }

        [Required]
        public string TargetAddress { get; set; }

        [Required]
        public decimal Amount { get; set; }

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

        private ConfigDbService _configDb;
        private DbWalletHelper _walletDbHelper;

        public SendPageModel()
        {
            _configDb = new ConfigDbService();
            _walletDbHelper = new DbWalletHelper(_configDb);

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

            SendButtonCommand = new Command(async () =>
            {
                var ss = Addresses;
            });

            LoadWalletAsync();
        }

        private async void LoadWalletAsync()
        {
            if (!IsUserValid())
            {
                await CoreMethods.PushPageModel<LoginPageModel>();
            }
            else
            {
                await _walletDbHelper.LoadFromDbAsync();
                if (!_walletDbHelper.IsWalletInit)
                {
                    await CoreMethods.PushPageModel<CreatePageModel>();
                }
                else
                {
                    var walletManager = new WalletManager(_walletDbHelper.SerializedWallet);

                    var coinType = walletManager.WalletMetadata.Wallet.Network.Consensus.CoinType;
                    var account = walletManager.WalletMetadata.Wallet.GetAccountByCoinType(WalletManager.DEFAULTACCOUNT, (CoinType)coinType);

                    var network = walletManager.GetNetwork(walletManager.WalletMetadata.IsMainNetwork);
                    var networkDbHelper = new DbNetworkHelper(_configDb, walletManager.WalletMetadata.IsMainNetwork);
                    await networkDbHelper.LoadFromDbAsync();

                    foreach (var address in account.ExternalAddresses)
                    {
                        var addressBalance = address.GetSpendableAmount(networkDbHelper.NetworkLastSyncedBlock, network);

                        if (addressBalance.confirmedAmount.Satoshi > 0)
                        {
                            var addItem = new AddressItemModel();
                            addItem.Balance = addressBalance.confirmedAmount.ToUnit(MoneyUnit.XRC);
                            addItem.Address = address.Address;
                            addItem.TxCount = address.Transactions.Count;
                            addItem.IsSelected = true;
                            Addresses.Add(addItem);
                        }
                    }
                }
            }
        }
    }
}
