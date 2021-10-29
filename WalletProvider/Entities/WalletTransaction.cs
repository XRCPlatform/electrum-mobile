using System;
using System.Collections.Generic;
using System.Text;
using static ElectrumXClient.Response.BlockchainTransactionGetResponse;

namespace WalletProvider.Entities
{
    public class WalletTransaction
    {
        public HdAddress Address { get; set; }
        public BlockchainTransactionGetResult Transaction { get; set; }
    }
}
