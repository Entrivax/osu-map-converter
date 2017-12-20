using BMAPI.v1.HitObjects;
using System.Collections.Generic;

namespace osu_to_Intralism.Utils
{
    public class ObjectMap
    {
        public CircleObject CircleObject;
        public int Timing;
        public int Direction;

        public override bool Equals(object obj)
        {
            var map = obj as ObjectMap;
            return map != null &&
                   EqualityComparer<CircleObject>.Default.Equals(CircleObject, map.CircleObject) &&
                   Timing == map.Timing &&
                   Direction == map.Direction;
        }

        public override int GetHashCode()
        {
            var hashCode = 1254876447;
            hashCode = hashCode * -1521134295 + EqualityComparer<CircleObject>.Default.GetHashCode(CircleObject);
            hashCode = hashCode * -1521134295 + Timing.GetHashCode();
            hashCode = hashCode * -1521134295 + Direction.GetHashCode();
            return hashCode;
        }
    }
}
