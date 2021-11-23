using ElectrumMobileXRC.Models;
using ElectrumMobileXRC.Resources;
using ElectrumMobileXRC.Services;
using FreshMvvm;
using NBitcoin;
using NetworkProvider;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WalletProvider;
using WalletProvider.Entities;
using Xamarin.Forms;

namespace ElectrumMobileXRC.PageModels
{
    public class SendPageModel : BasePageModel, INotifyPropertyChanged
    {
        public ICommand BackButtonCommand { get; set; }
        public ICommand MenuButtonCommand { get; set; }
        public ICommand SendButtonCommand { get; set; }
        public ICommand TargetAddressChangedCommand { get; set; }


        private int _feeSliderValue { get; set; }

        [Required]
        public int FeeSliderValue
        {
            get { return _feeSliderValue; }
            set
            {
                if (_feeSliderValue != value)
                {
                    _feeSliderValue = value;
                    // UpdateFeeEstimation();
                }
            }
        }

        [Required]
        public string TargetAddress { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public int UnitTypeIndex { get; set; }

        private ObservableCollection<string> _unitTypes = new ObservableCollection<string>();
        public ObservableCollection<string> UnitTypes
        {
            get
            {
                return _unitTypes;
            }
            set
            {
                _unitTypes = value;
            }
        }

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
        private DbNetworkHelper _networkDbHelper;
        private DbWalletHelper _walletDbHelper;
        private NetworkManager _networkManager;
        private WalletManager _walletManager;

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
                HideErrorLabels();

                if (IsFormValid())
                {
                    //send tx
                }
            });

            UnitTypes = new ObservableCollection<string>(Enum.GetNames(typeof(MoneyUnit)).Reverse().ToList());
            UnitTypeIndex = 0;
            FeeSliderValue = 1;

            var s = Thread.CurrentThread.CurrentUICulture;

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

                    var coinType = _walletManager.WalletMetadata.Wallet.Network.Consensus.CoinType;
                    var account = _walletManager.WalletMetadata.Wallet.GetAccountByCoinType(WalletManager.DEFAULTACCOUNT, (CoinType)coinType);

                    var network = _walletManager.GetNetwork(_walletManager.WalletMetadata.IsMainNetwork);
                    _networkDbHelper = new DbNetworkHelper(_configDb, _walletManager.WalletMetadata.IsMainNetwork);
                    await _networkDbHelper.LoadFromDbAsync();
                    
                    _networkManager = new NetworkManager(
                        _networkDbHelper.NetworkDefaultServer,
                        _networkDbHelper.NetworkDefaultPort,
                        network);

                    var isSynced = await _networkManager.IsSynced(_networkDbHelper.NetworkLastSyncedBlock);
                    if (isSynced)
                    {
                        foreach (var address in account.GetCombinedAddresses())
                        {
                            var addressBalance = address.GetSpendableAmount(_networkDbHelper.NetworkLastSyncedBlock, network);

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
                    else
                    {
                        await CoreMethods.DisplayAlert("Your wallet is out of sync. Please, return to main screen and wait to full synchronization.", "", "Ok");
                    }
                }
            }
        }

        private async void UpdateFeeEstimation()
        {
            var objFee = CurrentPage.FindByName<Label>("Fee");
            var feeType = FeeType.Low;

            switch (FeeSliderValue)
            {
                case 0:
                    feeType = FeeType.VeryLow;
                    break;
                case 2:
                    feeType = FeeType.Medium;
                    break;
                case 3:
                    feeType = FeeType.High;
                    break;
                case 4:
                    feeType = FeeType.VeryHigh;
                    break;
                default:
                    break;
            }

            var minRelayFee = await _networkManager.GetRelayFee();


            //generate fake tx
            //estimate size
            //    calsulate fee based on fee date per kb
            //    compare it


            //      var tx = _walletManager.GetFakeTransactionForEstimation(FeeType.Low, new Money(1000), "TQXdPYbtmyvyeXnEsZXygBN75jyTDb8z1m", _networkDbHelper.NetworkLastSyncedBlock);

            //    _networkManager.GetEstimateFee();
            objFee.Text = minRelayFee.ToString();
        }


        private void HideErrorLabels()
        {
            var objTargetAddressError = CurrentPage.FindByName<Label>("TargetAddressError");
            objTargetAddressError.IsVisible = false;
            var objAmountError = CurrentPage.FindByName<Label>("AmountError");
            objAmountError.IsVisible = false;
            var objFeeError = CurrentPage.FindByName<Label>("FeeError");
            objFeeError.IsVisible = false;
        }

        private bool IsFormValid()
        {
            var isValid = true;

            if (string.IsNullOrEmpty(TargetAddress))
            {
                var objTargetAddressError = CurrentPage.FindByName<Label>("TargetAddressError");
                objTargetAddressError.Text = string.Format(SharedResource.Error_FieldRequired, "Pay To");
                objTargetAddressError.IsVisible = true;
                isValid = false;
            }
            else
            {
                var network = _walletManager.GetNetwork(_walletManager.WalletMetadata.IsMainNetwork);

                try
                {
                    BitcoinAddress.Create(TargetAddress, network);
                }
                catch (Exception)
                {
                    var objTargetAddressError = CurrentPage.FindByName<Label>("TargetAddressError");
                    objTargetAddressError.Text = string.Format(SharedResource.Error_AddressIsntValid, "Pay To");
                    objTargetAddressError.IsVisible = true;
                    isValid = false;
                }
            }

            if (Amount <= 0)
            {
                var objAmount = CurrentPage.FindByName<Label>("AmountError");
                objAmount.Text = string.Format(SharedResource.Error_FieldRequired, "Amount");
                objAmount.IsVisible = true;
                isValid = false;
            }

            return isValid;
        }

    }
}
