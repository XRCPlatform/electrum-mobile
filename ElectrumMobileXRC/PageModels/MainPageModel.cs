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
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElectrumMobileXRC.PageModels
{
    public class MainPageModel : BasePageModel, INotifyPropertyChanged
    {
        private string _lastDateUpdateFormatted;
        public string LastDateUpdateFormatted
        {
            get { return _lastDateUpdateFormatted; }
            set
            {
                SetProperty(ref _lastDateUpdateFormatted, value);
            }
        }
        public void ChangeLastDateUpdateValue(string val)
        {
            LastDateUpdateFormatted = val;
        }

        private string _lastBlockUpdate;
        public string LastBlockUpdate
        {
            get { return _lastBlockUpdate; }
            set
            {
                SetProperty(ref _lastBlockUpdate, value);
            }
        }

        private decimal _balanceUnconfirmed;
        public decimal BalanceUnconfirmed
        {
            get { return _balanceUnconfirmed; }
            set
            {
                SetProperty(ref _balanceUnconfirmed, value);
            }
        }

        private decimal _balance;
        public decimal Balance
        {
            get { return _balance; }
            set
            {
                SetProperty(ref _balance, value);
            }
        }

        private ObservableCollection<TransactionHistoryItemModel> _unconfirmedTransactions = new ObservableCollection<TransactionHistoryItemModel>();
        public ObservableCollection<TransactionHistoryItemModel> UnconfirmedTransactions
        {
            get
            {
                return _unconfirmedTransactions;
            }
            set
            {
                SetProperty(ref _unconfirmedTransactions, value);
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
                SetProperty(ref _confirmedTransactions, value);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChange(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

        protected bool SetProperty<T>(ref T prop, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(prop, value)) return false;
            prop = value;
            this.RaisePropertyChange(propertyName);
            return true;
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
                var actionSheet = await CoreMethods.DisplayActionSheet("Electrum Mobile XRC", "Hide", null, "Addresses", "Network", "Force synchronization");

                switch (actionSheet)
                {
                    case "Addresses":
                        await CoreMethods.PushPageModel<AddressesPageModel>();

                        break;

                    case "Network":
                        await CoreMethods.PushPageModel<NetworkPageModel>();

                        break;

                    case "Force synchronization":
                        
                        if (_networkDbHelper.NetworkDateLastUpdate < DateTime.UtcNow.AddSeconds(-5))
                        {
                            var network = _walletManager.GetNetwork(_walletManager.WalletMetadata.IsMainNetwork);
                            await SyncWalletWithNetwork(network);
                        } 
                        else
                        {
                            await CoreMethods.DisplayAlert("You are too fast. Try it again in several seconds.", "", "Ok");
                        }

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

                    if (_networkDbHelper.NetworkDateLastUpdate < DateTime.UtcNow.AddSeconds(-60))
                    {
                        await SyncWalletWithNetwork(network);
                    }
                }
            }
        }

        private async Task SyncWalletWithNetwork(Network network)
        {
            var blockchainTransactionData = await _networkManager.StartSyncingAsync(
                _walletManager.GetCombinedAddresses(),
                _networkDbHelper.NetworkLastSyncedBlock,
                Resources.SharedResource.Main_Syncing,
                (text) => ChangeLastDateUpdateValue(text));

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
            ChangeLastDateUpdateValue(_networkDbHelper.NetworkDateLastUpdateFormatted);
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
                    if (itemTransaction.Transaction.Id.ToString() == "037c392aab3ed28340d896234e449e587dc33b906b2b7be8f1290f5f5b02754d")
                    {
                        var s = true;
                    }

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
                        historyItem.Height = itemTransaction.Transaction.BlockHeight;

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
