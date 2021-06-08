using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class BlockchainBlockHeaderRequest : RequestBase
    {
        public BlockchainBlockHeaderRequest() : base()
        {
            base.Method = "blockchain.block.header";
            base.Parameters = null;
        }
    }
}
