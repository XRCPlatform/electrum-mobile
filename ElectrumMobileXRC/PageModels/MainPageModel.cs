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
using WalletProvider.Entities;

namespace ElectrumMobileXRC.PageModels
{
    public class MainPageModel : BasePageModel
    {
        public string LastDateUpdate { get; set; }
        public string LastBlockUpdate { get; set; }
        public decimal BalanceUnconfirmed { get; set; }
        public decimal Balance { get; set; }

        private ObservableCollection<TransactionHistoryItemModel> _unconfirmedTransactions = new ObservableCollection<TransactionHistoryItemModel>();
        public ObservableCollection<TransactionHistoryItemModel> UnconfirmedTransactions
        {
            get
            {
                return _unconfirmedTransactions;
            }
            set
            {
                _unconfirmedTransactions = value;
            }
        }

        private ObservableCollection<TransactionHistoryItemModel> _confirmedTransactions = new ObservableCollection<TransactionHistoryItemModel>();
        public ObservableCollection<TransactionHistoryItemModel> ConfirmedTransactions
        {
            get
            {
                return _confirmedTransactions;
            }
            set
            {
                _confirmedTransactions = value;
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

                    _networkDbHelper = new DbNetworkHelper(_configDb, _walletManager.WalletMetadata.IsMainNetwork);
                    await _networkDbHelper.LoadFromDbAsync();

                    var network = _walletManager.GetNetwork(_walletManager.WalletMetadata.IsMainNetwork);
                    _networkManager = new NetworkManager(
                        _networkDbHelper.NetworkDefaultServer,
                        _networkDbHelper.NetworkDefaultPort,
                        network);

                    UpdateWalletUI(network);

                    await SyncWalletWithNetwork(network);
                }
            }
        }

        private async Task SyncWalletWithNetwork(Network network)
        {
            LastDateUpdate = Resources.SharedResource.Main_Syncing;

            var blockchainTransactionData = await _networkManager.StartSyncingAsync(
                _walletManager.GetCombinedAddresses(),
                _networkDbHelper.NetworkLastSyncedBlock);

            await _networkDbHelper.UpdateNetworkInfoAsync(_networkManager.ServerInfo.Result.BlockHeight);

            var isUpdated = _walletManager.SyncBlockchainData(blockchainTransactionData, network);
            if (isUpdated)
            {
                await _walletDbHelper.UpdateWalletAsync(_walletManager.SerializeWalletMetadata());
            }

            UpdateWalletUI(network);
        }

        private void UpdateWalletUI(Network network)
        {
            LastDateUpdate = _networkDbHelper.NetworkDateLastUpdate;
            LastBlockUpdate = "N/A";

            if (_networkDbHelper.NetworkLastSyncedBlock > -1) LastBlockUpdate = _networkDbHelper.NetworkLastSyncedBlock.ToString();

            var walletBalance = _walletManager.GetWalletBalance(_networkDbHelper.NetworkLastSyncedBlock, network);
            if (walletBalance != null)
            {
                Balance = walletBalance.AmountConfirmed.ToUnit(MoneyUnit.XRC);
                BalanceUnconfirmed = walletBalance.AmountUnconfirmed.ToUnit(MoneyUnit.XRC);
            }

            var walletTransactions = _walletManager.GetWalletHistory();
            if ((walletTransactions != null) && walletTransactions.Any())
            {
                var updatedConfirmedTransactions = new ObservableCollection<TransactionHistoryItemModel>();
                var updatedUnconfirmedTransactions = new ObservableCollection<TransactionHistoryItemModel>();

                foreach (var itemTransaction in walletTransactions)
                {
                    var historyItem = new TransactionHistoryItemModel();

                    if (itemTransaction.Address.IsChangeAddress())
                    {
                        historyItem.Balance = GetBalanceForOutputTransaction(itemTransaction).ToUnit(MoneyUnit.XRC) * -1;
                    }
                    else
                    {
                        historyItem.Balance = itemTransaction.Transaction.Amount.ToUnit(MoneyUnit.XRC);
                    }

                    if (itemTransaction.Transaction.IsConfirmed())
                    {
                        historyItem.CreationDate = string.Format("{0} {1}",
                            itemTransaction.Transaction.CreationTime.ToLocalTime().DateTime.ToShortDateString(),
                            itemTransaction.Transaction.CreationTime.ToLocalTime().DateTime.ToShortTimeString());

                        updatedConfirmedTransactions.Add(historyItem);
                    }
                    else
                    {
                        historyItem.CreationDate = Resources.SharedResource.Main_Transaction_State_Unconfirmed.ToUpper();

                        if (itemTransaction.Transaction.IsPropagated == false)
                            historyItem.CreationDate = Resources.SharedResource.Main_Transaction_State_Local.ToUpper();

                        updatedUnconfirmedTransactions.Add(historyItem);
                    }

                    ConfirmedTransactions = updatedConfirmedTransactions;
                    UnconfirmedTransactions = updatedUnconfirmedTransactions;
                }

                if (!UnconfirmedTransactions.Any())
                {
                    var objUnconfirmedTransactionsLabel = CurrentPage.FindByName<Label>("UnconfirmedTransactionsLabel");
                    objUnconfirmedTransactionsLabel.IsVisible = false;
                    var objUnconfirmedTransactions = CurrentPage.FindByName<ContentView>("UnconfirmedTransactions");
                    objUnconfirmedTransactions.IsVisible = false;
                }
            }
        }

        private Money GetBalanceForOutputTransaction(WalletTransaction itemTransaction)
        {
            var transationData = itemTransaction.Transaction.Transaction;

            var prevOutInputs = transationData.Inputs.Select(i => i.PrevOut).ToList();
            var sourceCoins =_walletManager.FindCoinsSatoshi(prevOutInputs);

            var feeCoins = sourceCoins - transationData.TotalOut.Satoshi;
            var outCoins = transationData.Outputs[0].Value.Satoshi;

            return new Money(feeCoins + outCoins);
        }
    }
}
