using ElectrumXClient;
using ElectrumXClient.Response;
using NBitcoin;
using NetworkProvider.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WalletProvider.Entities;

namespace NetworkProvider
{
    public class NetworkManager : IDisposable
    {
        private string _server; 
        private int _port;
        private Network _net;
        private Client _electrumClient;
        public BlockchainHeadersSubscribeResponse ServerInfo { get; set; }

        public NetworkManager(string server, int port, Network net)
        {
            _server = server;
            _port = port;
            _net = net;
            _electrumClient = new Client(_server, _port, true);
        }

        public async Task<bool> IsSynced(int lastSyncedHeight)
        {
            ServerInfo = await _electrumClient.GetBlockchainHeadersSubscribe();
            if ((ServerInfo == null) || (ServerInfo.Result == null) || (lastSyncedHeight < ServerInfo.Result.BlockHeight)) 
            {
                return false;
            } 
            else
            {
                return true;
            }
        }

        public async Task<List<WalletTransaction>> StartSyncingAsync(IEnumerable<HdAddress> addresses, int lastSyncedHeight,
            string progressText, Action<string> UpdateProgress)
        {
            var cnvHelper = new ConversionHelper();
            var transactionList = new List<WalletTransaction>();
            var i = 0;

            UpdateProgress?.Invoke(string.Format("{0}{1}/{2}", progressText, i, addresses.Count()));

            ServerInfo = await _electrumClient.GetBlockchainHeadersSubscribe();
            if ((ServerInfo != null) && (ServerInfo.Result != null) && (lastSyncedHeight < ServerInfo.Result.BlockHeight))
            {
                foreach (var itemAddress in addresses)
                {
                    i++;
                    UpdateProgress?.Invoke(string.Format("{0}{1}/{2}", progressText, i, addresses.Count()));

                    var address = BitcoinAddress.Create(itemAddress.Address, _net);
                    var addressBytes = address.ScriptPubKey.ToBytes();
                    var P2PKHBytes = cnvHelper.ByteArrayToString(addressBytes);
                    var P2PKH = cnvHelper.StringToByteArray(P2PKHBytes);
                    var hashAddressReverse = cnvHelper.sha256_hash_reverse_bytes(P2PKH);

                    var addressHistory = await _electrumClient.GetBlockchainScripthashGetHistory(hashAddressReverse);
                    if ((addressHistory != null) && (addressHistory.Result != null))
                    {
                        var transactionResult = addressHistory.Result;
                        foreach (var itemTransaction in transactionResult)
                        {
                            if (itemTransaction.Height > lastSyncedHeight)
                            {
                                var itemTxDataResult = await _electrumClient.GetBlockchainTransactionGet(itemTransaction.TxHash);
                                if ((itemTxDataResult != null) && (itemTxDataResult.Result != null))
                                {
                                    itemTxDataResult.Result.Height = itemTransaction.Height;
                                    WalletTransaction newWalletTransation;

                                    if (itemTransaction.Height != 0)
                                    {
                                        var itemTxMerkleResult = await _electrumClient.GetTransactionGetMerkle(itemTransaction.TxHash, itemTransaction.Height);
                                        newWalletTransation = new WalletTransaction(itemAddress, itemTxDataResult.Result, itemTxMerkleResult.Result);
                                    }
                                    else
                                    {
                                        newWalletTransation = new WalletTransaction(itemAddress, itemTxDataResult.Result);
                                    }

                                    transactionList.Add(newWalletTransation);
                                }
                            }
                        }
                    }
                }
            } 
            else
            {
                foreach (var itemAddress in addresses)
                {
                    var address = BitcoinAddress.Create(itemAddress.Address, _net);
                    var addressBytes = address.ScriptPubKey.ToBytes();
                    var P2PKHBytes = cnvHelper.ByteArrayToString(addressBytes);
                    var P2PKH = cnvHelper.StringToByteArray(P2PKHBytes);
                    var hashAddressReverse = cnvHelper.sha256_hash_reverse_bytes(P2PKH);

                    var addressMemPool = await _electrumClient.GetBlockchainScripthashGetMempool(hashAddressReverse);
                    if ((addressMemPool != null) && (addressMemPool.Result != null))
                    {
                        var transactionResult = addressMemPool.Result;
                        foreach (var itemTransaction in transactionResult)
                        {
                            var itemTxDataResult = await _electrumClient.GetBlockchainTransactionGet(itemTransaction.TxHash);
                            if ((itemTxDataResult != null) && (itemTxDataResult.Result != null))
                            {
                                itemTxDataResult.Result.Height = itemTransaction.Height;
                                WalletTransaction newWalletTransation;
                                newWalletTransation = new WalletTransaction(itemAddress, itemTxDataResult.Result);

                                transactionList.Add(newWalletTransation);
                            }
                        }
                    }
                }
            }

            transactionList = transactionList
                .OrderBy(tx => tx.BlockchainTransaction.Height)
                .ThenBy(tx => tx.BlockchainTransaction.Time)
                .ToList();

            return transactionList;
        }

        public async Task<decimal> GetEstimateFee(uint height)
        {
            decimal feeEstimation = 0;
            var fee = await _electrumClient.GetBlockchainEstimateFee(height);
            if ((fee != null) && (fee.Result != null))
            {
                return fee.Result.Value;
            }

            return feeEstimation;
        }

        public async Task<decimal> GetRelayFee()
        {
            decimal relayFeeEstimation = 0;
            var relayFee = await _electrumClient.GetBlockchainRelayFee();
            if ((relayFee != null) && (relayFee.Result != null))
            {
                return relayFee.Result.Value;
            }

            return relayFeeEstimation;
        }

        public async Task<string> TransactionBroadcast(string hex)
        {
            var broadcast = await _electrumClient.BlockchainTransactionBroadcast(hex);
            if ((broadcast != null) && (broadcast.Result != null))
            {
                return broadcast.Result;
            }

            return null;
        }

        public void Dispose()
        {
            _electrumClient.Dispose();
        }
    }
}
