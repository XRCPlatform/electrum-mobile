using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using FreshMvvm;
using ElectrumMobileXRC.Models;
using Xamarin.Forms;

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

        public ICommand OpenResultCommand { get; set; }

        public MainPageModel()
        {
            var RiskPercentage = "2";
            var CapitalSize = "0.2";
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

            OpenResultCommand = new Command(async () =>
            {
                double riskPerc, capSize, entryPrice, stopPrice, targetPrice;

                if (double.TryParse(RiskPercentage, out riskPerc) &&
                    double.TryParse(CapitalSize, out capSize) &&
                    double.TryParse(EntryPrice, out entryPrice) &&
                    double.TryParse(StopPrice, out stopPrice) &&
                    double.TryParse(TargetPrice, out targetPrice))
                {
                    var model = new InputModel
                    {
                        RiskPercentage = riskPerc,
                        CapitalSize = capSize,
                        EntryPrice = entryPrice,
                        StopPrice = stopPrice,
                        TargetPrice = targetPrice
                    };

                    await CoreMethods.PushPageModel<ResultPageModel>(model);
                }
            });
        }
    }
}
