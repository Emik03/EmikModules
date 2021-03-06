using Newtonsoft.Json;

namespace Netherite
{
    internal class VoltData
    {
        [JsonProperty("voltage")]
        public string Voltage { get; set; }
    }
}
