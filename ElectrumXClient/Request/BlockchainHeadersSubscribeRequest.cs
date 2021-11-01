using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class BlockchainHeadersSubscribeRequest:RequestBase
    {
        public BlockchainHeadersSubscribeRequest() : base()
        {
            base.Method = "blockchain.headers.subscribe";
            base.Parameters = new string[0];
        }
    }
}
