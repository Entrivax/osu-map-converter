using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Map.Converter.IntralismObjects
{
    public class SpawnObjEvent : IEvent
    {
        public float Time { get; set; }

        public string[] Data
        {
            get
            {
                if (Objs.Count == 1)
                    return new string[]
                    {
                        "SpawnObj",
                        $"[{Objs[0]}]"
                    };
                string[] output = new string[] { "SpawnObj", "[" };
                for (int i = 0; i < Objs.Count; i++)
                {
                    output[1] += ((i > 0) ? "-" : "") + Objs[i];
                }
                output[1] += "]";
                return output;
            }
        }

        public List<Object> Objs;

        public enum Object { Up, Right, Down, Left, PowerUp };

        public SpawnObjEvent(float time, Object obj)
        {
            this.Objs = new List<Object>();
            this.Objs.Add(obj);
            this.Time = time;
        }

        public void AddObject(Object obj)
        {
            if (Objs.Count >= 4
                || (obj == Object.PowerUp && Objs.Count > 0)
                || (obj != Object.PowerUp && Objs.Contains(Object.PowerUp))
                || Objs.Contains(obj))
                return;
            Objs.Add(obj);
            Objs.Sort();
        }
    }
}
