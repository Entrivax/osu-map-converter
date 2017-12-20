using BMAPI.v1;
using BMAPI.v1.HitObjects;
using Newtonsoft.Json;
using osu.Map.Converter.StepmaniaObjects;
using osu.Map.Converter.Utils;
using osu_to_Intralism.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace osu.Map.Converter
{
    public class StepmaniaConverter
    {
        public static void ConvertToStepmania(Beatmap beatmap, string patternsInput, string osuInput, string stepmaniaDir)
        {
            var simfile = new Simfile
            {
                Artist = beatmap.Artist,
                ArtistTranslit = beatmap.ArtistUnicode,
                MapCreator = beatmap.Creator,
                Offset = -beatmap.TimingPoints[0].Time / 1000f,
                SampleStart = (beatmap.PreviewTime ?? 0) / 1000f,
                SampleLength = (beatmap.HitObjects[beatmap.HitObjects.Count - 1].StartTime - beatmap.PreviewTime ?? 0) / 1000f,
                Title = beatmap.Title,
                TitleTranslit = beatmap.TitleUnicode,
                Music = beatmap.AudioFilename,
                Comments = $"Steps converted from osu! beatmap.{Environment.NewLine}Original beatmap http://osu.ppy.sh/s/{beatmap.BeatmapSetID}",
                Selectable = true
            };

            simfile.BPMS.Add(new DoublePair((beatmap.TimingPoints[0].Time - beatmap.TimingPoints[0].Time) / 1000d, 60000 / beatmap.TimingPoints[0].BpmDelay));
            /*double lastNonInheritedBPM = 0;
            for(int i = 0; i < beatmap.TimingPoints.Count; i++)
            {
                if (!beatmap.TimingPoints[i].InheritsBPM)
                {
                    simfile.BPMS.Add(new DoublePair((beatmap.TimingPoints[i].Time - beatmap.TimingPoints[0].Time) / 1000d, 60000 / beatmap.TimingPoints[i].BpmDelay));
                    lastNonInheritedBPM = beatmap.TimingPoints[i].BpmDelay;
                }
                else
                {
                    simfile.BPMS.Add(new DoublePair((beatmap.TimingPoints[i].Time - beatmap.TimingPoints[0].Time) / 1000d, 60000 / (lastNonInheritedBPM * (-100 / beatmap.TimingPoints[i].BpmDelay))));
                }
            }*/

            var objectsMap = CreateTimingMap(beatmap);
            var patterns = JsonConvert.DeserializeObject<List<Pattern>>(File.ReadAllText(patternsInput));
            CreateMap(new Random().Next(), objectsMap, patterns);

            CreateMeasures(objectsMap, beatmap, simfile);

            var beatmapDir = Path.GetDirectoryName(osuInput);
            var songFileName = string.Join("_", beatmap.Title.Split(Path.GetInvalidFileNameChars()));
            var outputDir = Path.Combine(stepmaniaDir, "Songs", "osu converted", songFileName);

            Directory.CreateDirectory(outputDir);

            var audioOutputFilePath = Path.Combine(outputDir, simfile.Music);
            if (File.Exists(audioOutputFilePath))
                File.Delete(audioOutputFilePath);

            File.Copy(Path.Combine(beatmapDir, beatmap.AudioFilename), audioOutputFilePath);

            File.WriteAllText(Path.Combine(outputDir, songFileName + ".sm"), GetOutputSm(simfile));
        }

        /// <summary>
        /// Get measure and note at <paramref name="time"/>
        /// </summary>
        /// <param name="time">Time in seconds from which to get measure and note</param>
        /// <param name="bpms">List of BPMs of the simfile</param>
        /// <param name="measure">The measure at <paramref name="time"/></param>
        /// <param name="note">The note at <paramref name="time"/></param>
        private static void GetMeasureAndDivisionAt(double time, List<DoublePair> bpms, out int measure, out int note)
        {
            if (/*bpms.Count == 1*/ true)
            {
                double measureLength = (60f / bpms[0].v2) * 4d;
                measure = (int)(time / measureLength);
                double measureStart = measure * measureLength;
                note = (int)Math.Round(192 * ((time - measureStart) / measureLength));
            }
            else
            {
                measure = 0;
                note = 0;
                double measuresCounted = 0;
                double previousMeasureLength = 0;
                double previousBPMMeasureStart = 0;
                for (int i = 0; i < bpms.Count; i++)
                {
                    double currentMeasureLength = (60f / bpms[i].v2) * 4d;
                    Console.WriteLine($"Changed measure length to {currentMeasureLength} ---- BPM : {bpms[i].v2}");
                    if (i == 0)
                    {
                        previousMeasureLength = currentMeasureLength;
                    }

                    if (i + 1 < bpms.Count)
                    {
                        if (bpms[i + 1].v1 < time)
                        {
                            double measuresElapsed = (bpms[i].v1 - previousBPMMeasureStart) / previousMeasureLength;
                            measuresCounted = measuresCounted + measuresElapsed;
                            previousMeasureLength = currentMeasureLength;
                            previousBPMMeasureStart = bpms[i].v1;
                        }
                        else
                        {
                            double measuresElapsed = (time - previousBPMMeasureStart) / previousMeasureLength;
                            measure = (int)measuresElapsed;
                            note = (int)Math.Round(192 * ((time - bpms[i].v1) / currentMeasureLength)) - measure * 192;
                            break;
                        }
                    }
                    else
                    {
                        double measuresElapsed = (time - previousBPMMeasureStart) / previousMeasureLength;
                        measure = (int)measuresElapsed;
                        note = (int)Math.Round(192 * ((time - bpms[i].v1) / currentMeasureLength)) - measure * 192;
                        break;
                    }
                }
            }
        }

        private static float GetTiniestTimeSpace(Beatmap beatmap)
        {
            float time = 999999f;

            beatmap.HitObjects.ForEach(circleA =>
            {
                beatmap.HitObjects.ForEach(circleB =>
                {
                    if (circleA == circleB)
                        return;

                    if (circleA.StartTime > circleB.StartTime)
                        return;

                    var timeDiff = circleB.StartTime - circleA.StartTime;

                    if (timeDiff < time)
                        time = timeDiff;
                });
            });

            return time;
        }

        public static List<ObjectMap> CreateTimingMap(Beatmap beatmap)
        {
            var timeBase = Math.Round(GetTiniestTimeSpace(beatmap), 3);
            var orderedObjects = beatmap.HitObjects.OrderBy(circle => circle.StartTime).ToList();

            var timings = new List<ObjectMap>();

            for(int i = 0; i < orderedObjects.Count; i++)
            {
                var objectMap = new ObjectMap
                {
                    Timing = 1,
                    CircleObject = orderedObjects[i]
                };

                if (i == 0)
                {
                    timings.Add(objectMap);
                    continue;
                }

                var timeDiff = Math.Round(orderedObjects[i].StartTime - orderedObjects[i - 1].StartTime, 3);

                objectMap.Timing = (int)Math.Round(timeDiff / timeBase, 0);
                timings.Add(objectMap);
            }

            return timings;
        }

        public static List<ObjectMap> CreateMap(int seed, List<ObjectMap> objectMap, List<Pattern> patterns)
        {
            if (objectMap == null)
                throw new ArgumentNullException("objectMap connot be null");
            if (patterns == null)
                throw new ArgumentNullException("patterns connot be null");

            if (patterns.Count < 1)
                throw new ArgumentException("Patterns cannot be empty");
            
            var random = new Random(seed);
            var i = 0;
            while (i < objectMap.Count)
            {
                var usablePatterns = new List<Pattern>();
                foreach (var pattern in patterns)
                {
                    var canUsePattern = true;
                    for(int j = 0; j < pattern.Timings.Count; j++)
                    {
                        if (objectMap.Count <= i + j || objectMap[i + j].Timing != pattern.Timings[j])
                        {
                            canUsePattern = false;
                            break;
                        }
                    }
                    if (canUsePattern)
                        usablePatterns.Add(pattern);
                }

                if (usablePatterns.Count > 0)
                {
                    usablePatterns = usablePatterns.OrderByDescending(pattern => pattern.Timings.Count).ToList();
                    usablePatterns = usablePatterns.Where(pattern => pattern.Timings.Count == usablePatterns[0].Timings.Count).ToList();

                    var patternToUse = usablePatterns[random.Next(usablePatterns.Count)];
                    for (int j = 0; j < patternToUse.Directions.Count; j++)
                    {
                        objectMap[i + j].Direction = patternToUse.Directions[j];
                    }
                    i += patternToUse.Timings.Count;
                }
                else
                {
                    // Recheck if the next note is just a not handled large time space
                    foreach (var pattern in patterns)
                    {
                        var canUsePattern = true;
                        for (int j = 0; j < pattern.Timings.Count; j++)
                        {
                            if ((objectMap.Count <= i + j || objectMap[i + j].Timing != pattern.Timings[j]) && !(j == 0 && pattern.Timings[j] == 1))
                            {
                                canUsePattern = false;
                                break;
                            }
                        }
                        if (canUsePattern)
                            usablePatterns.Add(pattern);
                    }

                    if (usablePatterns.Count > 0)
                    {
                        usablePatterns = usablePatterns.OrderByDescending(pattern => pattern.Timings.Count).ToList();
                        usablePatterns = usablePatterns.Where(pattern => pattern.Timings.Count == usablePatterns[0].Timings.Count).ToList();

                        var patternToUse = usablePatterns[random.Next(usablePatterns.Count)];
                        for (int j = 0; j < patternToUse.Directions.Count; j++)
                        {
                            objectMap[i + j].Direction = patternToUse.Directions[j];
                        }
                        i += patternToUse.Timings.Count;
                    }
                    else
                    {
                        objectMap[i].Direction = 0;
                        i++;
                    }
                }
            }

            return objectMap;
        }

        private static void CreateMeasures(List<ObjectMap> objects, Beatmap beatmap, Simfile simfile)
        {
            for(int i = 0; i < objects.Count; i++)
            {
                var hitObject = objects[i].CircleObject;
                double hitObjectTime = (hitObject.StartTime - objects[0].CircleObject.StartTime) / 1000f;

                int measureNumber;
                int noteNumberInMeasure;
                GetMeasureAndDivisionAt(hitObjectTime, simfile.BPMS, out measureNumber, out noteNumberInMeasure);

                if (noteNumberInMeasure == 192)
                {
                    noteNumberInMeasure = 0;
                    measureNumber++;
                }

                var hitObjectEnd = -1d;
                var endMeasureNumber = -1;
                var endNoteNumberInMeasure = -1;

                if ((hitObject.Type & HitObjectType.Slider) == HitObjectType.Slider)
                {
                    var slider = (SliderObject)hitObject;
                    hitObjectEnd = (slider.EndTime(beatmap) - objects[0].CircleObject.StartTime) / 1000f;
                    GetMeasureAndDivisionAt(hitObjectEnd, simfile.BPMS, out endMeasureNumber, out endNoteNumberInMeasure);

                    if (endNoteNumberInMeasure == 192)
                    {
                        endNoteNumberInMeasure = 0;
                        endMeasureNumber++;
                    }
                }

                while (measureNumber >= simfile.Notes.Count || (endMeasureNumber > measureNumber && endMeasureNumber >= simfile.Notes.Count) || (hitObjectTime == 0 && simfile.Notes.Count == 0))
                {
                    simfile.Notes.Add(new Measure());
                }

                var measure = simfile.Notes[measureNumber];
                Measure endMeasure = null;

                if (endMeasureNumber != -1)
                {
                    endMeasure = simfile.Notes[endMeasureNumber];
                }

                var column = 0;

                if (beatmap.Mode == GameMode.osu)
                {
                    /*var screenPartWidth = 512 / 4;
                    column = (int)(Math.Max(Math.Min(hitObject.Location.X / screenPartWidth, 3), 0));*/
                    column = objects[i].Direction;
                }
                else
                {
                    if (hitObject.Location.X <= 64)
                        column = 1;
                    else if (hitObject.Location.X <= 192)
                        column = 2;
                    else if (hitObject.Location.X <= 320)
                        column = 4;
                    else
                        column = 8;
                }

                if (endMeasureNumber == -1)
                {
                    if ((column & 1) > 0)
                        measure[noteNumberInMeasure][0] = '1';
                    if ((column & 2) > 0)
                        measure[noteNumberInMeasure][1] = '1';
                    if ((column & 4) > 0)
                        measure[noteNumberInMeasure][2] = '1';
                    if ((column & 8) > 0)
                        measure[noteNumberInMeasure][3] = '1';
                }
                else
                {
                    if ((column & 1) > 0)
                    {
                        measure[noteNumberInMeasure][0] = '2';
                        measure[endNoteNumberInMeasure][0] = '3';
                    }
                    if ((column & 2) > 0)
                    {
                        measure[noteNumberInMeasure][1] = '2';
                        measure[endNoteNumberInMeasure][1] = '3';
                    }
                    if ((column & 4) > 0)
                    {
                        measure[noteNumberInMeasure][2] = '2';
                        measure[endNoteNumberInMeasure][2] = '3';
                    }
                    if ((column & 8) > 0)
                    {
                        measure[noteNumberInMeasure][3] = '2';
                        measure[endNoteNumberInMeasure][3] = '3';
                    }
                }
            }
        }

        private static string GetOutputSm(Simfile simfile)
        {
            StringWriter writer = new StringWriter();

            writer.WriteLine($"#ARTIST:{simfile.Artist};");
            writer.WriteLine($"#ARTISTTRANSLIT:{simfile.ArtistTranslit};");
            writer.WriteLine($"#BACKGROUND:{simfile.Background};");
            writer.WriteLine($"#BANNER:{simfile.Banner};");

            writer.Write("#BPMS:");
            for (int i = 0; i < simfile.BPMS.Count; i++)
            {
                writer.Write($"{simfile.BPMS[i].v1}={simfile.BPMS[i].v2}{(i < simfile.BPMS.Count - 1 ? "," : "")}");
            }
            writer.WriteLine(";");

            writer.WriteLine($"#CDTITLE:{simfile.CDTitle};");
            writer.WriteLine($"#CREDIT:{simfile.Credit};");
            writer.WriteLine($"#MUSIC:{simfile.Music};");
            writer.WriteLine($"#OFFSET:{simfile.Offset};");
            writer.WriteLine($"#SAMPLELENGTH:{simfile.SampleLength};");
            writer.WriteLine($"#SAMPLESTART:{simfile.SampleStart};");
            writer.WriteLine($"#SELECTABLE:{(simfile.Selectable ? "YES" : "NO")};");

            writer.Write("#STOPS:");
            for (int i = 0; i < simfile.Stops.Count; i++)
            {
                writer.Write($"{simfile.Stops[i].v1}={simfile.Stops[i].v2}{(i < simfile.Stops.Count - 1 ? "," : "")}");
            }
            writer.WriteLine(";");

            writer.WriteLine($"#SUBTITLE:{simfile.SubTitle};");
            writer.WriteLine($"#SUBTITLETRANSLIT:{simfile.SubTitleTranslit};");
            writer.WriteLine($"#TITLE:{simfile.Title};");
            writer.WriteLine($"#TITLETRANSLIT:{simfile.TitleTranslit};");

            writer.WriteLine($"#NOTES:");
            writer.WriteLine($"    dance-single:");
            writer.WriteLine($"    {simfile.MapCreator}:");
            writer.WriteLine($"    Challenge:");
            writer.WriteLine($"    10:");
            writer.WriteLine($"    1.000,1.000,1.000,1.000,1.000:");
            for (int i = 0; i < simfile.Notes.Count; i++)
            {
                var linesToWrite = simfile.Notes[i].MostAdaptedNumberOfLines;
                for (int j = 0; j < linesToWrite; j++)
                {
                    int line = (192 / linesToWrite) * j;
                    if (simfile.Notes[i].IsNoteNull(line))
                        writer.WriteLine("0000");
                    else
                        writer.WriteLine($"{simfile.Notes[i][line][0]}{simfile.Notes[i][line][1]}{simfile.Notes[i][line][2]}{simfile.Notes[i][line][3]}");
                }
                writer.WriteLine(i < simfile.Notes.Count - 1 ? "," : "");
            }
            writer.WriteLine(";");

            return writer.ToString();
        }
    }
}
