using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Map.Converter.IntralismObjects
{
    public class ShowSpriteEvent : IEvent
    {
        public float Time { get; set; }

        public string[] Data
        {
            get
            {
                string kar = KeepAspectRatio ? "True" : "False";
                return new string[]
                {
                    "ShowSprite",
                    $"{Resource?.Name},{Position},{kar},{Duration}"
                };
            }
        }

        public Resource Resource;

        public float Position;

        public float Duration;

        public bool KeepAspectRatio;

        public ShowSpriteEvent(Resource resource, float position, float duration, bool keepAspectRatio, float time)
        {
            this.Resource = resource;
            this.Position = position;
            this.Duration = duration;
            this.KeepAspectRatio = keepAspectRatio;
            this.Time = time;
        }
    }
}
