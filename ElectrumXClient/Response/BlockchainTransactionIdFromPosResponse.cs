using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Response
{
    public class BlockchainTransactionIdFromPosResponse : ResponseBase
    {
        [JsonProperty("result")]
        public ResponseResult Result { get; set; }

        public static BlockchainTransactionIdFromPosResponse FromJson(string json) =>
            JsonConvert.DeserializeObject<BlockchainTransactionIdFromPosResponse>(json, Converter<ResponseResult>.Settings);

        //public class BlockchainTransactionIdFromPosResult
        //{
        //    BlockchainTransactionIdFromPosResult()
        //    {
        //        Merkle = new List<string>();
        //    }

        //    [JsonProperty("merkle")]
        //    public List<string> Merkle { get; set; }

        //    [JsonProperty("tx_hash")]
        //    public string TxHash { get; set; }
        //}
    }
}

