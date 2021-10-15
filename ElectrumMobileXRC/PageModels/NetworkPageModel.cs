using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using FreshMvvm;
using ElectrumMobileXRC.Models;
using Xamarin.Forms;
using Xamarin.Essentials;
using ElectrumMobileXRC.Services;
using WalletProvider;
using Newtonsoft.Json;
using ElectrumMobileXRC.Resources;
using System.Linq;

namespace ElectrumMobileXRC.PageModels
{
    public class NetworkPageModel : BasePageModel
    {
        public ICommand BackButtonCommand { get; set; }
        public ICommand MenuButtonCommand { get; set; }
        public ICommand SaveButtonCommand { get; set; }

        public string[] NetworkTestNet =
        {
            "telectrum.xrhodium.org"
        };

        public string[] NetworkMainNet =
        {
            "electrumx1.xrhodium.org",
            "electrumx2.xrhodium.org",
            "electrumx3.xrhodium.org",
            "electrumx4.xrhodium.org",
        };

        private ConfigDbService _configDb;

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

        public NetworkPageModel()
        {
            _configDb = new ConfigDbService();

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

            LoadNetworkData();
        }

        private async void LoadNetworkData()
        {
            if (!IsUserValid())
            {
                await CoreMethods.PushPageModel<LoginPageModel>();
            }
            else
            {
                var walletInit = await _configDb.Get(DbConfiguration.CFG_WALLETINIT);

                if ((walletInit == null) || (string.IsNullOrEmpty(walletInit.Value)) || walletInit.Value != DbConfiguration.CFG_TRUE)
                {
                    await CoreMethods.PushPageModel<CreatePageModel>();
                }
                else
                {
                    var walletManager = new WalletManager();

                    var serializedWallet = await _configDb.Get(DbConfiguration.CFG_WALLETMETADATA);
                    if ((serializedWallet != null) && (!string.IsNullOrEmpty(serializedWallet.Value)))
                    {
                        var deserializedWallet = walletManager.DeserializeWalletMetadata(serializedWallet.Value);

                        if (deserializedWallet.IsMainNetwork)
                        {
                            NetworkType = SharedResource.NetworkType_Main;
                        }
                        else
                        {
                            NetworkType = SharedResource.NetworkType_Test;
                        }

                        var networkLastUpdateUtc = await _configDb.Get(DbConfiguration.CFG_NETWORKLASTUPDATEUTC);
                        if ((networkLastUpdateUtc == null) || (string.IsNullOrEmpty(networkLastUpdateUtc.Value)))
                        {
                            NetworkLastUpdate = "N/A";
                        }
                        else
                        {
                            var lastUpdate = DateTime.Parse(networkLastUpdateUtc.Value).ToLocalTime();
                            NetworkLastUpdate = string.Format("{0} {1}", lastUpdate.ToShortDateString(), lastUpdate.ToShortTimeString());
                        }

                        var networkLastSyncedBlock = await _configDb.Get(DbConfiguration.CFG_NETWORKLASTSYNCEDBLOCK);
                        if ((networkLastSyncedBlock == null) || (string.IsNullOrEmpty(networkLastSyncedBlock.Value)))
                        {
                            NetworkLastSyncedBlock = 0;
                        }
                        else
                        {
                            NetworkLastSyncedBlock = Int32.Parse(networkLastUpdateUtc.Value);
                        }

                        var networkDefaultServer = await _configDb.Get(DbConfiguration.CFG_NETWORKDEFAULTSERVER);
                        if ((networkDefaultServer == null) || (string.IsNullOrEmpty(networkDefaultServer.Value)))
                        {
                            if (deserializedWallet.IsMainNetwork)
                            {
                                NetworkDefaultServer = NetworkMainNet.First();
                            }
                            else
                            {
                                NetworkDefaultServer = NetworkTestNet.First();
                            }
                        }
                        else
                        {
                            NetworkDefaultServer = networkDefaultServer.Value.ToString();
                        }

                        var networkDefaultPort = await _configDb.Get(DbConfiguration.CFG_NETWORKDEFAULTPORT);
                        if ((networkDefaultPort == null) || (string.IsNullOrEmpty(networkDefaultPort.Value)))
                        {
                            NetworkDefaultPort = 51002;
                        }
                        else
                        {
                            NetworkDefaultPort = Int32.Parse(networkDefaultPort.Value);
                        }

                        var networkServers = await _configDb.Get(DbConfiguration.CFG_NETWORKSERVERS);
                        if ((networkServers == null) || (string.IsNullOrEmpty(networkServers.Value)))
                        {
                            NetworkServers.Add(SharedResource.NetworkSelection_Auto);
                            NetworkServers.Add(SharedResource.NetworkSelection_Specific);

                            if (deserializedWallet.IsMainNetwork)
                            {
                                foreach (var item in NetworkMainNet)
                                {
                                    NetworkServers.Add(item);
                                }
                            }
                            else
                            {
                                foreach (var item in NetworkTestNet)
                                {
                                    NetworkServers.Add(item);
                                }
                            }
                        }
                        else
                        {
                            NetworkServers = JsonConvert.DeserializeObject<ObservableCollection<string>>(networkServers.Value);
                        }

                        SetPickerValue();
                    }
                    else
                    {
                        await CoreMethods.PushPageModel<CreatePageModel>();
                    }
                }
            }
        }

        private void SetPickerValue()
        {
            var selectedIndex = -1;
            var objPickerServers = CurrentPage.FindByName<Picker>("PickerServers");

            var mainServerId = NetworkMainNet.ToList().IndexOf(NetworkDefaultServer);
            if (mainServerId >= 0)
            {
                selectedIndex = mainServerId;
            }

            var testServerId = NetworkTestNet.ToList().IndexOf(NetworkDefaultServer);
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

            var serializedWallet = await _configDb.Get(DbConfiguration.CFG_WALLETMETADATA);
            if ((serializedWallet != null) && (!string.IsNullOrEmpty(serializedWallet.Value)))
            {
                var deserializedWallet = walletManager.DeserializeWalletMetadata(serializedWallet.Value);

                switch (NetworkServersSelectedIndex)
                {
                    case 0:
                        var random = new Random();
                        if (deserializedWallet.IsMainNetwork)
                        {
                            int index = random.Next(NetworkMainNet.Length);
                            NetworkDefaultServer = NetworkMainNet[index];
                        } 
                        else
                        {
                            int index = random.Next(NetworkTestNet.Length);
                            NetworkDefaultServer = NetworkTestNet[index];
                        }

                        break;
                    case 1:
                        //do nothing
                        break;

                    default:
                        if (deserializedWallet.IsMainNetwork)
                        {
                            NetworkDefaultServer = NetworkMainNet[NetworkServersSelectedIndex - 2];
                        }
                        else
                        {
                            NetworkDefaultServer = NetworkTestNet[NetworkServersSelectedIndex - 2];
                        }

                        break;
                }

                await _configDb.Add(DbConfiguration.CFG_NETWORKDEFAULTPORT, NetworkDefaultPort.ToString());
                await _configDb.Add(DbConfiguration.CFG_NETWORKDEFAULTSERVER, NetworkDefaultServer);
            }
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
