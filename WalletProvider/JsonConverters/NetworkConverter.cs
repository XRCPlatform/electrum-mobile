using System;
using NBitcoin;
using Newtonsoft.Json;

namespace WalletProvider.JsonConverters
{
    /// <summary>
    /// Converter used to convert <see cref="Network"/> to and from JSON.
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.JsonConverter" />
    public class NetworkConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Network);
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var network = reader.Value.ToString().ToLowerInvariant();
            Network selectNetwork = Network.GetNetwork(network);

            if (selectNetwork == null)
                throw new ArgumentException($"Network '{network}' is not a valid network.");

            return selectNetwork;
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((Network)value).ToString());
        }
    }
}
