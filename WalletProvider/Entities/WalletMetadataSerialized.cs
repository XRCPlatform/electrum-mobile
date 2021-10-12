using System;
using System.Collections.Generic;
using System.Text;

namespace WalletProvider.Entities
{
    public class WalletMetadataSerialized
    {
        /// <summary>
        /// HD Account
        /// </summary>
        public byte[] Account { get; set; }

        /// <summary>
        /// Receiving addresses
        /// </summary>
        public List<byte[]> ReceivingAddresses { get; set; }

        /// <summary>
        /// Change addresses
        /// </summary>
        public List<byte[]> ChangeAddresses { get; set; }

        /// <summary>
        /// Wallet base data
        /// </summary>
        public byte[] Wallet { get; set; }

        /// <summary>
        /// Wallet seed
        /// </summary>
        public string Seed { get; set; }

        /// <summary>
        /// Encrypted Password
        /// </summary>
        public string PasswordEncrypted { get; set; }

        /// <summary>
        /// UserName
        /// </summary>
        public string UserName { get; set; }

    }
}
