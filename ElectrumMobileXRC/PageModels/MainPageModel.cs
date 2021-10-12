using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using FreshMvvm;
using ElectrumMobileXRC.Models;
using Xamarin.Forms;
using ElectrumMobileXRC.Services;
using WalletProvider.Entities;
using WalletProvider;

namespace ElectrumMobileXRC.PageModels
{
    public class MainPageModel : FreshBasePageModel
    {
        // Not proud of this construction, but with Bitcoin there are a lot of decimals to
        // take into account which somehow get magically converted to e7 blabla.
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
                //RaisePropertyChanged();
                //NotifyPropertyChanged("YourList");
            }
        }

        public ObservableCollection<string> _Test2 = new ObservableCollection<string>();

        public ObservableCollection<string> Test2
        {
            get
            {
                return _Test2;
            }
            set
            {
                _Test2 = value;
                //RaisePropertyChanged();
                //NotifyPropertyChanged("YourList");
            }
        }

        public string TargetPrice { get; set; }
        public string EntryPrice { get; set; }
        public string StopPrice { get; set; }

        public ICommand SendButtonCommand { get; set; }
        public ICommand ReceiveButtonCommand { get; set; }
        public ICommand MenuButtonCommand { get; set; }

        private ConfigDbService _configDb;

        public MainPageModel()
        {
            _configDb = new ConfigDbService();

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

            LoadWallet();
        }

        private async void LoadWallet()
        {
            var walletInit = await _configDb.Get(DbConfiguration.CFG_WALLETINIT);
            
            if ((walletInit == null) || (string.IsNullOrEmpty(walletInit.Value)) || walletInit.Value != DbConfiguration.CFG_TRUE)
            {
                var walletManager = new WalletManager();

                var serializedWallet = await _configDb.Get(DbConfiguration.CFG_WALLETMETADATA);
                if ((serializedWallet != null) && (!string.IsNullOrEmpty(serializedWallet.Value)))
                {
                    var deserializedWallet = walletManager.DeserializeWalletMetadata(serializedWallet.Value);
                } 
                else
                {
                    await CoreMethods.PushPageModel<CreatePageModel>();
                }
            }
            else
            {
                await CoreMethods.PushPageModel<CreatePageModel>();
            }

            LastDateUpdate = string.Format("{0} {1}",
                DateTime.Now.ToShortDateString(),
                DateTime.Now.ToShortTimeString());
            Balance = 2;
            BalanceUnconfirmed = 0.00000001;

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

            Test2.Add("xsxsx");
            Test2.Add("xxxsxsx");
        }
    }
}
