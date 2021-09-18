using System;
using System.Collections.Generic;
using System.Text;

namespace WalletProvider.Entities
{
    public class WalletMetadata
    {
        /// <summary>
        /// HD Account
        /// </summary>
        public HdAccount Account { get; set; }

        /// <summary>
        /// Receiving addresses
        /// </summary>
        public List<HdAddress> ReceivingAddresses { get; set; }

        /// <summary>
        /// Change addresses
        /// </summary>
        public List<HdAddress> ChangeAddresses { get; set; }

        /// <summary>
        /// Wallet base data
        /// </summary>
        public Wallet Wallet { get; set; }
    }
}
