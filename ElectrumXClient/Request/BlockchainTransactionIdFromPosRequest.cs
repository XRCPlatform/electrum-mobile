using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class BlockchainTransactionIdFromPosRequest : RequestBase
    {
        public BlockchainTransactionIdFromPosRequest() : base()
        {
            base.Method = "blockchain.transaction.id_from_pos";
        }
    }
}