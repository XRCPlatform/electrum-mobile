using ElectrumXClient;
using ElectrumXClient.Response;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ElectrumXClient.Tests
{
    public class UnitTestMethods
    {
        private Client _client;

        [SetUp]
        public void Setup()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("client-secrets.json")
                .Build();
            var host = "telectrum.xrhodium.org";
            var port = 51002;
            var useSSL = true;

            _client = new Client(host, port, useSSL);
        }

        [TearDown]
        public void Teardown()
        {
            _client.Dispose();
        }

        [Test]
        public async Task Test_CanGetServerVersion()
        {
            var response = await _client.GetServerVersion();
            Assert.IsInstanceOf<ServerVersionResponse>(response);
        }

        [Test]
        public async Task Test_CanGetServerFeatures()
        {
            var response = await _client.GetServerFeatures();
            Assert.IsInstanceOf<ServerFeaturesResponse>(response);
        }

        [Test]
        public async Task Test_CanGetServerPeersSubscribe()
        {
            var response = await _client.GetServerPeersSubscribe();
            Assert.IsInstanceOf<ServerPeersSubscribeResponse>(response);
        }

        [Test]
        public async Task Test_CanGetBlockchainNumblocksSubscribe()
        {
            var response = await _client.GetBlockchainNumblocksSubscribe();
            Assert.IsInstanceOf<BlockchainNumblocksSubscribeResponse>(response);
        }

        [Test]
        public async Task Test_CanGetBlockchainBlockHeader()
        {
            var response = await _client.GetBlockchainBlockHeader(0);
            Assert.IsInstanceOf<BlockchainBlockHeaderResponse>(response);
        }

        [Test]
        public async Task Test_CanGetBlockchainScripthashGetBalance()
        {
            var response = await _client.GetBlockchainScripthashGetBalance("914bb56abc57ee1317e039f249b9e3fd0ebc5a4b447bd244b8f56bb7c9704998");
            var response2 = await _client.GetBlockchainScripthashGetBalance("984970c9b76bf5b844d27b444b5abc0efde3b949f239e01713ee57bc6ab54b91");
            Assert.IsInstanceOf<BlockchainScripthashGetBalanceResponse>(response);
        }

        [Test]
        public async Task Test_CanGetBlockchainTransactionGet()
        {
            var response = await _client.GetBlockchainTransactionGet("4a5e1e4baab89f3a32518a88c31bc87f618f76673e2cc77ab2127b7afdeda33b");
            Assert.IsInstanceOf<BlockchainTransactionGetResponse>(response);
        }
        
        [Test]
        public async Task Test_CanGetScripthashGetHistory()
        {
            var response = await _client.GetBlockchainScripthashGetHistory("914bb56abc57ee1317e039f249b9e3fd0ebc5a4b447bd244b8f56bb7c9704998");
            var response2 = await _client.GetBlockchainScripthashGetHistory("984970c9b76bf5b844d27b444b5abc0efde3b949f239e01713ee57bc6ab54b91");
            Assert.IsInstanceOf<BlockchainScripthashGetHistoryResponse>(response);
            Assert.Greater(response.Result.Count, 0);
        }

        [Test]
        public async Task Test_CanGetScripthashListunspent()
        {
            var response = await _client.GetBlockchainListunspent("8b01df4e368ea28f8dc0423bcf7a4923e3a12d307c875e47a0cfbf90b5c39161");
            Assert.IsInstanceOf<BlockchainScripthashListunspentResponse>(response);
            Assert.Greater(response.Result.Count, 0);
        }

        [Test]
        public async Task Test_CanGetBlockchainEstimatefee()
        {
            var response = await _client.GetBlockchainEstimatefee(1);
            Assert.IsInstanceOf<BlockchainEstimatefeeResponse>(response);
        }

        [Test]
        public async Task Test_CanGetTxFromPos()
        {
            var response = await _client.GetTransactionIdFromPos(0, 0);
            Assert.IsInstanceOf<BlockchainTransactionIdFromPosResponse>(response);
        }
    }
}
