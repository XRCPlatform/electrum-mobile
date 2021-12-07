using ElectrumMobileXRC.Models;
using ElectrumMobileXRC.Resources;
using ElectrumMobileXRC.Services;
using FreshMvvm;
using NBitcoin;
using NetworkProvider;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
                    UpdateFeeEstimation();
                }
            }
        }

        [Required]
        public string TargetAddress { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string TransactionHex { get; set; }

        [Required]
        public string TransactionId { get; set; }

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
        private ConcurrentDictionary<FeeType, Money> _feeEstimationCalculationCache;

        public SendPageModel()
        {
            _configDb = new ConfigDbService();
            _walletDbHelper = new DbWalletHelper(_configDb);
            _feeEstimationCalculationCache = new ConcurrentDictionary<FeeType, Money>();

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
                    bool answer = await CoreMethods.DisplayAlert(SharedResource.Send_DialogConfirmationTitle,
                        SharedResource.Send_DialogConfirmation, SharedResource.Yes, SharedResource.No);

                    if (answer)
                    {
                        var objActivityLayout = CurrentPage.FindByName<StackLayout>("ActivityLayout");
                        var objResultLayout = CurrentPage.FindByName<StackLayout>("ResultLayout");
                        var objSendLayout = CurrentPage.FindByName<StackLayout>("SendLayout");
                        var objSendButton = CurrentPage.FindByName<Button>("SendButton");
                        objActivityLayout.IsVisible = true;
                        objSendLayout.IsVisible = false;
                        objSendButton.IsEnabled = false;

                        var feeType = ConvertToFeeType(FeeSliderValue);
                        var unitType = GetUnitFromIndex(UnitTypeIndex);
                        var amount = new Money(Amount, unitType);

                        try
                        {
                            var outPoints = _walletManager.GetOutPoins(Addresses.Where(a => a.IsSelected)
                                                          .Select(a => a.Address).ToList(), _networkDbHelper.NetworkLastSyncedBlock);

                            var transaction = await _walletManager.CreateTransaction(feeType, amount,
                                TargetAddress, _networkDbHelper.NetworkLastSyncedBlock, Password, outPoints);

                            TransactionHex = transaction.ToHex();
                            TransactionId = transaction.GetHash().ToString();

                            var result = await _networkManager.TransactionBroadcast(TransactionHex);
                            if (result == TransactionId)
                            {
                                await CoreMethods.DisplayAlert(SharedResource.Send_DialogDone, "", "Ok");
                            }
                            else
                            {
                                throw new Exception(result);
                            }
                        }
                        catch (Exception e)
                        {
                            await CoreMethods.DisplayAlert(e.Message, "", "Ok");
                        }

                        objActivityLayout.IsVisible = false;
                        objResultLayout.IsVisible = true;
                    }
                }
            });

            UnitTypes = new ObservableCollection<string>(Enum.GetNames(typeof(MoneyUnit)).Reverse().ToList());
            UnitTypeIndex = 0;
            FeeSliderValue = 0;

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
            var feeType = ConvertToFeeType(FeeSliderValue);
            var transactionMoneyFee = new Money(0);

            if (!_feeEstimationCalculationCache.TryGetValue(feeType, out transactionMoneyFee))
            {
                try
                {
                    var minRelayFee = await _networkManager.GetRelayFee();

                    var transaction = await _walletManager.GetFakeTransactionForEstimation(new Money(1),
                        _walletManager.GetFirstAddresses().Address, _networkDbHelper.NetworkLastSyncedBlock);

                    var transactionSize = transaction.GetVirtualSize();
                    var estimateForBlockHeight = (uint)_networkDbHelper.NetworkLastSyncedBlock + FeeToBlocks(feeType);
                    var minFeePerKb = await _networkManager.GetEstimateFee((uint)_networkDbHelper.NetworkLastSyncedBlock);

                    var transactionFee = transactionSize * minFeePerKb;
                    if (transactionFee < minRelayFee)
                    {
                        transactionFee = minRelayFee;
                    }

                    transactionMoneyFee = new Money(transactionFee, MoneyUnit.XRC);

                    _feeEstimationCalculationCache.TryAdd(feeType, transactionMoneyFee);
                }
                catch (Exception e)
                {
                    await CoreMethods.DisplayAlert(e.Message, "", "Ok");
                }
            } 

            var objFee = CurrentPage.FindByName<Label>("Fee");
            objFee.Text = string.Format(SharedResource.Send_EstimateFee, transactionMoneyFee.ToUnit(MoneyUnit.XRC), FeeToBlocks(feeType));
        }

        private int FeeToBlocks(FeeType feeType)
        {
            var blocks = 25;

            switch (feeType)
            {
                case FeeType.VeryHigh:
                    blocks = 1;
                    break;
                case FeeType.High:
                    blocks = 2;
                    break;
                case FeeType.Medium:
                    blocks = 5;
                    break;
                case FeeType.Low:
                    blocks = 10;
                    break;
                default:
                    break;
            }

            return blocks;
        }

        private FeeType ConvertToFeeType(int feeValue)
        {
            var feeType = FeeType.Low;

            switch (feeValue)
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

            return feeType;
        }

        private void HideErrorLabels()
        {
            var objTargetAddressError = CurrentPage.FindByName<Label>("TargetAddressError");
            objTargetAddressError.IsVisible = false;
            var objAmountError = CurrentPage.FindByName<Label>("AmountError");
            objAmountError.IsVisible = false;
            var objFeeError = CurrentPage.FindByName<Label>("FeeError");
            objFeeError.IsVisible = false;
            var objPasswordError = CurrentPage.FindByName<Label>("PasswordError");
            objPasswordError.IsVisible = false;
            var objAddressesError = CurrentPage.FindByName<Label>("AddressesError");
            objAddressesError.IsVisible = false;
        }

        private bool IsFormValid()
        {
            var isValid = true;

            if (string.IsNullOrEmpty(Password))
            {
                var objPasswordError = CurrentPage.FindByName<Label>("PasswordError");
                objPasswordError.Text = string.Format(SharedResource.Error_FieldRequired, "Password");
                objPasswordError.IsVisible = true;
                isValid = false;
            }

            if (!Addresses.Any())
            {
                var objAddressesError = CurrentPage.FindByName<Label>("AddressesError");
                objAddressesError.Text = string.Format(SharedResource.Error_FieldRequired, "From");
                objAddressesError.IsVisible = true;
                isValid = false;
            }

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
            else
            {
                var isTooManyDecimals = false;
                var unitType = GetUnitFromIndex(UnitTypeIndex);

                switch (unitType)
                {
                    case MoneyUnit.MilliXRC:
                        if (decimal.Round(Amount, 5) != Amount) isTooManyDecimals = true;
                        break;

                    case MoneyUnit.Bit:
                        if (decimal.Round(Amount, 2) != Amount) isTooManyDecimals = true;
                        break;

                    case MoneyUnit.Satoshi:
                        if (decimal.Round(Amount, 0) != Amount) isTooManyDecimals = true;
                        break;

                    default:
                        if (decimal.Round(Amount, 8) != Amount) isTooManyDecimals = true;
                        break;
                }

                if (isTooManyDecimals)
                {
                    var objAmount = CurrentPage.FindByName<Label>("AmountError");
                    objAmount.Text = string.Format(SharedResource.Error_TooMuchDecimals, "Amount");
                    objAmount.IsVisible = true;
                    isValid = false;
                }
            }

            return isValid;
        }

        private MoneyUnit GetUnitFromIndex(int unitTypeIndex)
        {
            switch (unitTypeIndex)
            {
                case 1:
                    return MoneyUnit.MilliXRC;
  
                case 2:
                    return MoneyUnit.Bit;

                case 3:
                    return MoneyUnit.Satoshi;

                default:
                    return MoneyUnit.XRC;
            }
        }
    }
}
