﻿using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WalletProvider.Entities;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Newtonsoft.Json;
using WalletProvider.Utils;

namespace WalletProvider
{
    public class WalletManager
    {
        /// <summary>Size of the buffer of unused addresses maintained in an account. </summary>
        private const int UNUSEDADDRESSESBUFFER = 20;

        /// <summary>Quantity of accounts created in a wallet file when a wallet is created.</summary>
        private const int WALLETCREATIONACCOUNTSCOUNT = 1;

        /// <summary>Default account name </summary>
        public const string DEFAULTACCOUNT = "account 0";

        /// <summary>Provider of time functions.</summary>
        private readonly IDateTimeProvider _dateTimeProvider;

        private Dictionary<OutPoint, TransactionData> _outpointLookup;
        private Dictionary<Script, HdAddress> _keysLookup;

        public WalletMetadata Wallet { get; set; }

        public WalletManager()
        {
            _dateTimeProvider = DateTimeProvider.Default;
        }

        public WalletManager(string serializedWallet)
        {
            _dateTimeProvider = DateTimeProvider.Default;
            _keysLookup = new Dictionary<Script, HdAddress>();
            _outpointLookup = new Dictionary<OutPoint, TransactionData>();

            Wallet = DeserializeWalletMetadata(serializedWallet);
            if (Wallet.Wallet.Network == null) Wallet.Wallet.Network = GetNetwork(Wallet.IsMainNetwork);

            LoadKeysLookupLock();
        }

        public WalletMetadata CreateElectrumWallet(string password, string name, string mnemonicList, string passphrase = null, bool isMainNetwork = true)
        {
            return ImportElectrumWallet(password, name, mnemonicList, passphrase, isMainNetwork);
        }

        public WalletMetadata ImportElectrumWallet(string password, string name, string mnemonicList, string passphrase = null, bool isMainNetwork = true)
        {
            var walletMetadata = new WalletMetadata();
            var network = GetNetwork(isMainNetwork);

            // Generate the root seed used to generate keys from a mnemonic picked at random
            // and a passphrase optionally provided by the user.
            MnemonicElectrum mnemonic = new MnemonicElectrum(mnemonicList);

            ExtKey extendedKey = HdOperations.GetHdPrivateKey(mnemonic, passphrase);

            ////TESTONLY private key
            BitcoinExtKey b58key = network.CreateBitcoinExtKey(extendedKey);

            BitcoinExtPubKey b58pubkey = network.CreateBitcoinExtPubKey(extendedKey.Neuter());

            PubKey pubkey = HdOperations.GeneratePublicKey(extendedKey.Neuter().ToString(network), 0, false);
            BitcoinPubKeyAddress address = pubkey.GetAddress(network);

            ////END-TESTONLY private key

            // Create a wallet file.
            string encryptedSeed = extendedKey.PrivateKey.GetEncryptedBitcoinSecret(password, network).ToWif();
            Wallet wallet = new Wallet
            {
                Name = name,
                EncryptedSeed = encryptedSeed,
                ChainCode = extendedKey.ChainCode,
                CreationTime = _dateTimeProvider.GetTimeOffset(),
                Network = network,
                AccountsRoot = new List<AccountRoot> { new AccountRoot() { Accounts = new List<HdAccount>(), CoinType = (CoinType)network.Consensus.CoinType } },
            };

            // Generate multiple accounts and addresses from the get-go.
            for (int i = 0; i < WALLETCREATIONACCOUNTSCOUNT; i++)
            {
                HdAccount account = wallet.AddNewElectrumAccount(password, (CoinType)network.Consensus.CoinType, _dateTimeProvider.GetTimeOffset());
                account.CreateAddresses(network, UNUSEDADDRESSESBUFFER);
                account.CreateAddresses(network, UNUSEDADDRESSESBUFFER, true);
                UpdateKeysLookupLock(account.GetCombinedAddresses());

                walletMetadata.Account = account;
            }

            walletMetadata.Wallet = wallet;
            walletMetadata.Seed = mnemonic.ToString();

            return walletMetadata;
        }

        public WalletMetadata ImportWebWalletBase64(string password, string name, string mnemonicList, long creationTime, string passphrase = null, bool isMainNetwork = true)
        {
            // For now the passphrase is set to be the password by default.
            if (passphrase == null)
                passphrase = password;

            if (isMainNetwork)
            {
                var creationTimeDate = DateTimeOffset.FromUnixTimeSeconds(creationTime).DateTime;
                var breakDate = DateTimeOffset.FromUnixTimeSeconds(1539810380).DateTime;
                if (creationTimeDate > breakDate) passphrase = Convert.ToBase64String(Encoding.UTF8.GetBytes(passphrase));
            }

            return ImportWallet(password, name, mnemonicList, passphrase);
        }

        public WalletMetadata ImportWallet(string password, string name, string mnemonicList, string passphrase = null, bool isMainNetwork = true)
        {
            var walletMetadata = new WalletMetadata();
            var network = GetNetwork(isMainNetwork);

            Mnemonic mnemonic = new Mnemonic(mnemonicList);

            ExtKey extendedKey = HdOperations.GetHdPrivateKey(mnemonic, passphrase);

            // Create a wallet file.
            string encryptedSeed = extendedKey.PrivateKey.GetEncryptedBitcoinSecret(password, network).ToWif();
            Wallet wallet = new Wallet
            {
                Name = name,
                EncryptedSeed = encryptedSeed,
                ChainCode = extendedKey.ChainCode,
                CreationTime = _dateTimeProvider.GetTimeOffset(),
                Network = network,
                AccountsRoot = new List<AccountRoot> { new AccountRoot() { Accounts = new List<HdAccount>(), CoinType = (CoinType)network.Consensus.CoinType } },
            };

            // Generate multiple accounts and addresses from the get-go.
            for (int i = 0; i < WALLETCREATIONACCOUNTSCOUNT; i++)
            {
                HdAccount account = wallet.AddNewAccount(password, (CoinType)network.Consensus.CoinType, _dateTimeProvider.GetTimeOffset());
                account.CreateAddresses(network, UNUSEDADDRESSESBUFFER);
                account.CreateAddresses(network, UNUSEDADDRESSESBUFFER, true);
                UpdateKeysLookupLock(account.GetCombinedAddresses());

                walletMetadata.Account = account;
            }

            walletMetadata.Wallet = wallet;
            walletMetadata.Seed = mnemonic.ToString();

            return walletMetadata;
        }

        public Network GetNetwork(bool isMainNetwork)
        {
            if (isMainNetwork)
            {
                return Network.XRCMain(false);
            } 
            else
            {
                return Network.XRCTest(false);
            }
        }

        public bool ValidateWalletMetadata(WalletMetadata walletMetadata)
        {
            var isValid = true;

            if (string.IsNullOrEmpty(walletMetadata.Seed))
            {
                isValid = false;
            }

            if (walletMetadata.Wallet == null)
            {
                isValid = false;
            } 
            else
            {
                var coinType = walletMetadata.Wallet.Network.Consensus.CoinType;
                var account = walletMetadata.Wallet.GetAccountByCoinType(DEFAULTACCOUNT, (CoinType)coinType);

                if ((account.InternalAddresses == null) || (account.InternalAddresses.Count == 0))
                {
                    isValid = false;
                }

                if ((account.ExternalAddresses == null) || (account.ExternalAddresses.Count == 0))
                {
                    isValid = false;
                }
            }

            return isValid;
        }

        private byte[] SerializeObject(object value)
        {
            MemoryStream memorystream = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(memorystream, value);
            return memorystream.ToArray();
        }

        private T DeseralizeObject<T>(byte[] value)
        {
            MemoryStream memorystream = new MemoryStream(value);
            BinaryFormatter bf = new BinaryFormatter();
            return (T)bf.Deserialize(memorystream);
        }

        public string SerializeWalletMetadata(WalletMetadata walletMetadata, string userName, string password, bool isMainNetwork)
        {
            var walletSerialized = new WalletMetadataSerialized();
            var cryptography = new InMemoryCryptography();

            walletSerialized.Account = SerializeObject(walletMetadata.Account);
            walletSerialized.Seed = cryptography.Encrypt(walletMetadata.Seed, password);
            walletSerialized.Wallet = SerializeObject(walletMetadata.Wallet);
            walletSerialized.UserName = userName;
            walletSerialized.PasswordEncrypted = cryptography.Encrypt(password, password);
            walletSerialized.IsMainNetwork = isMainNetwork;

            return JsonConvert.SerializeObject(walletSerialized);
        }

        public string SerializeWalletMetadata()
        {
            var walletSerialized = new WalletMetadataSerialized();

            walletSerialized.Account = SerializeObject(Wallet.Account);
            walletSerialized.Seed = Wallet.Seed;
            walletSerialized.Wallet = SerializeObject(Wallet.Wallet);
            walletSerialized.UserName = Wallet.UserName;
            walletSerialized.PasswordEncrypted = Wallet.PasswordEncrypted;
            walletSerialized.IsMainNetwork = Wallet.IsMainNetwork;

            return JsonConvert.SerializeObject(walletSerialized);
        }

        private WalletMetadata DeserializeWalletMetadata(string jsonWalletMetadata)
        {
            var walletMetadata = new WalletMetadata();
            var walletDeserialized = JsonConvert.DeserializeObject<WalletMetadataSerialized>(jsonWalletMetadata);

            walletMetadata.Seed = walletDeserialized.Seed;
            walletMetadata.Account = DeseralizeObject<HdAccount>(walletDeserialized.Account);
            walletMetadata.Wallet = DeseralizeObject<Wallet>(walletDeserialized.Wallet);
            walletMetadata.UserName = walletDeserialized.UserName;
            walletMetadata.PasswordEncrypted = walletDeserialized.PasswordEncrypted;
            walletMetadata.IsMainNetwork = walletDeserialized.IsMainNetwork;

            return walletMetadata;
        }

        public bool IsPasswordUserValid(WalletMetadata walletMetadata, string userName, string password)
        {
            var cryptography = new InMemoryCryptography();

            if ((walletMetadata.UserName.ToLower() == userName.ToLower())
                 && (walletMetadata.PasswordEncrypted == cryptography.Encrypt(password, password)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SyncBlockchainData(List<WalletTransaction> blockchainTransactionData, Network network)
        {
            if ((blockchainTransactionData != null) && (blockchainTransactionData.Any()))
            {
                foreach (var itemTransactionData in blockchainTransactionData)
                {
                    var transaction = Transaction.Load(itemTransactionData.BlockchainTransaction.Hex, network);

                    int? blockHeight = itemTransactionData.BlockchainTransaction.Height;
                    if (blockHeight == 0) blockHeight = null;

                    var blockTime = itemTransactionData.BlockchainTransaction.Blocktime;

                    uint256 blockHash = null;
                    if (!string.IsNullOrEmpty(itemTransactionData.BlockchainTransaction.Blockhash)) blockHash = new uint256(itemTransactionData.BlockchainTransaction.Blockhash);

                    ProcessTransaction(transaction, itemTransactionData.BlockchainTransactionMerkle, blockHeight, blockTime, blockHash, network);
                }
            }         
        }

        private bool ProcessTransaction(
            Transaction transaction,
            List<string> transactionMerkle,
            int? blockHeight, 
            long blockTime, 
            uint256 blockHash,
            Network network, 
            bool isPropagated = true)
        {
            uint256 hash = transaction.GetHash();

            bool foundReceivingTrx = false, foundSendingTrx = false;

            // Check the outputs.
            foreach (TxOut utxo in transaction.Outputs)
            {
                // Check if the outputs contain one of our addresses.
                if (_keysLookup.TryGetValue(utxo.ScriptPubKey, out HdAddress _))
                {
                    AddTransactionToWallet(transaction, transactionMerkle, utxo, blockHeight, blockTime, blockHash, isPropagated);
                    foundReceivingTrx = true;
                }
            }

            // Check the inputs - include those that have a reference to a transaction containing one of our scripts and the same index.
            foreach (TxIn input in transaction.Inputs)
            {
                if (!_outpointLookup.TryGetValue(input.PrevOut, out TransactionData tTx))
                {
                    continue;
                }

                // Get the details of the outputs paid out.
                IEnumerable<TxOut> paidOutTo = transaction.Outputs.Where(o =>
                {
                    // If script is empty ignore it.
                    if (o.IsEmpty)
                        return false;

                    // Check if the destination script is one of the wallet's.
                    bool found = _keysLookup.TryGetValue(o.ScriptPubKey, out HdAddress addr);

                    // Include the keys not included in our wallets (external payees).
                    if (!found)
                        return true;

                    // Include the keys that are in the wallet but that are for receiving
                    // addresses (which would mean the user paid itself).
                    // We also exclude the keys involved in a staking transaction.
                    return !addr.IsChangeAddress();
                });

                AddSpendingTransactionToWallet(transaction, paidOutTo, tTx.Id, tTx.Index, network, blockTime, blockHeight);
                foundSendingTrx = true;
            }

            return foundSendingTrx || foundReceivingTrx;
        }

        private void AddTransactionToWallet(
            Transaction transaction,
            List<string> transactionMerkle,
            TxOut utxo, 
            int? blockHeight, 
            long blockTime, 
            uint256 blockHash, 
            bool isPropagated = true)
        {
            uint256 transactionHash = transaction.GetHash();

            // Get the collection of transactions to add to.
            Script script = utxo.ScriptPubKey;
            _keysLookup.TryGetValue(script, out HdAddress address);
            ICollection<TransactionData> addressTransactions = address.Transactions;

            // Check if a similar UTXO exists or not (same transaction ID and same index).
            // New UTXOs are added, existing ones are updated.
            int index = transaction.Outputs.IndexOf(utxo);
            Money amount = utxo.Value;
            TransactionData foundTransaction = addressTransactions.FirstOrDefault(t => (t.Id == transactionHash) && (t.Index == index));
            if (foundTransaction == null)
            {
                var newTransaction = new TransactionData
                {
                    Amount = amount,
                    BlockHeight = blockHeight,
                    BlockHash = blockHash,
                    Id = transactionHash,
                    CreationTime = DateTimeOffset.FromUnixTimeSeconds(blockTime),
                    Index = index,
                    ScriptPubKey = script,
                    Hex = transaction.ToHex(),
                    IsPropagated = isPropagated,
                    IsCoinbase = transaction.IsCoinBase
                };

                // Add the Merkle proof to the (non-spending) transaction.
                if (blockHeight.HasValue)
                {
                    newTransaction.MerkleProof = GetMerkleTree(transactionMerkle, transactionHash);
                }

                addressTransactions.Add(newTransaction);
                AddInputKeysLookupLock(newTransaction);
            }
            else
            {
                // Update the block height and block hash.
                if ((foundTransaction.BlockHeight == null) && (blockHeight != null))
                {
                    foundTransaction.BlockHeight = blockHeight;
                    foundTransaction.BlockHash = blockHash;
                }

                // Update the block time.
                if (blockHeight.HasValue)
                {
                    foundTransaction.CreationTime = DateTimeOffset.FromUnixTimeSeconds(blockTime);
                }

                // Add the Merkle proof now that the transaction is confirmed in a block.
                if ((blockHeight.HasValue) && (foundTransaction.MerkleProof == null))
                {
                    foundTransaction.MerkleProof = GetMerkleTree(transactionMerkle, transactionHash);
                }

                if (isPropagated)
                {
                    foundTransaction.IsPropagated = true;
                }

                foundTransaction.IsCoinbase = transaction.IsCoinBase;
            }
        }

        private void AddSpendingTransactionToWallet(
            Transaction transaction, 
            IEnumerable<TxOut> paidToOutputs,
            uint256 spendingTransactionId, 
            int? spendingTransactionIndex, 
            Network network, 
            long? blockTime = null, 
            int? blockHeight = null)
        {
            // Get the transaction being spent.
            TransactionData spentTransaction = this._keysLookup.Values.Distinct().SelectMany(v => v.Transactions)
                .SingleOrDefault(t => (t.Id == spendingTransactionId) && (t.Index == spendingTransactionIndex));
            if (spentTransaction == null)
            {
                // Strange, why would it be null?
                return;
            }

            // If the details of this spending transaction are seen for the first time.
            if (spentTransaction.SpendingDetails == null)
            {
                List<PaymentDetails> payments = new List<PaymentDetails>();
                foreach (TxOut paidToOutput in paidToOutputs)
                {
                    // Figure out how to retrieve the destination address.
                    string destinationAddress = string.Empty;
                    ScriptTemplate scriptTemplate = paidToOutput.ScriptPubKey.FindTemplate(network);
                    switch (scriptTemplate.Type)
                    {
                        // Pay to PubKey can be found in outputs of staking transactions.
                        case TxOutType.TX_PUBKEY:
                            PubKey pubKey = PayToPubkeyTemplate.Instance.ExtractScriptPubKeyParameters(paidToOutput.ScriptPubKey);
                            destinationAddress = pubKey.GetAddress(network).ToString();
                            break;
                        // Pay to PubKey hash is the regular, most common type of output.
                        case TxOutType.TX_PUBKEYHASH:
                            destinationAddress = paidToOutput.ScriptPubKey.GetDestinationAddress(network).ToString();
                            break;
                        case TxOutType.TX_NONSTANDARD:
                        case TxOutType.TX_SCRIPTHASH:
                        case TxOutType.TX_MULTISIG:
                        case TxOutType.TX_NULL_DATA:
                            destinationAddress = paidToOutput.ScriptPubKey.GetDestinationAddress(network)?.ToString();
                            break;
                    }

                    payments.Add(new PaymentDetails
                    {
                        DestinationScriptPubKey = paidToOutput.ScriptPubKey,
                        DestinationAddress = destinationAddress,
                        Amount = paidToOutput.Value
                    });
                }

                SpendingDetails spendingDetails = new SpendingDetails
                {
                    TransactionId = transaction.GetHash(),
                    Payments = payments,
                    CreationTime = DateTimeOffset.FromUnixTimeSeconds(blockTime ?? transaction.Time),
                    BlockHeight = blockHeight,
                    Hex = transaction.ToHex()
                };

                spentTransaction.SpendingDetails = spendingDetails;
                spentTransaction.MerkleProof = null;
            }
            else // If this spending transaction is being confirmed in a block.
            {
                // Update the block height.
                if (spentTransaction.SpendingDetails.BlockHeight == null && blockHeight != null)
                {
                    spentTransaction.SpendingDetails.BlockHeight = blockHeight;
                }

                // Update the block time to be that of the block in which the transaction is confirmed.
                if (blockHeight.HasValue)
                {
                    spentTransaction.SpendingDetails.CreationTime = DateTimeOffset.FromUnixTimeSeconds(blockTime.Value);
                }
            }
        }

        public void LoadKeysLookupLock()
        {
            IEnumerable<HdAddress> addresses = GetCombinedAddresses();
            foreach (HdAddress address in addresses)
            {
                _keysLookup[address.ScriptPubKey] = address;
                if (address.Pubkey != null)
                {
                    _keysLookup[address.Pubkey] = address;
                }

                foreach (var transaction in address.Transactions)
                {
                    _outpointLookup[new OutPoint(transaction.Id, transaction.Index)] = transaction;
                }
            }
        }

        private void UpdateKeysLookupLock(IEnumerable<HdAddress> addresses)
        {
            if (addresses == null || !addresses.Any())
            {
                return;
            }

            foreach (HdAddress address in addresses)
            {
                _keysLookup[address.ScriptPubKey] = address;
                if (address.Pubkey != null)
                {
                    _keysLookup[address.Pubkey] = address;
                }
            }
        }

        private void AddInputKeysLookupLock(TransactionData transactionData)
        {
            _outpointLookup[new OutPoint(transactionData.Id, transactionData.Index)] = transactionData;
        }

        public IEnumerable<HdAddress> GetCombinedAddresses()
        {
            IEnumerable<HdAddress> addresses = new List<HdAddress>();

            if (Wallet != null)
            {
                var coinType = Wallet.Wallet.Network.Consensus.CoinType;
                var account = Wallet.Wallet.GetAccountByCoinType(DEFAULTACCOUNT, (CoinType)coinType);

                if (account.ExternalAddresses != null)
                {
                    addresses = account.ExternalAddresses;
                }

                if (account.InternalAddresses != null)
                {
                    addresses = addresses.Concat(account.InternalAddresses);
                }
            }

            return addresses;
        }

        public WalletBalance GetWalletBalance(int lastBlockHeight, Network network)
        {
            (Money amountConfirmed, Money amountUnconfirmed, Money amountImmature) result = 
                Wallet.Account.GetSpendableAmount(lastBlockHeight, network);

            return new WalletBalance
            {
                AmountConfirmed = result.amountConfirmed,
                AmountUnconfirmed = result.amountUnconfirmed,
                AmountImmature = result.amountImmature
            };
        }

        public List<WalletTransaction> GetWalletHistory()
        {
            var transactions = new List<WalletTransaction>();

            transactions = GetCombinedAddresses()
                .Where(a => a.Transactions.Any())
                .SelectMany(s => s.Transactions.Select(t => new WalletTransaction(s, t))).ToList();

            return transactions;
        }

        private PartialMerkleTree GetMerkleTree(List<string> transactionHashes, uint256 transactionHash)
        {
            List<bool> vMatch = new List<bool>();
            List<uint256> vHashes = new List<uint256>();

            if ((transactionHashes != null) && (transactionHashes.Any()))
            {
                foreach (var itemHash in transactionHashes)
                {
                    var uint256hash = new uint256(itemHash);

                    vHashes.Add(uint256hash);
                    vMatch.Add(uint256hash.Equals(transactionHash));
                }
            }

            return new PartialMerkleTree(vHashes.ToArray(), vMatch.ToArray());
        }
    }
}
