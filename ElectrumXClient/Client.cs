using ElectrumXClient.Request;
using ElectrumXClient.Response;
using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace ElectrumXClient
{
    public class Client : IDisposable
    {
        private string _host;
        private int _port;
        private bool _useSSL;
        TcpClient _tcpClient;
        SslStream _sslStream;
        NetworkStream _tcpStream;
        Stream _stream;
        readonly int BUFFERSIZE = 1024;

        public Client(string host, int port, bool useSSL)
        {
            _host = host;
            _port = port;
            _useSSL = useSSL;
        }

        public async Task<ServerVersionResponse> GetServerVersion()
        {
            var request = new ServerVersionRequest();
            var requestData = request.GetRequestData<ServerVersionRequest>();
            await this.Connect();
            string response = await SendMessage(requestData);
            this.Disconnect();
            return ServerVersionResponse.FromJson(response);
        }

        public async Task<ServerPeersSubscribeResponse> GetServerPeersSubscribe()
        {
            var request = new ServerPeersSubscribeRequest();
            var requestData = request.GetRequestData<ServerPeersSubscribeRequest>();
            await this.Connect();
            string response = await SendMessage(requestData);
            this.Disconnect();
            return ServerPeersSubscribeResponse.FromJson(response);
        }

        public async Task<BlockchainHeadersSubscribeResponse> GetBlockchainHeadersSubscribe()
        {
            var request = new BlockchainHeadersSubscribeRequest();
            var requestData = request.GetRequestData<BlockchainHeadersSubscribeRequest>();
            await this.Connect();
            string response = await SendMessage(requestData);
            this.Disconnect();
            return BlockchainHeadersSubscribeResponse.FromJson(response);
        }

        public async Task<BlockchainBlockHeaderResponse> GetBlockchainBlockHeader(uint height)
        {
            var request = new BlockchainBlockHeaderRequest();
            request.Parameters = new object[1] { height };
            var requestData = request.GetRequestData<BlockchainBlockHeaderRequest>();
            await this.Connect();
            string response = await SendMessage(requestData);
            this.Disconnect();
            return BlockchainBlockHeaderResponse.FromJson(response);
        }

        public async Task<BlockchainEstimateFeeResponse> GetBlockchainEstimateFee(uint number)
        {
            var request = new BlockchainEstimateFeeRequest();
            request.Parameters = new uint[] { number };
            var requestData = request.GetRequestData<BlockchainEstimateFeeRequest>();
            await this.Connect();
            string response = await SendMessage(requestData);
            this.Disconnect();
            return BlockchainEstimateFeeResponse.FromJson(response);
        }

        public async Task<BlockchainRelayFeeResponse> GetBlockchainRelayFee()
        {
            var request = new BlockchainRelayFeeRequest();
            var requestData = request.GetRequestData<BlockchainRelayFeeRequest>();
            await this.Connect();
            string response = await SendMessage(requestData);
            this.Disconnect();
            return BlockchainRelayFeeResponse.FromJson(response);
        }

        public async Task<BlockchainScripthashGetBalanceResponse> GetBlockchainScripthashGetBalance(string scripthash)
        {
            var request = new BlockchainScripthashGetBalanceRequest();
            request.Parameters = new string[] { scripthash };
            var requestData = request.GetRequestData<BlockchainScripthashGetBalanceRequest>();
            await this.Connect();
            string response = await SendMessage(requestData);
            this.Disconnect();
            return BlockchainScripthashGetBalanceResponse.FromJson(response);
        }

        public async Task<BlockchainScripthashGetHistoryResponse> GetBlockchainScripthashGetHistory(string scripthash)
        {
            var request = new BlockchainScripthashGetHistoryRequest();
            request.Parameters = new string[] { scripthash };
            var requestData = request.GetRequestData<BlockchainScripthashGetHistoryRequest>();
            await this.Connect();
            string response = await SendMessage(requestData);
            this.Disconnect();
            return BlockchainScripthashGetHistoryResponse.FromJson(response);
        }

        public async Task<BlockchainScripthashGetMempoolResponse> GetBlockchainScripthashGetMempool(string scripthash)
        {
            var request = new BlockchainScripthashGetMempoolRequest();
            request.Parameters = new string[] { scripthash };
            var requestData = request.GetRequestData<BlockchainScripthashGetMempoolRequest>();
            await this.Connect();
            string response = await SendMessage(requestData);
            this.Disconnect();
            return BlockchainScripthashGetMempoolResponse.FromJson(response);
        }

        public async Task<BlockchainScripthashListunspentResponse> GetBlockchainListunspent(string scripthash)
        {
            var request = new BlockchainScripthashListunspentRequest();
            request.Parameters = new string[] { scripthash };
            var requestData = request.GetRequestData<BlockchainScripthashListunspentRequest>();
            await this.Connect();
            string response = await SendMessage(requestData);
            this.Disconnect();
            return BlockchainScripthashListunspentResponse.FromJson(response);
        }

        public async Task<BlockchainTransactionGetResponse> GetBlockchainTransactionGet(string txhash)
        {
            var request = new BlockchainTransactionGetRequest();
            request.Parameters = new object[] { txhash, true };
            var requestData = request.GetRequestData<BlockchainTransactionGetRequest>();
            await this.Connect();
            string response = await SendMessage(requestData);
            this.Disconnect();
            return BlockchainTransactionGetResponse.FromJson(response);
        }

        public async Task<BlockchainTransactionBroadcastResponse> BlockchainTransactionBroadcast(string tx)
        {
            var request = new BlockchainTransactionBroadcastRequest();
            request.Parameters = new object[] { tx };
            var requestData = request.GetRequestData<BlockchainTransactionBroadcastRequest>();
            await this.Connect();
            string response = await SendMessage(requestData);
            this.Disconnect();
            return BlockchainTransactionBroadcastResponse.FromJson(response);
        }

        public async Task<ServerFeaturesResponse> GetServerFeatures()
        {
            var request = new ServerFeaturesRequest();            
            var requestData = request.GetRequestData<ServerFeaturesRequest>();
            await this.Connect();
            string response = await SendMessage(requestData);
            this.Disconnect();
            return ServerFeaturesResponse.FromJson(response);
        }

        private async Task Connect()
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(_host, _port);
            if (_useSSL)
            {
                _sslStream = new SslStream(_tcpClient.GetStream(), true,
                    new RemoteCertificateValidationCallback(CertificateValidationCallback));
                await _sslStream.AuthenticateAsClientAsync(_host);
                _stream = _sslStream;
            }
            else
            {
                _tcpStream = _tcpClient.GetStream();
                _stream = _tcpStream;
            }
        }

        private void Disconnect()
        {
            _stream.Close();
            _tcpClient.Close();
        }

        private async Task<string> SendMessage(byte[] requestData)
        {
            var response = string.Empty;
            var buffer = new byte[BUFFERSIZE];
            await _stream.WriteAsync(requestData, 0, requestData.Length);

            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = await _stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                    if (read < BUFFERSIZE) break;
                }
                response = System.Text.Encoding.ASCII.GetString(ms.ToArray());
            }

            return response;
        }

        private static bool CertificateValidationCallback(object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // We don't check the validity of the certificate (yet)
            return true;
        }

        public void Dispose()
        {
            if (_sslStream != null) ((IDisposable)_sslStream).Dispose();
            ((IDisposable)_tcpClient).Dispose();
        }

        public async Task<BlockchainTransactionIdFromPosResponse> GetTransactionIdFromPos(uint height, uint index)
        {
            var request = new BlockchainTransactionIdFromPosRequest();
            request.Parameters = new object[2] { height, index };
            var requestData = request.GetRequestData<BlockchainTransactionIdFromPosRequest>();
            await this.Connect();
            string response = await SendMessage(requestData);
            this.Disconnect();
            return BlockchainTransactionIdFromPosResponse.FromJson(response);
        }

        public async Task<BlockchainTransactionGetMerkleResponse> GetTransactionGetMerkle(string txhash, int height)
        {
            var request = new BlockchainTransactionGetMerkleRequest();
            request.Parameters = new object[2] { txhash, height };
            var requestData = request.GetRequestData<BlockchainTransactionGetMerkleRequest>();
            await this.Connect();
            string response = await SendMessage(requestData);
            this.Disconnect();
            return BlockchainTransactionGetMerkleResponse.FromJson(response);
        }
    }
}
