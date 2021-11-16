using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Response
{
    public class BlockchainEstimateFeeResponse : ResponseBase
    {
        [JsonProperty("result")]
        public decimal? Result { get; set; }

        public static BlockchainEstimateFeeResponse FromJson(string json) =>
            JsonConvert.DeserializeObject<BlockchainEstimateFeeResponse>(json, Converter<ResponseResult>.Settings);
    }
}
