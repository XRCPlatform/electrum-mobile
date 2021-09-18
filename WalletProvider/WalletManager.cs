using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WalletProvider.Entities;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Newtonsoft.Json;

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
        private readonly IDateTimeProvider dateTimeProvider;

        public WalletManager()
        {
            dateTimeProvider = DateTimeProvider.Default;
        }

        public WalletMetadata CreateElectrumWallet(string password, string name, string mnemonicList, string passphrase = null)
        {
            return ImportElectrumWallet(password, name, mnemonicList, passphrase);
        }

        public WalletMetadata ImportElectrumWallet(string password, string name, string mnemonicList, string passphrase = null)
        {
            var walletMetadata = new WalletMetadata();
            var network = Network.XRCTest(true);

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
                CreationTime = dateTimeProvider.GetTimeOffset(),
                Network = network,
                AccountsRoot = new List<AccountRoot> { new AccountRoot() { Accounts = new List<HdAccount>(), CoinType = (CoinType)network.Consensus.CoinType } },
            };

            // Generate multiple accounts and addresses from the get-go.
            for (int i = 0; i < WALLETCREATIONACCOUNTSCOUNT; i++)
            {
                HdAccount account = wallet.AddNewElectrumAccount(password, (CoinType)network.Consensus.CoinType, dateTimeProvider.GetTimeOffset());
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

        public WalletMetadata ImportWebWalletBase64(string password, string name, string mnemonicList, long creationTime, string passphrase = null)
        {
            // For now the passphrase is set to be the password by default.
            if (passphrase == null)
                passphrase = password;

            var creationTimeDate = DateTimeOffset.FromUnixTimeSeconds(creationTime).DateTime;
            var breakDate = DateTimeOffset.FromUnixTimeSeconds(1539810380).DateTime;
            if (creationTimeDate > breakDate) passphrase = Convert.ToBase64String(Encoding.UTF8.GetBytes(passphrase));

            return ImportWallet(password, name, mnemonicList, passphrase);
        }

        public WalletMetadata ImportWallet(string password, string name, string mnemonicList, string passphrase = null)
        {
            var walletMetadata = new WalletMetadata();
            var network = Network.XRCTest(false);

            Mnemonic mnemonic = new Mnemonic(mnemonicList);

            ExtKey extendedKey = HdOperations.GetHdPrivateKey(mnemonic, passphrase);

            // Create a wallet file.
            string encryptedSeed = extendedKey.PrivateKey.GetEncryptedBitcoinSecret(password, network).ToWif();
            Wallet wallet = new Wallet
            {
                Name = name,
                EncryptedSeed = encryptedSeed,
                ChainCode = extendedKey.ChainCode,
                CreationTime = dateTimeProvider.GetTimeOffset(),
                Network = network,
                AccountsRoot = new List<AccountRoot> { new AccountRoot() { Accounts = new List<HdAccount>(), CoinType = (CoinType)network.Consensus.CoinType } },
            };

            // Generate multiple accounts and addresses from the get-go.
            for (int i = 0; i < WALLETCREATIONACCOUNTSCOUNT; i++)
            {
                HdAccount account = wallet.AddNewAccount(password, (CoinType)network.Consensus.CoinType, dateTimeProvider.GetTimeOffset());
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

        public string SerializeWalletMetadata(WalletMetadata walletMetadata)
        {
            var walletSerialized = new WalletMetadataSerialized();

            walletSerialized.Account = SerializeObject(walletMetadata.Account);
            walletSerialized.Seed = walletMetadata.Seed;
            walletSerialized.Wallet = SerializeObject(walletMetadata.Wallet);
            walletSerialized.ReceivingAddresses = SerializeListOfObjects(walletMetadata.ReceivingAddresses);
            walletSerialized.ChangeAddresses = SerializeListOfObjects(walletMetadata.ChangeAddresses);

            return JsonConvert.SerializeObject(walletSerialized);
        }

        public WalletMetadata DeserializeWalletMetadata(string jsonWalletMetadata)
        {
            var walletMetadata = new WalletMetadata();
            var walletSerialized = JsonConvert.DeserializeObject<WalletMetadataSerialized>(jsonWalletMetadata);

            walletMetadata.Seed = walletSerialized.Seed;
            walletMetadata.Account = DeseralizeObject<HdAccount>(walletSerialized.Account);
            walletMetadata.Wallet = DeseralizeObject<Wallet>(walletSerialized.Wallet);
            walletMetadata.ReceivingAddresses = DeserializeListOfObjects<HdAddress>(walletSerialized.ReceivingAddresses);
            walletMetadata.ChangeAddresses = DeserializeListOfObjects<HdAddress>(walletSerialized.ChangeAddresses);

            return walletMetadata;
        }
    }
}
