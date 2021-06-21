using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBitcoin.Crypto;
using NBitcoin.DataEncoders;
using Xunit;

namespace NBitcoin.Tests
{
    public class key_tests
    {
        const string strSecret1 = ("4Mo31zUnzzXh5HS4KcTUxmL3FrSuRKT4zz4PjseRGqbH4Pq5S3b");
        const string strSecret2 = ("4NYr2sJwyjcoS7zngKdoSMDktXJtX2XTzcuMrrvriCS7TZgjj38");
        const string strSecret1C = ("FpnBuNhg2xvTsuXbf9VtR8f2xZFLNLS4evqyNEYA8uUHsYbNGBs7");
        const string strSecret2C = ("Ft7ZRF4DfR71wst4cZFRxtFJiQmxPXjU9KU755NiQqTqVv8NT5ov");
        const string strAddressBad = ("RgDN4S5kRzeozbFW2EruxAyZ8jA9CxwoF");

        BitcoinPubKeyAddress addr1 = (BitcoinPubKeyAddress)Network.Main.CreateBitcoinAddress("RbVMRhNMAt2iFHbyRuKDqW5WHu35ZEo44a");
        BitcoinPubKeyAddress addr2 = (BitcoinPubKeyAddress)Network.Main.CreateBitcoinAddress("RgDN4S5kRzeozbFW2EruxAyZ8jA9CxwoFm");
        BitcoinPubKeyAddress addr1C = (BitcoinPubKeyAddress)Network.Main.CreateBitcoinAddress("RkmzitmmMUaoD4pCtPMFfv8umGDdTcEgQD");
        BitcoinPubKeyAddress addr2C = (BitcoinPubKeyAddress)Network.Main.CreateBitcoinAddress("RmVq3RwXhHSDnGG3ftGft19aG4SkuSJv5j");


        BitcoinAddress addrLocal = Network.Main.CreateBitcoinAddress("RpHNMdRPTJDNmKBR2eGnBQ7Z8RQNJUUAg3");
        uint256 msgLocal = Hashes.Hash256(TestUtils.ToBytes("Localbitcoins.com will change the world"));
        byte[] signatureLocal = Convert.FromBase64String("IJ/17TjGGUqmEppAliYBUesKHoHzfY4gR4DW0Yg7QzrHUB5FwX1uTJ/H21CF8ncY8HHNB5/lh8kPAOeD5QxV8Xc=");

        [Fact]
        [Trait("UnitTest", "UnitTest")]
        public void CanVerifySignature()
        {
            var tests = new[]
            {
                new
                {
                    Address = "RqNASqaCmQUirCnm1inTkbaAkSfAdBXQLQ",
                    PrivateKey = "FrkyXRbDEd6dT5MmerHzX1rEK3JxB9rNNmVSTCGjCTEH4S1VMxhD",
                    Message = "This is an example of a signed message.",
                    Signature = "ILAAHRRW3n2LX0W8Kd4K4OWGYgPPFAg1V1g/fra7JP8OL2OaqjhdGXEzBP/FUsjE4u+8y6k4Mlryxo0QDCU2Pqg="
                },
                new
                {
                    Address = "Rbu3WGhYPuZzeKLZPMdRfvd5SnNM5E7zPB",
                    PrivateKey = "FrCQzSKcx8eCPy6pWp5cxXnkvT32C1ozwufLfhwrU9xv6zHLbqyj",
                    Message = "hello world",
                    Signature = "HyhT0IVZlWd0sesuqs9NLk8fx0bhzYOFlxJt+wPjuhEvLF1IR8cjUGeCG7+OZ7p7E3gEUM5afpkkR6wtPtaM/Mo="
                },
                new
                {
                    Address = "RnXYWqMoet7fW4ADnQhQK3R5JQCTsWyQLh",
                    PrivateKey = null as string, // "Fq4dyFPp6GANwXhstP1kXt7kFdwciNzbG399wLcF4H8dPYSVSfQW",
                    Message = "Localbitcoins.com will change the world",
                    Signature = "IOKUuat/1JvDyja18E7g0VqCfB8JZa7fLMpo7GPKX+VLc7qsS35GjgOrnATTEGJY60b+7VkG1swwqyfzGDqrdew="
                },
                new
                {
                    Address = "Rok6y7ZcHPtbJtDgk3Bv15zUNRBJYviRbr",
                    PrivateKey = "FtRNGTmg3wnfLCMtrrPqze2GUQFASDwzEHgfr9g4PRz6zGMxuxpW",
                    Message = "This is an example of a signed message2",
                    Signature = "H96g5iogFiOcN4/Ju8hoTHI8iZFrqjmuwaTD6hjJVY+THOQkYC9Xm8DtLxj0XLHREi7+nr2laUaOobJI3SarJh4="
                },
                new
                {
                    Address = "Rtoa81TWHB2onLi7J11QzjgqLi3jY3eM7x",
                    PrivateKey = "FoRs91ydfVBxyYmBA1LSPADCR8rdUBEEkxhSL5petCCBgP64DRbo",
                    Message = "this is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long messagethis is a very long message",
                    Signature = "IOEDlFJOxMZUEzXreUBtIGv/Um/CefbD4/q0Cqa2UogpZjcdClrhn8AvWQ0vzkxufyPVFvmtF3H+PZHNUC8LVTw="
                },
            };


            foreach(var test in tests)
            {
                if(test.PrivateKey != null)
                {
                    var secret = Network.Main.CreateBitcoinSecret(test.PrivateKey);
                    var signature = secret.PrivateKey.SignMessage(test.Message);
                    Assert.True(((BitcoinPubKeyAddress)Network.Main.CreateBitcoinAddress(test.Address)).VerifyMessage(test.Message, signature));
                    Assert.True(secret.PubKey.VerifyMessage(test.Message, signature));
                }
                BitcoinPubKeyAddress address = (BitcoinPubKeyAddress)Network.Main.CreateBitcoinAddress(test.Address);
                Assert.True(address.VerifyMessage(test.Message, test.Signature));
                Assert.True(!address.VerifyMessage("bad message", test.Signature));
            }
        }

        [Fact]
        [Trait("UnitTest", "UnitTest")]
        public void CanGeneratePubKeysAndAddress()
        {
            //Took from http://brainwallet.org/ and http://procbits.com/2013/08/27/generating-a-bitcoin-address-with-javascript
            var tests = new[]
            {
                new
                {
                    Address = "Rt2hmjyerERs7bMEtqQv9xeXvbsuSKBN9c",
                    PrivateKeyWIF = "4PJRkswc8vCKUzZXaPeBKYFmm88PKiCpBC5ncvLnvYf6PL7hSeU",
                    PubKey = "04a62e4bbba0a19e491388441bce3a34c42199b272696ee18b10fc2611cfb4caddbf4ee78543926b9b115f69e7ce2bdffbb5a21d9a910f4d9b85107855db210261",
                    CompressedPrivateKeyWIF = "FwRvGfr25wnxaZ4ZxNw6YpxCZkTbXas9Dyv8MYSMhLj8mSWuNDjY",
                    CompressedPubKey = "03a62e4bbba0a19e491388441bce3a34c42199b272696ee18b10fc2611cfb4cadd",
                    CompressedAddress = "Ru8iXLEPRA3tvXpGMDNvqnCa9vJ5ERLGgP",
                    Hash160= "d4eff4c98deecca671c6579a3eab950fe911e619",
                    CompressedHash160 = "e10b348337aed78a05d318be6fef18c5dca0dc7f"
                },

                new
                {
                    Address = "Rju4zW5NP1AbtLQuwJhsWM8M3CczuXncRv",
                    PrivateKeyWIF = "4MfkUVXczxDpXoGRz3czm3iND8NLhEwss3eLvGMxHjBvFKLCCzT",
                    PubKey = "04852d83103162b39b08fd50e3c51cb5ab65939106f5444162879d004663c5a44defe461b2dc17610dc3488625ac6c47539cf0a3da07ac18c76cd550e721f2c4e9",
                    CompressedPrivateKeyWIF = "FpE2tLv9tnjanwD7GnZswQyacXXBvcZDVtfpv6oV2qDC1so9C91s",
                    CompressedPubKey = "03852d83103162b39b08fd50e3c51cb5ab65939106f5444162879d004663c5a44d",
                    CompressedAddress = "RZhf55nWFbLpyitwSdw9u5MNKr1Mnhg5V3",
                    Hash160 = "7bbd55f748912082cd80d9e1e46f62fd93b25c4f",
                    CompressedHash160 = "0be388bcc23428d9ef25a1e27e440467b45efa8f"
                }
            };

            foreach(var test in tests)
            {
                BitcoinSecret secret = Network.Main.CreateBitcoinSecret(test.PrivateKeyWIF);
                Assert.Equal(test.PubKey, secret.PrivateKey.PubKey.ToHex());

                var address = (BitcoinPubKeyAddress)Network.Main.CreateBitcoinAddress(test.Address);
                Assert.Equal(new KeyId(test.Hash160), address.Hash);
                Assert.Equal(new KeyId(test.Hash160), secret.PrivateKey.PubKey.Hash);
                Assert.Equal(address.Hash, secret.PrivateKey.PubKey.GetAddress(Network.Main).Hash);

                var compressedSec = secret.Copy(true);

                var a = secret.PrivateKey.PubKey;
                var b = compressedSec.PrivateKey.PubKey;

                Assert.Equal(test.CompressedPrivateKeyWIF, compressedSec.ToWif());
                Assert.Equal(test.CompressedPubKey, compressedSec.PrivateKey.PubKey.ToHex());
                Assert.True(compressedSec.PrivateKey.PubKey.IsCompressed);

                var compressedAddr = (BitcoinPubKeyAddress)Network.Main.CreateBitcoinAddress(test.CompressedAddress);
                Assert.Equal(new KeyId(test.CompressedHash160), compressedAddr.Hash);
                Assert.Equal(new KeyId(test.CompressedHash160), compressedSec.PrivateKey.PubKey.Hash);


            }
        }

        [Fact]
        [Trait("Core", "Core")]
        public void key_test1()
        {
            BitcoinSecret bsecret1 = Network.Main.CreateBitcoinSecret(strSecret1);
            BitcoinSecret bsecret2 = Network.Main.CreateBitcoinSecret(strSecret2);
            BitcoinSecret bsecret1C = Network.Main.CreateBitcoinSecret(strSecret1C);
            BitcoinSecret bsecret2C = Network.Main.CreateBitcoinSecret(strSecret2C);
            Assert.Throws<FormatException>(() => Network.Main.CreateBitcoinSecret(strAddressBad));

            Key key1 = bsecret1.PrivateKey;
            Assert.True(key1.IsCompressed == false);
            Assert.True(bsecret1.Copy(true).PrivateKey.IsCompressed == true);
            Assert.True(bsecret1.Copy(true).Copy(false).IsCompressed == false);
            Assert.True(bsecret1.Copy(true).Copy(false).ToString() == bsecret1.ToString());
            Key key2 = bsecret2.PrivateKey;
            Assert.True(key2.IsCompressed == false);
            Key key1C = bsecret1C.PrivateKey;
            Assert.True(key1C.IsCompressed == true);
            Key key2C = bsecret2C.PrivateKey;
            Assert.True(key1C.IsCompressed == true);

            PubKey pubkey1 = key1.PubKey;
            PubKey pubkey2 = key2.PubKey;
            PubKey pubkey1C = key1C.PubKey;
            PubKey pubkey2C = key2C.PubKey;

            Assert.True(addr1.Hash == pubkey1.Hash);
            Assert.True(addr2.Hash == pubkey2.Hash);
            Assert.True(addr1C.Hash == pubkey1C.Hash);
            Assert.True(addr2C.Hash == pubkey2C.Hash);



            for(int n = 0; n < 16; n++)
            {
                string strMsg = String.Format("Very secret message {0}: 11", n);
                if(n == 10)
                {
                    //Test one long message
                    strMsg = String.Join(",", Enumerable.Range(0, 2000).Select(i => i.ToString()).ToArray());
                }
                uint256 hashMsg = Hashes.Hash256(TestUtils.ToBytes(strMsg));

                // normal signatures

                ECDSASignature sign1 = null, sign2 = null, sign1C = null, sign2C = null;
                List<Task> tasks = new List<Task>();
                tasks.Add(Task.Run(() => sign1 = key1.Sign(hashMsg)));
                tasks.Add(Task.Run(() => sign2 = key2.Sign(hashMsg)));
                tasks.Add(Task.Run(() => sign1C = key1C.Sign(hashMsg)));
                tasks.Add(Task.Run(() => sign2C = key2C.Sign(hashMsg)));
                Task.WaitAll(tasks.ToArray());
                tasks.Clear();

                tasks.Add(Task.Run(() => Assert.True(pubkey1.Verify(hashMsg, sign1))));
                tasks.Add(Task.Run(() => Assert.True(pubkey2.Verify(hashMsg, sign2))));
                tasks.Add(Task.Run(() => Assert.True(pubkey1C.Verify(hashMsg, sign1C))));
                tasks.Add(Task.Run(() => Assert.True(pubkey2C.Verify(hashMsg, sign2C))));
                Task.WaitAll(tasks.ToArray());
                tasks.Clear();

                tasks.Add(Task.Run(() => Assert.True(pubkey1.Verify(hashMsg, sign1))));
                tasks.Add(Task.Run(() => Assert.True(!pubkey1.Verify(hashMsg, sign2))));
                tasks.Add(Task.Run(() => Assert.True(pubkey1.Verify(hashMsg, sign1C))));
                tasks.Add(Task.Run(() => Assert.True(!pubkey1.Verify(hashMsg, sign2C))));

                tasks.Add(Task.Run(() => Assert.True(!pubkey2.Verify(hashMsg, sign1))));
                tasks.Add(Task.Run(() => Assert.True(pubkey2.Verify(hashMsg, sign2))));
                tasks.Add(Task.Run(() => Assert.True(!pubkey2.Verify(hashMsg, sign1C))));
                tasks.Add(Task.Run(() => Assert.True(pubkey2.Verify(hashMsg, sign2C))));

                tasks.Add(Task.Run(() => Assert.True(pubkey1C.Verify(hashMsg, sign1))));
                tasks.Add(Task.Run(() => Assert.True(!pubkey1C.Verify(hashMsg, sign2))));
                tasks.Add(Task.Run(() => Assert.True(pubkey1C.Verify(hashMsg, sign1C))));
                tasks.Add(Task.Run(() => Assert.True(!pubkey1C.Verify(hashMsg, sign2C))));

                tasks.Add(Task.Run(() => Assert.True(!pubkey2C.Verify(hashMsg, sign1))));
                tasks.Add(Task.Run(() => Assert.True(pubkey2C.Verify(hashMsg, sign2))));
                tasks.Add(Task.Run(() => Assert.True(!pubkey2C.Verify(hashMsg, sign1C))));
                tasks.Add(Task.Run(() => Assert.True(pubkey2C.Verify(hashMsg, sign2C))));

                Task.WaitAll(tasks.ToArray());
                tasks.Clear();

                // compact signatures (with key recovery)

                byte[] csign1 = null, csign2 = null, csign1C = null, csign2C = null;

                tasks.Add(Task.Run(() => csign1 = key1.SignCompact(hashMsg)));
                tasks.Add(Task.Run(() => csign2 = key2.SignCompact(hashMsg)));
                tasks.Add(Task.Run(() => csign1C = key1C.SignCompact(hashMsg)));
                tasks.Add(Task.Run(() => csign2C = key2C.SignCompact(hashMsg)));
                Task.WaitAll(tasks.ToArray());
                tasks.Clear();

                PubKey rkey1 = null, rkey2 = null, rkey1C = null, rkey2C = null;
                tasks.Add(Task.Run(() => rkey1 = PubKey.RecoverCompact(hashMsg, csign1)));
                tasks.Add(Task.Run(() => rkey2 = PubKey.RecoverCompact(hashMsg, csign2)));
                tasks.Add(Task.Run(() => rkey1C = PubKey.RecoverCompact(hashMsg, csign1C)));
                tasks.Add(Task.Run(() => rkey2C = PubKey.RecoverCompact(hashMsg, csign2C)));
                Task.WaitAll(tasks.ToArray());
                tasks.Clear();

                Assert.True(rkey1.ToHex() == pubkey1.ToHex());
                Assert.True(rkey2.ToHex() == pubkey2.ToHex());
                Assert.True(rkey1C.ToHex() == pubkey1C.ToHex());
                Assert.True(rkey2C.ToHex() == pubkey2C.ToHex());
            }
        }


        [Fact]
        [Trait("Core", "Core")]
        public void key_test_from_bytes()
        {
            Byte[] privateKey = new Byte[32] { 0xE9, 0x87, 0x3D, 0x79, 0xC6, 0xD8, 0x7D, 0xC0, 0xFB, 0x6A, 0x57, 0x78, 0x63, 0x33, 0x89, 0xF4, 0x45, 0x32, 0x13, 0x30, 0x3D, 0xA6, 0x1F, 0x20, 0xBD, 0x67, 0xFC, 0x23, 0x3A, 0xA3, 0x32, 0x62 };
            Key key1 = new Key(privateKey, -1, false);

            ISecret wifKey = key1.GetWif(NBitcoin.Network.Main);

            const String expected = "4PAHor6G8ma51qQyEyn8PPChguDmMqwX4oXWXgcMA2nmj4Rp7o3";
            Assert.True(wifKey.ToString() == expected);
        }
    }
}
