using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Response
{
    public class BlockchainRelayFeeResponse : ResponseBase
    {
        [JsonProperty("result")]
        public decimal? Result { get; set; }

        public static BlockchainRelayFeeResponse FromJson(string json) =>
            JsonConvert.DeserializeObject<BlockchainRelayFeeResponse>(json, Converter<ResponseResult>.Settings);
    }
}