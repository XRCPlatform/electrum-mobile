using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.DataEncoders;
using NBitcoin.Protocol;

namespace NBitcoin
{
    public enum CoinType
    {
        /// <summary>
        /// XRC
        /// </summary>
        XRC = 10291,

        /// <summary>
        /// Testnet (all coins)
        /// </summary>
        Testnet = 1,

        /// <summary>
        /// RegTest
        /// </summary>
        RegTest = 1
    }

    public partial class Network
    {
        /// <summary> Bitcoin maximal value for the calculated time offset. If the value is over this limit, the time syncing feature will be switched off. </summary>
        public const int BitcoinMaxTimeOffsetSeconds = 70 * 60;

        /// <summary> XRC maximal value for the calculated time offset. If the value is over this limit, the time syncing feature will be switched off. </summary>
        public const int XRCMaxTimeOffsetSeconds = 25 * 60;

        /// <summary> Bitcoin default value for the maximum tip age in seconds to consider the node in initial block download (24 hours). </summary>
        public const int BitcoinDefaultMaxTipAgeInSeconds = 24 * 60 * 60;

        /// <summary> XRC default value for the maximum tip age in seconds to consider the node in initial block download (2 hours). </summary>
        public const int XRCDefaultMaxTipAgeInSeconds = 2 * 60 * 60;

        /// <summary> The name of the root folder containing the different XRC blockchains (XRCMain, XRCTest, XRCRegTest). </summary>
        public const string XRCRootFolderName = "XRC";

        /// <summary> The default name used for the XRC configuration file. </summary>
        public const string XRCDefaultConfigFilename = "xrc.conf";

        /// <summary>
        /// Default name for base network
        /// </summary>
        public const string XRCBaseName = "XRCMain";

        public static Network Main => Network.GetNetwork(XRCBaseName) ?? InitXRCMain();

        public static Network TestNet => Network.GetNetwork("XRCTest") ?? InitXRCTest();

        public static Network RegTest => Network.GetNetwork("XRCRegTest") ?? InitXRCRegTest();

        public static Network XRCMain(bool isElectrum = false) => Network.GetNetwork(XRCBaseName) ?? InitXRCMain(isElectrum);

        public static Network XRCTest(bool isElectrum = false) => Network.GetNetwork("XRCTest") ?? InitXRCTest(isElectrum);

        public static Network XRCRegTest => Network.GetNetwork("XRCRegTest") ?? InitXRCRegTest();

        private static Network InitXRCMain(bool isElectrum = false)
        {
            var messageStart = new byte[4];
            messageStart[0] = 0x33;
            messageStart[1] = 0x33;
            messageStart[2] = 0x34;
            messageStart[3] = 0x35;
            var magic = BitConverter.ToUInt32(messageStart, 0);

            Network network = new Network
            {
                Name = "XRCMain",
                RootFolderName = XRCRootFolderName,
                DefaultConfigFilename = XRCDefaultConfigFilename,
                Magic = magic,
                DefaultPort = 37270,
                RPCPort = 19660,
                MaxTimeOffsetSeconds = XRCMaxTimeOffsetSeconds,
                MaxTipAge = 604800, //one week
                MinTxFee = 1000,
                FallbackFee = 20000,
                MinRelayTxFee = 1000
            };

            network.Consensus.SubsidyHalvingInterval = 210000;
            network.Consensus.MajorityEnforceBlockUpgrade = 750;
            network.Consensus.MajorityRejectBlockOutdated = 950;
            network.Consensus.MajorityWindow = 1000;
            network.Consensus.BuriedDeployments[BuriedDeployments.BIP34] = 0;
            network.Consensus.BuriedDeployments[BuriedDeployments.BIP65] = 0;
            network.Consensus.BuriedDeployments[BuriedDeployments.BIP66] = 0;
            network.Consensus.BIP34Hash = new uint256("0x000000000000024b89b42a942fe0d9fea3bb44ab7bd1b19115dd6a759c0808b8");
            network.Consensus.PowLimit = new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"));
            network.Consensus.PowLimit2 = new Target(new uint256("0000000000092489000000000000000000000000000000000000000000000000"));
            network.Consensus.PowLimit2Height = 1648;

            network.Consensus.PowTargetTimespan = TimeSpan.FromSeconds(14 * 24 * 60 * 60); // two weeks
            network.Consensus.PowTargetSpacing = TimeSpan.FromSeconds(10 * 60);
            network.Consensus.PowAllowMinDifficultyBlocks = false;
            network.Consensus.PowNoRetargeting = false;
            network.Consensus.RuleChangeActivationThreshold = 1916; // 95% of 2016
            network.Consensus.MinerConfirmationWindow = 2016; // nPowTargetTimespan / nPowTargetSpacing
            network.Consensus.CoinType = (int)CoinType.XRC;
            network.Consensus.DefaultAssumeValid = null; // turn off assumevalid for regtest.
            network.Consensus.ConsensusFactory = new ConsensusFactory() { Consensus = network.Consensus};

            network.Checkpoints.Add(17,new CheckpointInfo(new uint256("2430c4151e10cdc5ccbdea56b909c7c37ab2a852d3e7fb908e0a32493e2ac706")));
            network.Checkpoints.Add(117, new CheckpointInfo(new uint256("bf3082be3b2da88187ebeb902548b41dbff3bcac6687352e0c47d902acd28e62")));
            network.Checkpoints.Add(400, new CheckpointInfo(new uint256("20cb04127f12c1ae7a04ee6dc4c7e36f4c85ee2038c92126b3fd537110d96595")));
            network.Checkpoints.Add(800, new CheckpointInfo(new uint256("df37ca401ecccfc6dedf68ab76a7161496ad93d47c2a474075efb3220e3f3526")));
            network.Checkpoints.Add(26800, new CheckpointInfo(new uint256("c4efd4b6fa294fd72ab6f614dd6705eea43d0a83cd03d597c3214eaaf857a4b6")));
            network.Checkpoints.Add(43034, new CheckpointInfo(new uint256("4df06bd483d2c4ccde5cd1efe3b2ea7d969c41e5923a74c2bba1656a41fc6891")));
            network.Checkpoints.Add(110000, new CheckpointInfo(new uint256("d1d1282681f20223a281393528e6c624539e60177ecb42ab4512555974ac7775")));

            var pubKeyMain = "04ffff0f1e01041a52656c6561736520746865204b72616b656e212121205a657573";
            Block genesis = CreateXRCGenesisBlock(network.Consensus.ConsensusFactory, 1512043200, 0, network.Consensus.PowLimit.ToCompact(), 45, network, pubKeyMain);
            network.genesis = genesis;
            network.Consensus.HashGenesisBlock = genesis.GetHash(network);

            network.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (61) };
            network.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (123) };
            network.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (100) };
            network.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x42 };
            network.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x43 };
            network.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xB2), (0x1E) };
            network.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xAD), (0xE4) };
            network.Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
            network.Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };
            network.Base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x2a };
            network.Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 23 };
            network.Base58Prefixes[(int)Base58Type.COLORED_ADDRESS] = new byte[] { 0x13 };

            if (isElectrum)
            {
                network.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xb2), (0x1e) };
                network.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xad), (0xe4) };
            }

            var encoder = new Bech32Encoder("rh");
            network.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            network.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;

            network.DNSSeeds.AddRange(new[]
            {
                new DNSSeedData("dns.btrmine.com", "dns.btrmine.com"),
                new DNSSeedData("dns2.btrmine.com", "dns2.btrmine.com"),
                new DNSSeedData("xrc.dnsseed.ekcdd.com", "xrc.dnsseed.ekcdd.com")
            });

            Network.Register(network);

            return network;
        }

        private static Network InitXRCTest(bool isElectrum = false)
        {
            var messageStart = new byte[4];
            messageStart[0] = 0x39;
            messageStart[1] = 0x33;
            messageStart[2] = 0x34;
            messageStart[3] = 0x35;
            var magic = BitConverter.ToUInt32(messageStart, 0); // 0xefc0f2cd

            Network network = new Network
            {
                Name = "XRCTest",
                RootFolderName = XRCRootFolderName,
                DefaultConfigFilename = XRCDefaultConfigFilename,
                Magic = magic,
                DefaultPort = 16665,
                RPCPort = 16661,
                MaxTimeOffsetSeconds = XRCMaxTimeOffsetSeconds,
                MaxTipAge = 604800, //one week
                MinTxFee = 10000,
                FallbackFee = 60000,
                MinRelayTxFee = 10000
            };

            network.Consensus.SubsidyHalvingInterval = 210000;
            network.Consensus.MajorityEnforceBlockUpgrade = 750;
            network.Consensus.MajorityRejectBlockOutdated = 950;
            network.Consensus.MajorityWindow = 1000;
            network.Consensus.BuriedDeployments[BuriedDeployments.BIP34] = 0;
            network.Consensus.BuriedDeployments[BuriedDeployments.BIP65] = 0;
            network.Consensus.BuriedDeployments[BuriedDeployments.BIP66] = 0;
            network.Consensus.BIP34Hash = new uint256("0x000000000000024b89b42a942fe0d9fea3bb44ab7bd1b19115dd6a759c0808b8");
            network.Consensus.PowLimit = new Target(uint256.Parse("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"));
            network.Consensus.PowLimit2 = network.Consensus.PowLimit;
            network.Consensus.PowTargetTimespan = TimeSpan.FromSeconds(14 * 24 * 60 * 60); // two weeks
            network.Consensus.PowTargetSpacing = TimeSpan.FromSeconds(10 * 60);
            network.Consensus.PowAllowMinDifficultyBlocks = false;
            network.Consensus.PowNoRetargeting = false;
            network.Consensus.RuleChangeActivationThreshold = 1916; // 95% of 2016
            network.Consensus.MinerConfirmationWindow = 2016; // nPowTargetTimespan / nPowTargetSpacing
            network.Consensus.CoinType = (int)CoinType.Testnet;
            network.Consensus.DefaultAssumeValid = null; // turn off assumevalid for regtest.
            network.Consensus.ConsensusFactory = new ConsensusFactory() { Consensus = network.Consensus };

            var prodTEST = "04ffff0f1e01041a52656c6561736520746865204b72616b656e212121205a657573";
            Block genesis = CreateXRCGenesisBlock(network.Consensus.ConsensusFactory, 1527811200, 0, network.Consensus.PowLimit.ToCompact(), 45, network, prodTEST);
            genesis.Header.Bits = network.Consensus.PowLimit;
            network.genesis = genesis;
            network.Consensus.HashGenesisBlock = genesis.GetHash(network);

            network.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (65) };
            network.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (128) };
            network.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (0xef) };
            network.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x42 };
            network.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x43 };
            network.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xB2), (0x1E) };
            network.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xAD), (0xE4) };
            network.Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
            network.Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };
            network.Base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x2a };
            network.Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 23 };
            network.Base58Prefixes[(int)Base58Type.COLORED_ADDRESS] = new byte[] { 0x13 };

            if (isElectrum)
            {
                network.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x35), (0x87), (0xCF) };
                network.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x35), (0x83), (0x94) };
            }

            var encoder = new Bech32Encoder("th");
            network.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            network.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;

            Network.Register(network);

            return network;
        }

        private static Network InitXRCRegTest()
        {
            var messageStart = new byte[4];
            messageStart[0] = 0x34;
            messageStart[1] = 0x33;
            messageStart[2] = 0x34;
            messageStart[3] = 0x35;
            var magic = BitConverter.ToUInt32(messageStart, 0); // 0xefc0f2cd

            Network network = new Network
            {
                Name = "XRCRegTest",
                RootFolderName = XRCRootFolderName,
                DefaultConfigFilename = XRCDefaultConfigFilename,
                Magic = magic,
                DefaultPort = 16665,
                RPCPort = 16661,
                MaxTimeOffsetSeconds = XRCMaxTimeOffsetSeconds,
                MaxTipAge = XRCDefaultMaxTipAgeInSeconds
            };

            network.Consensus.SubsidyHalvingInterval = 210000;
            network.Consensus.MajorityEnforceBlockUpgrade = 750;
            network.Consensus.MajorityRejectBlockOutdated = 950;
            network.Consensus.MajorityWindow = 1000;
            network.Consensus.BuriedDeployments[BuriedDeployments.BIP34] = 0;
            network.Consensus.BuriedDeployments[BuriedDeployments.BIP65] = 0;
            network.Consensus.BuriedDeployments[BuriedDeployments.BIP66] = 0;
            network.Consensus.BIP34Hash = new uint256("0x000000000000024b89b42a942fe0d9fea3bb44ab7bd1b19115dd6a759c0808b8");
            network.Consensus.PowLimit = new Target(uint256.Parse("7fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")); //0.00000000046565418188
            network.Consensus.PowLimit2 = network.Consensus.PowLimit;
            network.Consensus.PowTargetTimespan = TimeSpan.FromSeconds(14 * 24 * 60 * 60); // two weeks
            network.Consensus.PowTargetSpacing = TimeSpan.FromSeconds(10 * 60);
            network.Consensus.PowAllowMinDifficultyBlocks = true;
            network.Consensus.PowNoRetargeting = false;
            network.Consensus.RuleChangeActivationThreshold = 1916; // 95% of 2016
            network.Consensus.MinerConfirmationWindow = 2016; // nPowTargetTimespan / nPowTargetSpacing
            network.Consensus.CoinType = (int)CoinType.RegTest;
            network.Consensus.DefaultAssumeValid = null; // turn off assumevalid for regtest.
            network.Consensus.ConsensusFactory = new ConsensusFactory() { Consensus = network.Consensus };

            Block genesis = CreateXRCGenesisBlock(network.Consensus.ConsensusFactory, 1527811200, 0, network.Consensus.PowLimit.ToCompact(), 45, network);
            genesis.Header.Bits = network.Consensus.PowLimit;
            network.genesis = genesis;
            network.Consensus.HashGenesisBlock = genesis.GetHash(network);

            network.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (65) };
            network.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (128) };
            network.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (100) };
            network.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x42 };
            network.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x43 };
            network.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xB2), (0x1E) };
            network.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xAD), (0xE4) };
            network.Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
            network.Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };
            network.Base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x2a };
            network.Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 23 };
            network.Base58Prefixes[(int)Base58Type.COLORED_ADDRESS] = new byte[] { 0x13 };

            var encoder = new Bech32Encoder("th");
            network.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = encoder;
            network.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = encoder;

            Network.Register(network);

            return network;
        }

        private static Block CreateXRCGenesisBlock(ConsensusFactory consensusFactory, uint nTime, uint nNonce, uint nBits, int nVersion, Network network, string hexNew = null)
        {
            string message = "Release the Kraken!!! Zeus";
            return CreateXRCGenesisBlock(consensusFactory, message, nTime, nNonce, nBits, nVersion, network, hexNew);
        }

        private static Block CreateXRCGenesisBlock(ConsensusFactory consensusFactory, string message, uint nTime, uint nNonce, uint nBits, int nVersion, Network network, string pubKeyHexNew = null)
        {
            //nTime = 1512043200 => Thursday, November 30, 2017 12:00:00 PM (born XRC)
            //nTime = 1527811200 => Friday, Jun 1, 2017 12:00:00 PM (born TestXRC)
            //nBits = 0x1d00ffff (it is exactly 0x1b = 27 bytes long) => 0x00ffff0000000000000000000000000000000000000000000000000000 => 1
            //nNonce = XTimes to trying to find a genesis block
            var pubKeyHex = "2103d1b6cd5f956ccedf5877c89843a438bfb800468133fb2e73946e1452461a9b1aac";
            if (pubKeyHexNew != null) pubKeyHex = pubKeyHexNew;

            Transaction txNew = consensusFactory.CreateTransaction();
            txNew.Version = 2;
            txNew.Time = nTime;
            txNew.AddInput(new TxIn()
            {
                ScriptSig = new Script(Op.GetPushOp(nBits), new Op()
                {
                    Code = (OpcodeType)0x1,
                    PushData = new[] { (byte)4 }
                }, Op.GetPushOp(Encoders.ASCII.DecodeData(message)))
            });
            txNew.AddOutput(new TxOut()
            {
                Value = Money.Zero,
                ScriptPubKey = Script.FromBytesUnsafe(Encoders.Hex.DecodeData(pubKeyHex))
            });

            Block genesis = consensusFactory.CreateBlock();
            genesis.Header.BlockTime = Utils.UnixTimeToDateTime(nTime);
            genesis.Header.Bits = nBits;
            genesis.Header.Nonce = nNonce;
            genesis.Header.Version = nVersion;
            genesis.Transactions.Add(txNew);
            genesis.Header.HashPrevBlock = uint256.Zero;
            genesis.UpdateMerkleRoot();
            return genesis;
        }
    }
}
