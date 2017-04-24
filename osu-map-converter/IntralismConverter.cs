using BMAPI.v1;
using BMAPI.v1.Events;
using Newtonsoft.Json;
using osu.Map.Converter.IntralismObjects;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace osu.Map.Converter
{
    public class IntralismConverter
    {
        public static void ConvertOsuToIntralism(Beatmap beatmap, string osuInput, string intralismDir)
        {
            Level level = new Level()
            {
                Id = beatmap.BeatmapID.ToString()
            };
            string mapDir = intralismDir + beatmap.BeatmapID;

            level.Name = $"{beatmap.Artist} - {beatmap.Title} [{beatmap.Version}]";
            level.Description = $"Level by {beatmap.Creator} converted from osu! with osu!MapConverter";
            level.MoreInfoURL = $"http://osu.ppy.sh/s/{beatmap.BeatmapSetID}";
            level.MusicFile = "music.ogg";

            for (int i = 0; i < beatmap.Events.Count; i++)
            {
                if (beatmap.Events[i].GetType() == typeof(ContentEvent))
                {
                    ContentEvent ce = (ContentEvent)beatmap.Events[i];
                    if (ce.Type == ContentType.Image && ce.StartTime == 0)
                    {
                        level.Resources = new[] { new Resource("bg", Resource.ResourceType.Sprite, ce.Filename) };
                        level.IconFile = "icon.jpg";
                        level.Events.Add(new ShowSpriteEvent(level.Resources[0], 0, beatmap.HitObjects[beatmap.HitObjects.Count - 1].StartTime / 1000 + 1, true, 0.01f));
                        break;
                    }
                }
            }

            Random rand = new Random();
            for (int i = 0; i < beatmap.HitObjects.Count; i++)
            {
                SpawnObjEvent.Object obj;
                if (beatmap.HitObjects[i].Location.X <= 64)
                    obj = SpawnObjEvent.Object.Left;
                else if (beatmap.HitObjects[i].Location.X <= 192)
                    obj = SpawnObjEvent.Object.Down;
                else if (beatmap.HitObjects[i].Location.X > 192 && beatmap.HitObjects[i].Location.X <= 320)
                    obj = SpawnObjEvent.Object.Up;
                else if (beatmap.HitObjects[i].Location.X > 320)
                    obj = SpawnObjEvent.Object.Right;
                else
                {
                    switch (rand.Next(0, 3))
                    {
                        case 0:
                            obj = SpawnObjEvent.Object.Left;
                            break;
                        case 1:
                            obj = SpawnObjEvent.Object.Down;
                            break;
                        case 2:
                            obj = SpawnObjEvent.Object.Up;
                            break;
                        case 3:
                        default:
                            obj = SpawnObjEvent.Object.Right;
                            break;
                    }
                }
                float myeventtime = beatmap.HitObjects[i].StartTime / 1000;

                int index = -1;
                if ((index = level.Events.FindIndex((ev) => { return (ev.Time == myeventtime && ev.GetType() == typeof(SpawnObjEvent)); })) > 0)
                    ((SpawnObjEvent)level.Events[index]).AddObject(obj);
                else
                    level.Events.Add(new SpawnObjEvent(myeventtime, obj));
            }

            Console.WriteLine("Creating map directory...");
            if (Directory.Exists(mapDir))
                Directory.Delete(mapDir, true);
            Directory.CreateDirectory(mapDir);

            Console.WriteLine("Exporting resources...");
            foreach (Resource res in level.Resources)
            {
                if (res.Name == "bg")
                {
                    Bitmap bitmap = new Bitmap(Path.GetDirectoryName(osuInput) + "/" + res.Path);

                    int size = Math.Min(bitmap.Width, bitmap.Height);

                    Bitmap icon = new Bitmap(1024, 1024, bitmap.PixelFormat);
                    using (Graphics g = Graphics.FromImage(icon))
                    {
                        g.DrawImage(bitmap, new Rectangle(0, 0, 1024, 1024), new Rectangle(bitmap.Width / 2 - size / 2, bitmap.Height / 2 - size / 2, size, size), GraphicsUnit.Pixel);
                    }

                    ImageCodecInfo jpgEncoder = Array.Find(ImageCodecInfo.GetImageDecoders(), (ImageCodecInfo dec) => { return (dec.FormatID == ImageFormat.Jpeg.Guid); });
                    EncoderParameters encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 95L);

                    icon.Save(mapDir + "/icon.jpg", jpgEncoder, encoderParams);

                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        Brush b = new SolidBrush(System.Drawing.Color.FromArgb(100, 0, 0, 0));
                        g.FillRectangle(b, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                    }
                    bitmap.Save(mapDir + "/" + res.Path);
                }
                else
                    File.Copy(Path.GetDirectoryName(osuInput) + "/" + res.Path, mapDir + "/" + res.Path);
            }

            Console.WriteLine("Exporting level...");
            File.WriteAllText(mapDir + "/config.txt", JsonConvert.SerializeObject(level, Formatting.Indented));

            Console.WriteLine("Exporting audio...");
            ConvertToOGG(Path.GetDirectoryName(osuInput) + "/" + beatmap.AudioFilename, mapDir + "/music.ogg");

            Console.WriteLine("Exportation finished!");
        }

        public static void ConvertToOGG(string input, string output)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo("ffmpeg.exe", $"-y -i \"{input}\" -vn -c:a libvorbis -q:a 4 \"{output}\"")
            {
                CreateNoWindow = false,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Normal
            };
            try
            {
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }
    }
}
