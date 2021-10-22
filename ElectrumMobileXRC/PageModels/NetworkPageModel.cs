using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;
using ElectrumMobileXRC.Services;
using WalletProvider;
using ElectrumMobileXRC.Resources;
using System.Linq;
using NetworkProvider;

namespace ElectrumMobileXRC.PageModels
{
    public class NetworkPageModel : BasePageModel
    {
        public ICommand BackButtonCommand { get; set; }
        public ICommand MenuButtonCommand { get; set; }
        public ICommand SaveButtonCommand { get; set; }
        public string NetworkLastUpdate { get; set; }
        public string NetworkType { get; set; }
        public int NetworkLastSyncedBlock { get; set; }
        public string NetworkDefaultServer { get; set; }
        public int NetworkDefaultPort { get; set; }

        public int NetworkServersSelectedIndex { get; set; }

        private ObservableCollection<string> _networkServers = new ObservableCollection<string>();
        public ObservableCollection<string> NetworkServers
        {
            get
            {
                return _networkServers;
            }
            set
            {
                _networkServers = value;
            }
        }

        private ConfigDbService _configDb;
        private DbNetworkHelper _networkDbHelper;
        private DbWalletHelper _walletDbHelper;

        public NetworkPageModel()
        {
            _configDb = new ConfigDbService();
            _walletDbHelper = new DbWalletHelper(_configDb);

            NetworkServersSelectedIndex = 0;

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

            SaveButtonCommand = new Command(async () =>
            {
                HideErrorLabels();

                if (IsFormValid()) SaveConfiguration();
            });

            BackButtonCommand = new Command(async () =>
            {
                await CoreMethods.PushPageModel<MainPageModel>();
            });

            LoadNetworkDataAsync();
        }

        private async void LoadNetworkDataAsync()
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
                    var walletManager = new WalletManager();

                    var deserializedWallet = walletManager.DeserializeWalletMetadata(_walletDbHelper.SerializedWallet);
                    if (deserializedWallet.IsMainNetwork)
                    {
                        NetworkType = SharedResource.NetworkType_Main;
                    }
                    else
                    {
                        NetworkType = SharedResource.NetworkType_Test;
                    }

                    _networkDbHelper = new DbNetworkHelper(_configDb, deserializedWallet.IsMainNetwork);
                    await _networkDbHelper.LoadFromDbAsync();

                    NetworkLastUpdate = _networkDbHelper.NetworkLastUpdate;
                    NetworkLastSyncedBlock = _networkDbHelper.NetworkLastSyncedBlock;
                    NetworkDefaultServer = _networkDbHelper.NetworkDefaultServer;
                    NetworkDefaultPort = _networkDbHelper.NetworkDefaultPort;

                    NetworkServers.Add(SharedResource.NetworkSelection_Auto);
                    NetworkServers.Add(SharedResource.NetworkSelection_Specific);
                    foreach (var itemServer in _networkDbHelper.NetworkServers)
                    {
                        NetworkServers.Add(itemServer);
                    }

                    SetPickerValue();
                }
            }
        }

        private void SetPickerValue()
        {
            var selectedIndex = -1;
            var objPickerServers = CurrentPage.FindByName<Picker>("PickerServers");

            var mainServerId = NetworkConfig.MainNet.ToList().IndexOf(NetworkDefaultServer);
            if (mainServerId >= 0)
            {
                selectedIndex = mainServerId;
            }

            var testServerId = NetworkConfig.TestNet.ToList().IndexOf(NetworkDefaultServer);
            if (testServerId >= 0)
            {
                selectedIndex = testServerId;
            }

            if (!string.IsNullOrEmpty(NetworkDefaultServer) && selectedIndex == -1)
            {
                selectedIndex = 1; //own server
            } 
            else
            {
                selectedIndex = 0;
            }

            objPickerServers.SelectedIndex = selectedIndex;
        }

        private async void SaveConfiguration()
        {
            var walletManager = new WalletManager();
            var deserializedWallet = walletManager.DeserializeWalletMetadata(_walletDbHelper.SerializedWallet);

            switch (NetworkServersSelectedIndex)
            {
                case 0:
                    var random = new Random();
                    if (deserializedWallet.IsMainNetwork)
                    {
                        int index = random.Next(NetworkConfig.MainNet.Length);
                        NetworkDefaultServer = NetworkConfig.MainNet[index];
                    }
                    else
                    {
                        int index = random.Next(NetworkConfig.TestNet.Length);
                        NetworkDefaultServer = NetworkConfig.TestNet[index];
                    }

                    break;
                case 1:
                    //do nothing
                    break;

                default:
                    if (deserializedWallet.IsMainNetwork)
                    {
                        NetworkDefaultServer = NetworkConfig.MainNet[NetworkServersSelectedIndex - 2];
                    }
                    else
                    {
                        NetworkDefaultServer = NetworkConfig.TestNet[NetworkServersSelectedIndex - 2];
                    }

                    break;
            }

            await _networkDbHelper.UpdateServersAsync(NetworkDefaultServer, NetworkDefaultPort);

            await CoreMethods.DisplayAlert("Success", "New network configuration has been saved.", "OK");

            await CoreMethods.PushPageModel<MainPageModel>();
        }

        private bool IsFormValid()
        {
            var isValid = true;

            if ((NetworkServersSelectedIndex == 1) && (string.IsNullOrEmpty(NetworkDefaultServer)))
            {
                var objServerLabelError = CurrentPage.FindByName<Label>("ServerLabelError");
                objServerLabelError.Text = string.Format(SharedResource.Error_FieldRequired, "Own Server");
                objServerLabelError.IsVisible = true;
                isValid = false;
            }

            return isValid;
        }

        private void HideErrorLabels()
        {
            var objServerLabelError = CurrentPage.FindByName<Label>("ServerLabelError");
            objServerLabelError.IsVisible = false;
        }
    }
}
