﻿using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WalletProvider.Entities;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace WalletProvider
{
    public class WalletManager
    {
        // <summary>As per RPC method definition this should be the max allowable expiry duration.</summary>
        private const int MaxWalletUnlockDurationInSeconds = 60 * 60 * 5;

        /// <summary>Size of the buffer of unused addresses maintained in an account. </summary>
        private const int UnusedAddressesBuffer = 20;

        /// <summary>Quantity of accounts created in a wallet file when a wallet is restored.</summary>
        private const int WalletRecoveryAccountsCount = 1;

        /// <summary>Quantity of accounts created in a wallet file when a wallet is created.</summary>
        private const int WalletCreationAccountsCount = 1;

        /// <summary>Timer for saving wallet files to the file system.</summary>
        private const int WalletSavetimeIntervalInMinutes = 5;

        /// <summary>Default account name </summary>
        private const string DefaultAccount = "account 0";

        /// <summary>Provider of time functions.</summary>
        private readonly IDateTimeProvider dateTimeProvider;

        public WalletManager()
        {
            dateTimeProvider = DateTimeProvider.Default;
        }

        public MnemonicElectrum ImportElectrumWallet(string password, string name, string mnemonicList, string passphrase = null)
        {
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
            for (int i = 0; i < WalletCreationAccountsCount; i++)
            {
                HdAccount account = wallet.AddNewElectrumAccount(password, (CoinType)network.Consensus.CoinType, dateTimeProvider.GetTimeOffset());
                IEnumerable<HdAddress> newReceivingAddresses = account.CreateAddresses(network, UnusedAddressesBuffer);
                IEnumerable<HdAddress> newChangeAddresses = account.CreateAddresses(network, UnusedAddressesBuffer, true);
            }

            return mnemonic;
        }

        public Mnemonic ImportWebWalletBase64(string password, string name, string mnemonicList, long creationTime, string passphrase = null)
        {
            // For now the passphrase is set to be the password by default.
            if (passphrase == null)
                passphrase = password;

            var creationTimeDate = DateTimeOffset.FromUnixTimeSeconds(creationTime).DateTime;
            var breakDate = DateTimeOffset.FromUnixTimeSeconds(1539810380).DateTime;
            if (creationTimeDate > breakDate) passphrase = Convert.ToBase64String(Encoding.UTF8.GetBytes(passphrase));
            
            return ImportWallet(password, name, mnemonicList, passphrase);
        }

        public Mnemonic ImportWallet(string password, string name, string mnemonicList, string passphrase = null)
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
            for (int i = 0; i < WalletCreationAccountsCount; i++)
            {
                HdAccount account = wallet.AddNewAccount(password, (CoinType)network.Consensus.CoinType, dateTimeProvider.GetTimeOffset());
                IEnumerable<HdAddress> newReceivingAddresses = account.CreateAddresses(network, UnusedAddressesBuffer);
                IEnumerable<HdAddress> newChangeAddresses = account.CreateAddresses(network, UnusedAddressesBuffer, true);
            }

            return mnemonic;
        }

        private byte[] SerializeWallet(object wallet)
        {
            MemoryStream memorystream = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(memorystream, wallet);
            return memorystream.ToArray();
        }
    }
}
