using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WalletProvider.Entities;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Newtonsoft.Json;
using WalletProvider.Utils;
using System.Threading.Tasks;

namespace WalletProvider
{
    public class WalletManager
    {
        /// <summary>Size of the buffer of unused addresses maintained in an account. </summary>
        private const int UNUSEDADDRESSESBUFFER = 20;

        /// <summary>Quantity of accounts created in a wallet file when a wallet is created.</summary>
        private const int WALLETCREATIONACCOUNTSCOUNT = 1;

        /// <summary>Default account name </summary>
        private const string DEFAULTACCOUNT = "account 0";

        /// <summary>Provider of time functions.</summary>
        private readonly IDateTimeProvider _dateTimeProvider;

        public WalletMetadata Wallet { get; set; }

        public WalletManager()
        {
            _dateTimeProvider = DateTimeProvider.Default;
        }

        public WalletManager(string serializedWallet)
        {
            _dateTimeProvider = DateTimeProvider.Default;
            Wallet = DeserializeWalletMetadata(serializedWallet);
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
                IEnumerable<HdAddress> newReceivingAddresses = account.CreateAddresses(network, UNUSEDADDRESSESBUFFER);
                IEnumerable<HdAddress> newChangeAddresses = account.CreateAddresses(network, UNUSEDADDRESSESBUFFER, true);

                walletMetadata.Account = account;
                walletMetadata.ReceivingAddresses = newReceivingAddresses.ToList();
                walletMetadata.ChangeAddresses = newChangeAddresses.ToList();
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
                IEnumerable<HdAddress> newReceivingAddresses = account.CreateAddresses(network, UNUSEDADDRESSESBUFFER);
                IEnumerable<HdAddress> newChangeAddresses = account.CreateAddresses(network, UNUSEDADDRESSESBUFFER, true);

                walletMetadata.Account = account;
                walletMetadata.ReceivingAddresses = newReceivingAddresses.ToList();
                walletMetadata.ChangeAddresses = newChangeAddresses.ToList();
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

            if ((walletMetadata.ChangeAddresses == null) || (walletMetadata.ChangeAddresses.Count == 0))
            {
                isValid = false;
            }

            if ((walletMetadata.ReceivingAddresses == null) || (walletMetadata.ReceivingAddresses.Count == 0))
            {
                isValid = false;
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

        private List<byte[]> SerializeListOfObjects(List<HdAddress> value)
        {
            var listOfObjects = new List<byte[]>();

            foreach (var item in value)
            {
                listOfObjects.Add(SerializeObject(item));
            }

            return listOfObjects;
        }

        private List<T> DeserializeListOfObjects<T>(List<byte[]> value)
        {
            var listOfObjects = new List<T>();

            foreach (var item in value)
            {
                listOfObjects.Add(DeseralizeObject<T>(item));
            }

            return listOfObjects;
        }

        public string SerializeWalletMetadata(WalletMetadata walletMetadata, string userName, string password, bool isMainNetwork)
        {
            var walletSerialized = new WalletMetadataSerialized();
            var cryptography = new InMemoryCryptography();

            walletSerialized.Account = SerializeObject(walletMetadata.Account);
            walletSerialized.Seed = cryptography.Encrypt(walletMetadata.Seed, password);
            walletSerialized.Wallet = SerializeObject(walletMetadata.Wallet);
            walletSerialized.ReceivingAddresses = SerializeListOfObjects(walletMetadata.ReceivingAddresses);
            walletSerialized.ChangeAddresses = SerializeListOfObjects(walletMetadata.ChangeAddresses);
            walletSerialized.UserName = userName;
            walletSerialized.PasswordEncrypted = cryptography.Encrypt(password, password);
            walletSerialized.IsMainNetwork = isMainNetwork;

            return JsonConvert.SerializeObject(walletSerialized);
        }

        private WalletMetadata DeserializeWalletMetadata(string jsonWalletMetadata)
        {
            var walletMetadata = new WalletMetadata();
            var walletDeserialized = JsonConvert.DeserializeObject<WalletMetadataSerialized>(jsonWalletMetadata);

            walletMetadata.Seed = walletDeserialized.Seed;
            walletMetadata.Account = DeseralizeObject<HdAccount>(walletDeserialized.Account);
            walletMetadata.Wallet = DeseralizeObject<Wallet>(walletDeserialized.Wallet);
            walletMetadata.ReceivingAddresses = DeserializeListOfObjects<HdAddress>(walletDeserialized.ReceivingAddresses);
            walletMetadata.ChangeAddresses = DeserializeListOfObjects<HdAddress>(walletDeserialized.ChangeAddresses);
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

        public void SyncBlockchainData(List<WalletTransaction> blockchainTransactionData)
        {
            //throw new NotImplementedException();

          
        }

        //private void AddTransactionToWallet(Transaction transaction, TxOut utxo, int? blockHeight = null, string blockHash)
        //{
        //    uint256 transactionHash = transaction.GetHash();

        //    // Get the collection of transactions to add to.
        //    Script script = utxo.ScriptPubKey;
        //    ICollection<TransactionData> addressTransactions = address.Transactions;

        //    // Check if a similar UTXO exists or not (same transaction ID and same index).
        //    // New UTXOs are added, existing ones are updated.
        //    int index = transaction.Outputs.IndexOf(utxo);
        //    Money amount = utxo.Value;
        //    TransactionData foundTransaction = addressTransactions.FirstOrDefault(t => (t.Id == transactionHash) && (t.Index == index));
        //    if (foundTransaction == null)
        //    {
        //        var newTransaction = new TransactionData
        //        {
        //            Amount = amount,
        //            BlockHeight = blockHeight,
        //            BlockHash = new uint256(blockHash),
        //            Id = transactionHash,
        //            CreationTime = DateTimeOffset.FromUnixTimeSeconds(block?.Header.Time ?? transaction.Time),
        //            Index = index,
        //            ScriptPubKey = script,
        //            Hex = transaction.ToHex(),
        //            IsPropagated = true,
        //            IsCoinbase = transaction.IsCoinBase
        //        };

        //        addressTransactions.Add(newTransaction);
        //        this.AddInputKeysLookupLock(newTransaction);
        //    }
        //    else
        //    {
        //        // Update the block height and block hash.
        //        if ((foundTransaction.BlockHeight == null) && (blockHeight != null))
        //        {
        //            foundTransaction.BlockHeight = blockHeight;
        //            foundTransaction.BlockHash = block?.GetHash();
        //        }

        //        // Update the block time.
        //        if (block != null)
        //        {
        //            foundTransaction.CreationTime = DateTimeOffset.FromUnixTimeSeconds(block.Header.Time);
        //        }

        //        // Add the Merkle proof now that the transaction is confirmed in a block.
        //        if ((block != null) && (foundTransaction.MerkleProof == null))
        //        {
        //            foundTransaction.MerkleProof = new MerkleBlock(block, new[] { transactionHash }).PartialMerkleTree;
        //        }

        //        if (isPropagated)
        //        {
        //            foundTransaction.IsPropagated = true;
        //        }

        //        foundTransaction.IsCoinbase = transaction.IsCoinBase;
        //    }

        //    this.TransactionFoundInternal(script);
        //    this.logger.LogTrace("(-)");
        //}
    }
}
