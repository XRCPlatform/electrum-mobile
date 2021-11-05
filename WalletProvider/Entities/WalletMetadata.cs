using System;
using System.Collections.Generic;
using System.Text;

namespace WalletProvider.Entities
{
    public class WalletMetadata
    {
        /// <summary>
        /// Wallet base data
        /// </summary>
        public Wallet Wallet { get; set; }

        /// <summary>
        /// Encrypted Password
        /// </summary>
        public string PasswordEncrypted { get; set; }

        /// <summary>
        /// UserName
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Seed
        /// </summary>
        public string Seed { get; set; }

        /// <summary>
        /// Is Main Network
        /// </summary>
        public bool IsMainNetwork { get; set; }
    }
}
