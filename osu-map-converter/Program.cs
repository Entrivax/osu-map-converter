using BMAPI.v1;
using System;
using System.IO;

namespace osu.Map.Converter
{
    class Program
    {
        static ConsoleColor defaultColor;

        static void Main(string[] args)
        {
            string pathToStepmania = "";

            Console.WriteLine("Stepmania folder: ");
            pathToStepmania = Console.ReadLine();

            while (!Directory.Exists(pathToStepmania + "/Songs/"))
            {
                Console.WriteLine("Invalid Stepmania path!");
                Console.WriteLine("Stepmania folder: ");
                pathToStepmania = Console.ReadLine();
            }

            while (true)
            {
                Console.WriteLine(".osu file to convert: ");
                string pathToOsuFile = Console.ReadLine();

                Beatmap beatmap = new Beatmap(pathToOsuFile);

                Console.WriteLine(Environment.NewLine + "Beatmap infos:");
                WriteLineColor(new Tuple<string, ConsoleColor?>("Title: ", null), new Tuple<string, ConsoleColor?>(beatmap.Title, ConsoleColor.Cyan));
                WriteLineColor(new Tuple<string, ConsoleColor?>("Artist: ", null), new Tuple<string, ConsoleColor?>(beatmap.Artist, ConsoleColor.Cyan));
                WriteLineColor(new Tuple<string, ConsoleColor?>("Creator: ", null), new Tuple<string, ConsoleColor?>(beatmap.Creator, ConsoleColor.Cyan));
                Console.WriteLine();
                WriteLineColor(new Tuple<string, ConsoleColor?>("Difficulty: ", null), new Tuple<string, ConsoleColor?>(beatmap.Version, ConsoleColor.Cyan));
                WriteLineColor(new Tuple<string, ConsoleColor?>("Mode: ", null), new Tuple<string, ConsoleColor?>(beatmap.Mode.ToString(), ConsoleColor.Cyan));
                Console.WriteLine();

                bool reset = false;
                while (true)
                {
                    WriteColor(new Tuple<string, ConsoleColor?>("Are you sure to convert this beatmap? ", null), new Tuple<string, ConsoleColor?>("(y/n) ", ConsoleColor.White));
                    ConsoleKeyInfo answer = Console.ReadKey(false);
                    Console.WriteLine();
                    if (answer.KeyChar == 'y')
                    {
                        reset = false;
                        break;
                    }
                    else if (answer.KeyChar == 'n')
                    {
                        reset = true;
                        break;
                    }
                }
                if (reset)
                    continue;

                StepmaniaConverter.ConvertToStepmania(beatmap, "./patterns.json", pathToOsuFile, pathToStepmania);
            }
        }

        static void Main2(string[] args)
        {
            defaultColor = Console.ForegroundColor;
            Console.Write(  "       .:///////:.       "+ Environment.NewLine +
                            "     ./////////////.     "+ Environment.NewLine +
                            "   ://' '/' '/'/'/ //:   "+ Environment.NewLine +
                            "  ./// / /. // / / ///.  "+ Environment.NewLine +
                            "  '/// / // '/ / /////'  "+ Environment.NewLine +
                            "   ://. ./. ./. ./ //:   "+ Environment.NewLine +
                            "     '/////////////'     "+ Environment.NewLine +
                            "       ':///////:'       "+ Environment.NewLine);

            string pathToIntralism = "";

            if (File.Exists("config.txt"))
                pathToIntralism = File.ReadAllText("config.txt");
            else
            {
                Console.WriteLine("Intralism folder: ");
                pathToIntralism = Console.ReadLine();
            }
            while (!Directory.Exists(pathToIntralism + "/Editor/"))
            {
                Console.WriteLine("Invalid Intralism path!");
                Console.WriteLine("Intralism folder: ");
                pathToIntralism = Console.ReadLine();
            }
            File.WriteAllText("config.txt", pathToIntralism);
            pathToIntralism += "/Editor/";

            while (true)
            {
                Console.WriteLine(".osu file to convert: ");
                string pathToOsuFile = Console.ReadLine();

                Beatmap beatmap = new Beatmap(pathToOsuFile);

                Console.WriteLine(Environment.NewLine + "Beatmap infos:");
                WriteLineColor(new Tuple<string, ConsoleColor?>("Title: ", null), new Tuple<string, ConsoleColor?>(beatmap.Title, ConsoleColor.Cyan));
                WriteLineColor(new Tuple<string, ConsoleColor?>("Artist: ", null), new Tuple<string, ConsoleColor?>(beatmap.Artist, ConsoleColor.Cyan));
                WriteLineColor(new Tuple<string, ConsoleColor?>("Creator: ", null), new Tuple<string, ConsoleColor?>(beatmap.Creator, ConsoleColor.Cyan));
                Console.WriteLine();
                WriteLineColor(new Tuple<string, ConsoleColor?>("Difficulty: ", null), new Tuple<string, ConsoleColor?>(beatmap.Version, ConsoleColor.Cyan));
                WriteLineColor(new Tuple<string, ConsoleColor?>("Mode: ", null), new Tuple<string, ConsoleColor?>(beatmap.Mode.ToString(), ConsoleColor.Cyan));
                Console.WriteLine();


                string mapDir = pathToIntralism + beatmap.BeatmapID;

                Console.ForegroundColor = ConsoleColor.Yellow;
                if (Directory.Exists(mapDir))
                    Console.WriteLine("Warning: this level already exists!");
                Console.ForegroundColor = defaultColor;

                bool reset = false;
                while (true)
                {
                    WriteColor(new Tuple<string, ConsoleColor?>("Are you sure to convert this beatmap? ", null), new Tuple<string, ConsoleColor?>("(y/n) ", ConsoleColor.White));
                    ConsoleKeyInfo answer = Console.ReadKey(false);
                    Console.WriteLine();
                    if (answer.KeyChar == 'y')
                    {
                        reset = false;
                        break;
                    }
                    else if (answer.KeyChar == 'n')
                    {
                        reset = true;
                        break;
                    }
                }
                if (reset)
                    continue;

                IntralismConverter.ConvertOsuToIntralism(beatmap, pathToOsuFile, pathToIntralism);
            }
        }

        public static void WriteColor(params Tuple<string, ConsoleColor?>[] str)
        {
            ConsoleColor baseColor = Console.ForegroundColor;

            foreach (Tuple<string, ConsoleColor?> s in str)
            {
                if (s.Item2 != null)
                    Console.ForegroundColor = (ConsoleColor)s.Item2;
                Console.Write(s.Item1);
            }

            Console.ForegroundColor = baseColor;
        }

        public static void WriteLineColor(params Tuple<string, ConsoleColor?>[] str)
        {
            ConsoleColor baseColor = Console.ForegroundColor;

            foreach (Tuple<string, ConsoleColor?> s in str)
            {
                if (s.Item2 != null)
                    Console.ForegroundColor = (ConsoleColor)s.Item2;
                Console.Write(s.Item1);
            }

            Console.ForegroundColor = baseColor;

            Console.WriteLine();
        }
    }
}
