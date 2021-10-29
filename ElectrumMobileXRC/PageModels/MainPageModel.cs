using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using FreshMvvm;
using ElectrumMobileXRC.Models;
using Xamarin.Forms;
using ElectrumMobileXRC.Services;
using WalletProvider.Entities;
using WalletProvider;
using NetworkProvider;
using System.Threading;

namespace ElectrumMobileXRC.PageModels
{
    public class MainPageModel : BasePageModel
    {
        public string LastDateUpdate { get; set; }
        public double BalanceUnconfirmed { get; set; }
        public double Balance { get; set; }

        private ObservableCollection<TransactionHistoryItemModel> _transactionHistory = new ObservableCollection<TransactionHistoryItemModel>();
        public ObservableCollection<TransactionHistoryItemModel> TransactionHistory
        {
            get
            {
                return _transactionHistory;
            }
            set
            {
                _transactionHistory = value;
            }
        }

        public string TargetPrice { get; set; }
        public string EntryPrice { get; set; }
        public string StopPrice { get; set; }
        public ICommand SendButtonCommand { get; set; }
        public ICommand ReceiveButtonCommand { get; set; }
        public ICommand MenuButtonCommand { get; set; }

        private ConfigDbService _configDb;
        private DbNetworkHelper _networkDbHelper;
        private DbWalletHelper _walletDbHelper;

        public MainPageModel()
        {
            _configDb = new ConfigDbService();
            _walletDbHelper = new DbWalletHelper(_configDb);

            SendButtonCommand = new Command(async () =>
            {
                await CoreMethods.PushPageModel<SendPageModel>();
            });

            ReceiveButtonCommand = new Command(async () =>
            {
                await CoreMethods.PushPageModel<ReceivePageModel>();
            });

            MenuButtonCommand = new Command(async () =>
            {
                var actionSheet = await CoreMethods.DisplayActionSheet("Electrum Mobile XRC", "Hide", null, "Addresses", "Network" );

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

                    _networkDbHelper = new DbNetworkHelper(_configDb, walletManager.Wallet.IsMainNetwork);
                    await _networkDbHelper.LoadFromDbAsync();

                    var networkManager = new NetworkManager(_networkDbHelper.NetworkDefaultServer, _networkDbHelper.NetworkDefaultPort,
                            walletManager.GetNetwork(walletManager.Wallet.IsMainNetwork));

                    var blockchainTransactionData = await networkManager.StartSyncingAsync(walletManager.Wallet);
                    walletManager.SyncBlockchainData(blockchainTransactionData);
                }

                LastDateUpdate = string.Format("{0} {1}",
                    DateTime.Now.ToShortDateString(),
                    DateTime.Now.ToShortTimeString());
                Balance = 0;
                BalanceUnconfirmed = 0;

                var historyItem = new TransactionHistoryItemModel();
                historyItem.Balance = 2;
                historyItem.CreationDate = string.Format("{0} {1}",
                    DateTime.Now.ToShortDateString(),
                    DateTime.Now.ToShortTimeString());
                TransactionHistory.Add(historyItem);

                historyItem = new TransactionHistoryItemModel();
                historyItem.Balance = -200;
                historyItem.CreationDate = string.Format("{0} {1}",
                    DateTime.Now.ToShortDateString(),
                    DateTime.Now.ToShortTimeString());
                TransactionHistory.Add(historyItem);

                historyItem = new TransactionHistoryItemModel();
                historyItem.Balance = 200;
                historyItem.CreationDate = string.Format("{0} {1}",
                    DateTime.Now.ToShortDateString(),
                    DateTime.Now.ToShortTimeString());
                TransactionHistory.Add(historyItem);
            }
        }

    }
}
