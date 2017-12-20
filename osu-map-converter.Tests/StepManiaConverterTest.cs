using System;
using System.Collections.Generic;
using BMAPI.v1;
using BMAPI.v1.HitObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using osu.Map.Converter;
using osu_to_Intralism.Utils;

namespace osu_map_converter.Tests
{
    [TestClass]
    public class StepManiaConverterTest
    {
        [TestMethod]
        public void CreateTimingMap_ShouldReturnListOfTimings()
        {
            var beatmap = new Beatmap
            {
                HitObjects = new List<CircleObject>
                {
                    new CircleObject { StartTime = 1f },
                    new CircleObject { StartTime = 2f },
                    new CircleObject { StartTime = 2.5f },
                    new CircleObject { StartTime = 3.5f },
                    new CircleObject { StartTime = 4f },
                    new CircleObject { StartTime = 5.5f },
                }
            };

            var expectedTimings = new List<ObjectMap>
            {
                new ObjectMap{ Timing = 1, CircleObject = beatmap.HitObjects[0] },
                new ObjectMap{ Timing = 2, CircleObject = beatmap.HitObjects[1] },
                new ObjectMap{ Timing = 1, CircleObject = beatmap.HitObjects[2] },
                new ObjectMap{ Timing = 2, CircleObject = beatmap.HitObjects[3] },
                new ObjectMap{ Timing = 1, CircleObject = beatmap.HitObjects[4] },
                new ObjectMap{ Timing = 3, CircleObject = beatmap.HitObjects[5] },
            };

            var timings = StepmaniaConverter.CreateTimingMap(beatmap);

            CollectionAssert.AreEqual(expectedTimings, timings);
        }

        [TestMethod]
        public void CreateMap_ShouldReturnListOfDirections()
        {
            var timings = new List<ObjectMap>()
            {
                new ObjectMap{ Timing = 1 },
                new ObjectMap{ Timing = 1 },
                new ObjectMap{ Timing = 2 },
                new ObjectMap{ Timing = 1 },
                new ObjectMap{ Timing = 1 },
                new ObjectMap{ Timing = 2 },
                new ObjectMap{ Timing = 3 },
                new ObjectMap{ Timing = 2 },
                new ObjectMap{ Timing = 3 },
                new ObjectMap{ Timing = 2 },
                new ObjectMap{ Timing = 1 },
                new ObjectMap{ Timing = 1 },
            };

            var patterns = new List<Pattern>
            {
                new Pattern
                {
                    Directions  = new List<int> { 4, 5, 6 },
                    Timings     = new List<int> { 1, 1, 2 }
                },
                new Pattern
                {
                    Directions  = new List<int> { 8, 9 },
                    Timings     = new List<int> { 3, 2 }
                },
            };
            var result = StepmaniaConverter.CreateMap(0, timings, patterns);

            var expectedResult = new List<ObjectMap>()
            {
                new ObjectMap{ Timing = 1, Direction = 4 },
                new ObjectMap{ Timing = 1, Direction = 5 },
                new ObjectMap{ Timing = 2, Direction = 6 },
                new ObjectMap{ Timing = 1, Direction = 4 },
                new ObjectMap{ Timing = 1, Direction = 5 },
                new ObjectMap{ Timing = 2, Direction = 6 },
                new ObjectMap{ Timing = 3, Direction = 8 },
                new ObjectMap{ Timing = 2, Direction = 9 },
                new ObjectMap{ Timing = 3, Direction = 8 },
                new ObjectMap{ Timing = 2, Direction = 9 },
                new ObjectMap{ Timing = 1, Direction = 0 },
                new ObjectMap{ Timing = 1, Direction = 0 },
            };

            CollectionAssert.AreEquivalent(expectedResult, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateMap_ShouldThrowArgumentExceptionWhenPatternListIsEmpty()
        {
            var timings = new List<ObjectMap>
            {
                new ObjectMap{ Timing = 1 },
                new ObjectMap{ Timing = 2 },
                new ObjectMap{ Timing = 3 },
            };

            var patterns = new List<Pattern>();

            StepmaniaConverter.CreateMap(0, timings, patterns);
        }
    }
}
