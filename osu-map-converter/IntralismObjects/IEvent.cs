using Newtonsoft.Json;

namespace osu.Map.Converter.IntralismObjects
{
    [JsonObject(MemberSerialization.OptIn)]
    public interface IEvent
    {
        [JsonProperty(PropertyName = "time")]
        float Time { get; set; }

        [JsonProperty(PropertyName = "data")]
        string[] Data { get; }
    }
}
