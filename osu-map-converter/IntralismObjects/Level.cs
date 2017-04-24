using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Map.Converter.IntralismObjects
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Level
    {
        [JsonProperty(PropertyName = "id")]
        public string Id = "";

        [JsonProperty(PropertyName = "name")]
        public string Name = "";

        [JsonProperty(PropertyName = "info")]
        public string Description = "";

        [JsonProperty(PropertyName = "levelResources")]
        public Resource[] Resources = new Resource[0];

        [JsonProperty(PropertyName = "tags")]
        public LevelTag[] Tags = new [] { LevelTag.OneHand };

        [JsonConverter(typeof(StringEnumConverter))]
        public enum LevelTag { OneHand }

        [JsonProperty(PropertyName = "handCount")]
        public int HandCount = 1;

        [JsonProperty(PropertyName = "moreInfoURL")]
        public string MoreInfoURL = "";

        [JsonProperty(PropertyName = "speed")]
        public float Speed = 25;

        [JsonProperty(PropertyName = "lives")]
        public int Lives = 5;

        [JsonProperty(PropertyName = "maxLives")]
        public int MaxLives = 5;

        [JsonProperty(PropertyName = "musicFile")]
        public string MusicFile = "";

        [JsonProperty(PropertyName = "musicTime")]
        public float MusicTime = 0f;

        [JsonProperty(PropertyName = "iconFile")]
        public string IconFile = "";

        [JsonProperty(PropertyName = "generationType")]
        public int GenerationType = 4;

        [JsonProperty(PropertyName = "environmentType")]
        public int EnvironmentType = 0;

        [JsonProperty(PropertyName = "unlockConditions")]
        public string[] UnlockConditions = new string[0];

        [JsonProperty(PropertyName = "hidden")]
        public bool Hidden = false;

        [JsonProperty(PropertyName = "checkpoints")]
        public float[] Checkpoints = new float[0];

        [JsonProperty(PropertyName = "puzzleSequencesList")]
        public string[] PuzzleSequencesList = new string[0];

        [JsonProperty(PropertyName = "events")]
        public List<IEvent> Events = new List<IEvent>();
    }
}
