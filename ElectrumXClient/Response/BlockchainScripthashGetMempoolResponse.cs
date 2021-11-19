using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Response
{
    public class BlockchainScripthashGetMempoolResponse : ResponseBase
    {
        [JsonProperty("result")]
        public List<BlockchainScripthashGetHistoryResult> Result { get; set; }

        public static BlockchainScripthashGetMempoolResponse FromJson(string json) =>
            JsonConvert.DeserializeObject<BlockchainScripthashGetMempoolResponse>(json, Converter<ResponseResult>.Settings);

        public class BlockchainScripthashGetHistoryResult
        {
            [JsonProperty("height")]
            public int Height { get; set; }

            [JsonProperty("tx_hash")]
            public string TxHash { get; set; }

            [JsonProperty("fee")]
            public decimal Fee { get; set; }
        }
    }
}