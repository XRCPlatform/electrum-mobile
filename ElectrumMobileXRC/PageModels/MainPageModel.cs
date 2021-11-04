using System.Collections.ObjectModel;
using System.Windows.Input;
using ElectrumMobileXRC.Models;
using Xamarin.Forms;
using ElectrumMobileXRC.Services;
using WalletProvider;
using NetworkProvider;
using NBitcoin;
using System.Threading.Tasks;
using System.Linq;

namespace ElectrumMobileXRC.PageModels
{
    public class MainPageModel : BasePageModel
    {
        public string LastDateUpdate { get; set; }
        public decimal BalanceUnconfirmed { get; set; }
        public decimal Balance { get; set; }

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

        public ICommand SendButtonCommand { get; set; }
        public ICommand ReceiveButtonCommand { get; set; }
        public ICommand MenuButtonCommand { get; set; }

        private ConfigDbService _configDb;
        private DbNetworkHelper _networkDbHelper;
        private DbWalletHelper _walletDbHelper;
        private WalletManager _walletManager;
        private NetworkManager _networkManager;

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
                    _walletManager = new WalletManager(_walletDbHelper.SerializedWallet);

                    _networkDbHelper = new DbNetworkHelper(_configDb, _walletManager.Wallet.IsMainNetwork);
                    await _networkDbHelper.LoadFromDbAsync();

                    var network = _walletManager.GetNetwork(_walletManager.Wallet.IsMainNetwork);
                    _networkManager = new NetworkManager(
                        _networkDbHelper.NetworkDefaultServer,
                        _networkDbHelper.NetworkDefaultPort,
                        network);

                    await SyncWalletWithNetwork(network);
                }
            }
        }

        private async Task SyncWalletWithNetwork(Network network)
        {
            var blockchainTransactionData = await _networkManager.StartSyncingAsync(
                _walletManager.GetCombinedAddresses(),
                _networkDbHelper.NetworkLastSyncedBlock);
            
            if (_walletManager.SyncBlockchainData(blockchainTransactionData, network))
            {
                await _walletDbHelper.UpdateWalletAsync(_walletManager.SerializeWalletMetadata());
            }

            // await _networkDbHelper.UpdateNetworkInfoAsync(_networkManager.ServerInfo.Result.BlockHeight);

            await UpdateWalletUI(network);
        }

        private async Task UpdateWalletUI(Network network)
        {
            LastDateUpdate = _networkDbHelper.NetworkDateLastUpdate;

            var walletBalance = _walletManager.GetWalletBalance(_networkDbHelper.NetworkLastSyncedBlock, network);
            if (walletBalance != null)
            {
                Balance = walletBalance.AmountConfirmed.ToUnit(MoneyUnit.XRC);
                BalanceUnconfirmed = walletBalance.AmountUnconfirmed.ToUnit(MoneyUnit.XRC);
            }

            var walletTransactions = _walletManager.GetWalletHistory();
            if ((walletTransactions != null) && walletTransactions.Any())
            {
                foreach (var itemTransaction in walletTransactions)
                {
                    var historyItem = new TransactionHistoryItemModel();
                    historyItem.Balance = itemTransaction.Transaction.Amount.ToUnit(MoneyUnit.XRC);
                    historyItem.CreationDate = string.Format("{0} {1}",
                        itemTransaction.Transaction.CreationTime.DateTime.ToShortDateString(),
                        itemTransaction.Transaction.CreationTime.DateTime.ToShortTimeString());
                    TransactionHistory.Add(historyItem);
                }
            }
        }
    }
}
