using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Response
{
    public class BlockchainHeadersSubscribeResponse : ResponseBase
    {
        [JsonProperty("result")]
        public BlockchainHeadersSubscribeResult Result { get; set; }

        public static BlockchainHeadersSubscribeResponse FromJson(string json) =>
            JsonConvert.DeserializeObject<BlockchainHeadersSubscribeResponse>(json, Converter<ResponseResult>.Settings);

        public class BlockchainHeadersSubscribeResult
        {
            [JsonProperty("version")]
            public uint Version { get; set; }

            [JsonProperty("prev_block_hash")]
            public string PrevBlockHash { get; set; }

            [JsonProperty("merkle_root")]
            public string MerkleRoot { get; set; }

            [JsonProperty("timestamp")]
            public long Timestamp { get; set; }

            [JsonProperty("bits")]
            public uint Bits { get; set; }

            [JsonProperty("nonce")]
            public uint Nonce { get; set; }

            [JsonProperty("block_height")]
            public int BlockHeight { get; set; }
        }
    }
}