using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class BlockchainEstimateFeeRequest : RequestBase
    {
        public BlockchainEstimateFeeRequest() : base()
        {
            base.Method = "blockchain.estimatefee";
            base.Parameters = null;
        }
    }
}
