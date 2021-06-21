using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;


namespace NBitcoin
{
    public class ValidatedAddress
    {
        [JsonProperty(PropertyName = "isvalid")]
        public bool IsValid { get; set; }

        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "scriptPubKey")]
        public string ScriptPubKey { get; set; }

        [JsonProperty(PropertyName = "ismine")]
        public bool IsMine { get; set; }

        [JsonProperty(PropertyName = "iswatchonly")]
        public bool IsWatchOnly { get; set; }

        [JsonProperty(PropertyName = "isscript")]
        public bool IsScript { get; set; }
    }
}
