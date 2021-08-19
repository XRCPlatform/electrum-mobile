using System;
using System.IO;
using System.Linq;
using System.Threading;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.DataEncoders;
using Xunit;

namespace NBitcoin.Tests
{
    public class NetworkTests
    {
        [Fact]
        [Trait("UnitTest", "UnitTest")]
        public void CanGetNetworkFromName()
        {
            Network bitcoinMain = Network.Main;
            Network bitcoinTestnet = Network.TestNet;
            Network bitcoinRegtest = Network.RegTest;
            Network XRCMain = Network.XRCMain;
            Network XRCTestnet = Network.XRCTest;
            Network XRCRegtest = Network.XRCRegTest;

            Assert.Equal(Network.GetNetwork("XRCmain"), XRCMain);
            Assert.Equal(Network.GetNetwork("XRCMain"), XRCMain);
            Assert.Equal(Network.GetNetwork("XRCTest"), XRCTestnet);
            Assert.Equal(Network.GetNetwork("XRCtest"), XRCTestnet);
            Assert.Equal(Network.GetNetwork("XRCRegTest"), XRCRegtest);
            Assert.Equal(Network.GetNetwork("XRCregtest"), XRCRegtest);
            Assert.Null(Network.GetNetwork("invalid"));
        }

        [Fact]
        [Trait("UnitTest", "UnitTest")]
        public void RegisterNetworkTwiceFails()
        {
            Network main = Network.Main;
            var error = Assert.Throws<InvalidOperationException>(() => Network.Register(main));
            Assert.Contains("is already registered", error.Message);
        }

        [Fact]
        [Trait("UnitTest", "UnitTest")]
        public void RegisterNetworkTwiceWithDifferentNamesSucceeds()
        {
            Network main = Network.Main;
            Network main2 = Network.Register(main, "main2");

            Assert.Equal(Network.GetNetwork("XRCMain"), Network.GetNetwork("main2"));
        }
    }
}
