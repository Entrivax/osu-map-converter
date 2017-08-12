using BMAPI.v1;
using BMAPI.v1.HitObjects;
using osu.Map.Converter.StepmaniaObjects;
using osu.Map.Converter.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace osu.Map.Converter
{
    public class StepmaniaConverter
    {
        public static void ConvertToStepmania(Beatmap beatmap, string osuInput, string stepmaniaDir)
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

            double lastNonInheritedBPM = 0;
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
            }

            CreateMeasures(beatmap, simfile);

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
            if (bpms.Count == 1)
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

        private static void CreateMeasures(Beatmap beatmap, Simfile simfile)
        {
            for(int i = 0; i < beatmap.HitObjects.Count; i++)
            {
                var hitObject = beatmap.HitObjects[i];
                double hitObjectTime = (hitObject.StartTime - beatmap.HitObjects[0].StartTime) / 1000f;

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
                    hitObjectEnd = (slider.EndTime(beatmap) - beatmap.HitObjects[0].StartTime) / 1000f;
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
                    var screenPartWidth = 512 / 4;
                    column = (int)(Math.Max(Math.Min(hitObject.Location.X / screenPartWidth, 3), 0));
                }
                else
                {
                    if (hitObject.Location.X <= 64)
                        column = 0;
                    else if (hitObject.Location.X <= 192)
                        column = 1;
                    else if (hitObject.Location.X <= 320)
                        column = 2;
                    else
                        column = 3;
                }

                if (endMeasureNumber == -1)
                {
                    measure[noteNumberInMeasure][column] = '1';
                }
                else
                {
                    measure[noteNumberInMeasure][column] = '2';
                    measure[endNoteNumberInMeasure][column] = '3';
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
