using ElectrumXClient;
using NBitcoin;
using NetworkProvider.Utils;
using System;

namespace NetworkProvider
{
    public class NetworkManager : IDisposable
    {
        private string _server; 
        private int _port;
        private Network _net;
        private Client _electrumClient;

        public NetworkManager(string server, int port, Network net)
        {
            _server = server;
            _port = port;
            _net = net;
            _electrumClient = new Client(_server, _port, false);

            StartSyncingAsync();
        }

        private async void StartSyncingAsync()
        {
            var cnvHelper = new ConversionHelper();

            var address = BitcoinAddress.Create("TXF7wJVwt4sdwtGeE8oGAE78SJnu6nx6aq", _net);
            var bytes = address.ScriptPubKey.ToBytes();
            var P2PKHBytes = cnvHelper.ByteArrayToString(bytes);
            var P2PKH = cnvHelper.StringToByteArray(P2PKHBytes);
            var hass_addr = cnvHelper.sha256_hash_bytes(P2PKH);
            var hass_addr_reverse = cnvHelper.sha256_hash_reverse_bytes(P2PKH);

          //  var response = await _electrumClient.GetBlockchainScripthashGetBalance("914bb56abc57ee1317e039f249b9e3fd0ebc5a4b447bd244b8f56bb7c9704998");
            var x = true;
        }

        public void Dispose()
        {
            _electrumClient.Dispose();
        }
    }
}
