using NBitcoin;
using System;
using System.Collections.Generic;
using System.Text;

namespace WalletProvider.Entities
{
    public class WalletBalance
    {
        /// <summary>
        /// The balance of confirmed transactions.
        /// </summary>
        public Money AmountConfirmed { get; set; }

        /// <summary>
        /// The balance of unconfirmed transactions.
        /// </summary>
        public Money AmountUnconfirmed { get; set; }

        /// <summary>
        /// The balance of imature coinbase transactions 
        /// </summary>
        public Money AmountImmature { get; set; }
    }
}
