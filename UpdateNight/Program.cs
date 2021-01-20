using System;
using System.Threading.Tasks;
using EpicManifestParser.Objects;
using System.Linq;
using System.Reflection;
using System.Threading;
using UpdateNight.TocReader.IO;
using UpdateNight.TocReader.Parsers.Class;
using SkiaSharp;
using System.IO;
using System.Collections.Generic;
using UpdateNight.Source;
using UpdateNight.Source.Models;

namespace UpdateNight
{
    class Program
    {
        public static DateTime Start;
        public static DateTime End;

        public static List<string> Files = new List<string>();
        public static List<string> NewFiles = new List<string>();

        static async Task Main()
        {
            // Start
            Global.Print(ConsoleColor.Magenta, "Update Night", $"Started Update Night v{Assembly.GetExecutingAssembly().GetName().Version} made by Command");

            Console.WriteLine();
            Console.Write("Please enter the fortnite build to compare: ");
            string old_version = Console.ReadLine();
            if (old_version.Length == 44) old_version = old_version.Substring(19, 5);
            else if (old_version.Length != 5) Global.Exit(0, "Invalid build format!");
            Console.WriteLine();

            Global.Check(old_version);

            // Wait until new update
            bool newVersion = false;
            while (!newVersion)
            {
                ManifestInfo info = await Grabbers.ManifestGrabber.GrabInfo();
                if (info.BuildVersion.Substring(19, 5) != old_version)
                {
                    newVersion = true;
                    Global.version = info.BuildVersion;

                    Console.Write($"[{Global.BuildTime()}] ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("[Manifest Grabber] ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("New manifest detected ");
#if DEBUG
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(info.Uri.ToString().Split("/").Last().Split(".").First());
                    Console.ForegroundColor = ConsoleColor.White;
#endif
                    Console.WriteLine();
                }
                if (!newVersion) Thread.Sleep(1000);
            }

            bool newMapping = false;
            while (!newMapping)
            {
                // TODO: make this better
                Grabbers.Mapping[] mappings = await Grabbers.MappingsGrabber.GrabInfo();
                Grabbers.Mapping mapping = mappings.FirstOrDefault(m => m.Meta.CompressionMethod == "Oodle" && m.Meta.Version == Global.version);
                if (mapping != null)
                {
                    newMapping = true;
                    Console.Write($"[{Global.BuildTime()}] ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("[Mapping Grabber] ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Grabbed mappings for ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(Global.version);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine();
                    Grabbers.MappingsGrabber.mapping = mapping;
                    await Grabbers.MappingsGrabber.Grab();
                }
                if (!newMapping) Thread.Sleep(1000);
            }
            
            Start = DateTime.UtcNow;

            // Grabbers
            Manifest manifest = await Grabbers.ManifestGrabber.Grab();
            Global.CreateOut();

            await Grabbers.Toc.GrabTocsAsync(manifest);

            Image.PreLoad();

            // File Comparision
            List<string> OldFiles = (await File.ReadAllLinesAsync(Path.Combine(Global.current_path, "out", old_version.Replace(".", "_"), "files.txt"))).ToList();
            NewFiles = Source.Utils.GetNewFiles(OldFiles, Source.Utils.BuildFileList());
            await File.WriteAllLinesAsync(Path.Combine(Global.current_path, "out", Global.version.Substring(19, 5).Replace(".", "_"), "files.txt"), Source.Utils.BuildFileList());

            // Functions
            await GetCosmetics();
            await GetMap();
            await ExtractUi();

            // End program
            End = DateTime.UtcNow;
            TimeSpan dur = End.Subtract(Start);
            Console.WriteLine();
            Global.Exit(0, $"Finished Update Night in {dur.ToString("T").Replace(",", ".")}");
        }

        static Task GetCosmetics()
        {
            List<Cosmetic> CosmeticsData = new List<Cosmetic>();
            foreach (string path in NewFiles)
            {
                if (!(path.StartsWith("/FortniteGame/Content/Athena/Items/Cosmetics/") ||
                      path.StartsWith("/FortniteGame/Content/Athena/Items/CosmeticVariantTokens/"))) continue;
                if (path.Contains("Series")) continue;

                IoPackage asset = Toc.GetAsset(path);
                if (asset == null)
                {
                    Global.Print(ConsoleColor.Red, "Error", $"Could not get the asset for {path.Split("/").Last()}");
                    continue;
                }
                
                Cosmetic cosmetic = new Cosmetic(asset, path);
                Image.Cosmetic(cosmetic);
                CosmeticsData.Add(cosmetic);
            }

            Console.WriteLine();
            List<string> types = CosmeticsData.Select(c => c.Type).ToList();
            types = types.Distinct().ToList();
            foreach (string type in types)
            {
                List<Cosmetic> data = CosmeticsData.Where(c => c.Type == type).ToList();
                data = data.OrderBy(c => c.Name).ThenBy(c => c.Rarity)
                            .ThenBy(c => Source.Utils.BuildRarity(c.Rarity)).ThenBy(c => c.Type).ToList();
                Image.Collage(data.Select(c => c.Canvas).ToArray(), type);
            }
            
            Image.Collage(CosmeticsData.OrderBy(c => c.Name).ThenBy(c => c.Rarity)
                .ThenBy(c => Source.Utils.BuildRarity(c.Rarity)).ThenBy(c => c.Type)
                .Select(c => c.Canvas).ToArray(), "All");
            
            Console.WriteLine();

            return Task.CompletedTask;
        }

        static Task GetMap()
        {
            IoPackage asset = Toc.GetAsset("/FortniteGame/Content/Athena/Apollo/Maps/UI/Apollo_Terrain_Minimap");
            if (asset == null)
            {
                Global.Print(ConsoleColor.Red, "Error", "Could not get the asset for the Map");
                return Task.CompletedTask;
            }

            if (asset.ExportTypes.All(e => e.String != "Texture2D"))
            {
                Global.Print(ConsoleColor.Red, "Error", "Map asset does not have a Texture2D");
                return Task.CompletedTask;
            }

            UTexture2D texture = asset.GetExport<UTexture2D>();
            SKImage image = texture.Image;

            var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var stream = File.OpenWrite(Path.Combine(Global.current_path, "out", Global.version.Substring(19, 5).Replace(".", "_"), "map.png"));
            data.SaveTo(stream);
            stream.Close();

            Global.Print(ConsoleColor.Green, "Map", "Saved map image");
            Console.WriteLine();

            return Task.CompletedTask;
        }

        static Task ExtractUi()
        {
            foreach (string path in NewFiles)
            {
                if (!(path.StartsWith("/FortniteGame/Content/UI/Foundation/") ||
                      path.StartsWith("/FortniteGame/Content/2dAssets/"))) continue;
                IoPackage asset = Toc.GetAsset(path);
                if (asset == null)
                {
                    Global.Print(ConsoleColor.Red, "Error", $"Could not get the asset for {path.Split("/").Last()}");
                    return Task.CompletedTask;
                }
                
                if (asset.ExportTypes.All(e => e.String != "Texture2D"))
                {
                    Global.Print(ConsoleColor.Red, "Error", $"{path.Split("/").Last()} asset does not have a Texture2D");
                    return Task.CompletedTask;
                }

                UTexture2D texture = asset.GetExport<UTexture2D>();
                SKImage image = texture.Image;
                
                var data = image.Encode(SKEncodedImageFormat.Png, 100);
                var stream = File.OpenWrite(Path.Combine(Global.current_path, "out", Global.version.Substring(19, 5).Replace(".", "_"), "ui", $"{path.Split("/").Last()}.png"));
                data.SaveTo(stream);
                stream.Close();
                
                Global.Print(ConsoleColor.Green, "UI Manager", $"Saved image of {path.Split("/").Last()}");
            }
            
            
            return Task.CompletedTask;
        }
    }
}
