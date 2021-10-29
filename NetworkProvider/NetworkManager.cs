﻿using ElectrumXClient;
using ElectrumXClient.Request;
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

        public NetworkManager(string server, int port, Network net)
        {
            _server = server;
            _port = port;
            _net = net;
            _electrumClient = new Client(_server, _port, true);
        }

        public async Task<List<WalletTransaction>> StartSyncingAsync(WalletMetadata wallet)
        {
            var cnvHelper = new ConversionHelper();
            var transactionList = new List<WalletTransaction>();

            foreach (var itemAddress in wallet.ReceivingAddresses)
            {
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
                        var itemTxDataResult = await _electrumClient.GetBlockchainTransactionGet(itemTransaction.TxHash);
                        if ((itemTxDataResult != null) && (itemTxDataResult.Result != null))
                        {
                            var newWalletTransation = new WalletTransaction();
                            newWalletTransation.Address = itemAddress;
                            newWalletTransation.Transaction = itemTxDataResult.Result;

                            transactionList.Add(newWalletTransation);
                        }
                    }
                }
            }

            transactionList = transactionList
                .OrderBy(tx => tx.Transaction.Height)
                .ThenBy(tx => tx.Transaction.Time)
                .ToList();

            return transactionList;
        }

        public void Dispose()
        {
            _electrumClient.Dispose();
        }
    }
}