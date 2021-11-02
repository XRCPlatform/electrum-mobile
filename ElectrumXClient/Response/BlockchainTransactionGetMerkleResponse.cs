using Newtonsoft.Json;
using System.Collections.Generic;

namespace ElectrumXClient.Response
{
    public class BlockchainTransactionGetMerkleResponse : ResponseBase
    {
        [JsonProperty("result")]
        public BlockchainTransactionGetMerkleResult Result { get; set; }

        public static BlockchainTransactionGetMerkleResponse FromJson(string json) =>
            JsonConvert.DeserializeObject<BlockchainTransactionGetMerkleResponse>(json, Converter<ResponseResult>.Settings);

        public class BlockchainTransactionGetMerkleResult
        {
            public BlockchainTransactionGetMerkleResult()
            {
                Merkle = new List<string>();
            }

            [JsonProperty("block_height")]
            public uint BlockHeight { get; set; }

            [JsonProperty("merkle")]
            public List<string> Merkle { get; set; }

            [JsonProperty("pos")]
            public uint Pos { get; set; }
        }
    }
}
