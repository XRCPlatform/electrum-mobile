using NetworkProvider;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectrumMobileXRC.Services
{
    public class DbNetworkHelper
    {
        public string NetworkLastUpdate { get; set; }
        public int NetworkLastSyncedBlock { get; set; }
        public string NetworkDefaultServer { get; set; }
        public int NetworkDefaultPort { get; set; }

        private List<string> _networkServers = new List<string>();
        public List<string> NetworkServers
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

        private ConfigDbService _configDb { get; set; }
        private bool _isMainNetwork { get; set; }

        public DbNetworkHelper(ConfigDbService configDb, bool isMainNetwork)
        {
            _configDb = configDb;
            _isMainNetwork = isMainNetwork;

            NetworkLastUpdate = "N/A";
            NetworkLastSyncedBlock = 0;
            NetworkDefaultPort = 510021;

            if (_isMainNetwork)
            {
                NetworkDefaultServer = NetworkConfig.MainNet.First();

                foreach (var item in NetworkConfig.MainNet)
                {
                    NetworkServers.Add(item);
                }
            }
            else
            {
                NetworkDefaultServer = NetworkConfig.TestNet.First();

                foreach (var item in NetworkConfig.TestNet)
                {
                    NetworkServers.Add(item);
                }
            }
        }

        internal async Task LoadFromDbAsync()
        {
            var networkLastUpdateUtc = await _configDb.Get(DbConfig.CFG_NETWORKLASTUPDATEUTC);
            if ((networkLastUpdateUtc != null) && (!string.IsNullOrEmpty(networkLastUpdateUtc.Value)))
            {
                var lastUpdate = DateTime.Parse(networkLastUpdateUtc.Value).ToLocalTime();
                NetworkLastUpdate = string.Format("{0} {1}", lastUpdate.ToShortDateString(), lastUpdate.ToShortTimeString());
            }

            var networkLastSyncedBlock = await _configDb.Get(DbConfig.CFG_NETWORKLASTSYNCEDBLOCK);
            if ((networkLastSyncedBlock != null) && (!string.IsNullOrEmpty(networkLastSyncedBlock.Value)))
            {
                NetworkLastSyncedBlock = int.Parse(networkLastUpdateUtc.Value);
            }

            var networkDefaultServer = await _configDb.Get(DbConfig.CFG_NETWORKDEFAULTSERVER);
            if ((networkDefaultServer != null) && (!string.IsNullOrEmpty(networkDefaultServer.Value)))
            {
                NetworkDefaultServer = networkDefaultServer.Value.ToString();
            }

            var networkDefaultPort = await _configDb.Get(DbConfig.CFG_NETWORKDEFAULTPORT);
            if ((networkDefaultPort != null) && (!string.IsNullOrEmpty(networkDefaultPort.Value)))
            {
                NetworkDefaultPort = int.Parse(networkDefaultPort.Value);
            }
        }

        internal async Task UpdateServersAsync(string networkDefaultServer, int networkDefaultPort)
        {
            await _configDb.Add(DbConfig.CFG_NETWORKDEFAULTPORT, networkDefaultPort.ToString());
            await _configDb.Add(DbConfig.CFG_NETWORKDEFAULTSERVER, networkDefaultServer);
        }
    }
}
