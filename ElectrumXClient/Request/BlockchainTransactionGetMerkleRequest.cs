using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class BlockchainTransactionGetMerkleRequest : RequestBase
    {
        public BlockchainTransactionGetMerkleRequest() : base()
        {
            base.Method = "blockchain.transaction.get_merkle";
            base.Parameters = null;
        }
    }
}
