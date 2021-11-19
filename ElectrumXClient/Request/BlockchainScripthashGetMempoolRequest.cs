using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class BlockchainScripthashGetMempoolRequest : RequestBase
    {
        public BlockchainScripthashGetMempoolRequest() : base()
        {
            base.Method = "blockchain.scripthash.get_mempool";
            base.Parameters = null;
        }
    }
}
