using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class BlockchainRelayFeeRequest : RequestBase
    {
        public BlockchainRelayFeeRequest() : base()
        {
            base.Method = "blockchain.relayfee";
            base.Parameters = null;
        }
    }
}
