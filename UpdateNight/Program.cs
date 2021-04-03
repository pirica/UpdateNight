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
using UpdateNight.TocReader.Parsers.PropertyTagData;
using UpdateNight.Source.Utils;
using static UpdateNight.Source.Utils.Files;

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

            bool Force = false;

            Console.WriteLine();
            Console.Write("Please enter the fortnite build to compare: ");
            string old_version = Console.ReadLine();
            if (old_version.Length == 44) old_version = old_version.Substring(19, 5);
            else if (old_version == "force") Force = true;
            else if (old_version.Length != 5) Global.Exit(0, "Invalid build format!");
            Console.WriteLine();

            if (Force)
            {
                ManifestInfo info = await Grabbers.ManifestGrabber.GrabInfo();
                old_version = info.BuildVersion.Substring(19, 5);
            }

            Global.Check();

            // Wait until new update
            bool newVersion = false;
            while (!newVersion)
            {
                ManifestInfo info = await Grabbers.ManifestGrabber.GrabInfo();
                if (Force || info.BuildVersion.Substring(19, 5) != old_version)
                {
                    newVersion = true;
                    Global.Version = info.BuildVersion;

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
                var mappings = await Grabbers.MappingsGrabber.GrabInfo();
                var mapping = mappings.FirstOrDefault(m => m.Meta.CompressionMethod == "Oodle" && m.Meta.Version == Global.Version);
                if (mapping != null)
                {
                    newMapping = true;
                    Console.Write($"[{Global.BuildTime()}] ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("[Mapping Grabber] ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Grabbed mappings for ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(Global.Version);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine();
                    await Grabbers.MappingsGrabber.Grab(mapping);
                }
                if (!newMapping) Thread.Sleep(1000);
            }

            Start = DateTime.UtcNow;

            // Grabbers
            Manifest manifest = await Grabbers.ManifestGrabber.Grab();
            Global.CreateOut();

            await Grabbers.Toc.GrabTocsAsync(manifest);

            // Pre loads
            Image.PreLoad();
            await Localization.PreLoadAsync();

            Console.WriteLine();

            // File Comparision
            NewFiles = GetNewFiles(Force, old_version);
            Console.WriteLine();

            // Functions
            await GetCosmetics();
            await GetChallenges();
            await GetWeapons();
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
            List<string> Files = NewFiles.Where(f =>
                f.StartsWith("/FortniteGame/Content/Athena/Items/Cosmetics/")
                || f.StartsWith("/FortniteGame/Content/Athena/Items/CosmeticVariantTokens/")).ToList();

            Global.Print(ConsoleColor.Green, "Cosmetic Manager", $"{(Files.Count < 1 ? "No" : Files.Count.ToString())} new cosmetics{(Files.Count < 1 ? "\n" : "")}");
            if (Files.Count < 1) return Task.CompletedTask;

            Console.WriteLine();

            foreach (string path in Files)
            {
                if (path.Contains("/Series/")) continue;

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

            List<string> types = CosmeticsData.Where(c => !string.IsNullOrEmpty(c.Type)).Select(c => c.Type).ToList();
            types = types.Distinct().ToList();
            foreach (string type in types)
            {
                List<Cosmetic> data = CosmeticsData.Where(c => c.Type == type).ToList();
                Image.Collage(data.OrderBy(c => c.Rarity).ThenBy(c => c.Name).Select(c => c.Canvas).ToArray(), type);
            }

            List<string> sets = CosmeticsData.Where(c => !string.IsNullOrEmpty(c.Set)).Select(c => c.Set).ToList();
            sets = sets.Distinct().ToList();
            foreach (string set in sets)
            {
                List<Cosmetic> data = CosmeticsData.Where(c => c.Set == set).ToList();
                Image.Collage(data.OrderBy(c => c.Rarity).ThenBy(c => c.Name).Select(c => c.Canvas).ToArray(), set);
            }

            Image.Collage(CosmeticsData.OrderBy(c => c.Rarity).ThenBy(c => c.Name).Select(c => c.Canvas).ToArray(), "All");

            Console.WriteLine();

            return Task.CompletedTask;
        }

        static Task GetChallenges()
        {
            List<string> Files = NewFiles.Where(f =>
                f.StartsWith("/FortniteGame/Content/Athena/Items/ChallengeBundles/")
                || (f.StartsWith("/FortniteGame/Plugins/GameFeatures/BattlepassS") && f.Contains("/Content/Items/QuestBundles/"))).ToList();

            Global.Print(ConsoleColor.Green, "Challenge Manager", $"{(Files.Count < 1 ? "No" : Files.Count.ToString())} new challenges{(Files.Count < 1 ? "\n" : "")}");
            if (Files.Count < 1) return Task.CompletedTask;
            Console.WriteLine();

            foreach (string path in Files)
            {
                IoPackage asset = Toc.GetAsset(path);
                if (asset == null)
                {
                    Global.Print(ConsoleColor.Red, "Error", $"Could not get the asset for {path.Split("/").Last()}");
                    continue;
                }

                Challenge challenge = new Challenge(asset, path);
                Image.Challenge(challenge); // not implemented yet
            }

            return Task.CompletedTask;
        }

        static Task GetWeapons()
        {
            List<string> Files = NewFiles.Where(f =>
                f.StartsWith("/FortniteGame/Plugins/GameFeatures/NightmareGameplay/Content/Items/") && f.Contains("WID")).ToList();

            Global.Print(ConsoleColor.Green, "Weapon Manager", $"{(Files.Count < 1 ? "No" : Files.Count.ToString())} new weapons{(Files.Count < 1 ? "\n" : "")}");
            if (Files.Count < 1) return Task.CompletedTask;
            Console.WriteLine();

            foreach (string path in Files)
            {
                IoPackage asset = Toc.GetAsset(path);
                if (asset == null)
                {
                    Global.Print(ConsoleColor.Red, "Error", $"Could not get the asset for {path.Split("/").Last()}");
                    continue;
                }

                Weapon weapon = new Weapon(asset, path);
                Image.Weapon(weapon);
            }

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
            var stream = File.OpenWrite(Path.Combine(Global.OutPath, "map.png"));
            data.SaveTo(stream);
            stream.Close();

            Global.Print(ConsoleColor.Green, "Map", "Saved map image");

            asset = Toc.GetAsset("/FortniteGame/Content/Quests/QuestIndicatorData");
            if (asset == null)
            {
                Global.Print(ConsoleColor.Red, "Error", "Could not get the asset for POIs");
                return Task.CompletedTask;
            }

            IUExport export = asset.Exports[0];
            List<POI> Pois = new List<POI>();

            if (export.GetExport<ArrayProperty>("ChallengeMapPoiData") is { } adata)
                foreach (var bdata in adata.Value)
                    if (bdata is StructProperty cdata && cdata.Value is UObject info)
                    {
                        POI Poi = new POI(info);
                        if (Poi.Tag.Contains("Athena.Location.POI.Papaya.") || Poi.CalendarEventsRequired.Count >= 1) continue;
                        if (Poi.Name == "Quadcrasher Motocross" || Poi.Name == "Soccer Pitch") continue;
                        Pois.Add(Poi);
                    }

            Image.Map(image, Pois);
            Console.WriteLine();

            return Task.CompletedTask;
        }

        static Task ExtractUi()
        {
            List<string> Files = NewFiles.Where(f =>
                f.StartsWith("/FortniteGame/Content/UI/Foundation/")
                || f.StartsWith("/FortniteGame/Content/2dAssets/")).ToList();

            Global.Print(ConsoleColor.Green, "UI Manager", $"{(Files.Count < 1 ? "No" : Files.Count.ToString())} new textures{(Files.Count < 1 ? "\n" : "")}");
            if (Files.Count < 1) return Task.CompletedTask;

            foreach (string path in Files)
            {
                IoPackage asset = Toc.GetAsset(path);
                if (asset == null)
                {
                    Global.Print(ConsoleColor.Red, "Error", $"Could not get the asset for {path.Split("/").Last()}");
                    continue;
                }

                if (!asset.ExportTypes.Any(e => e.String == "Texture2D"))
                {
                    Global.Print(ConsoleColor.Red, "Error", $"{path.Split("/").Last()} asset does not have a Texture2D");
                    continue;
                }

                UTexture2D texture = asset.GetExport<UTexture2D>();
                SKImage image = texture.Image;

                var data = image.Encode(SKEncodedImageFormat.Png, 100);
                var stream = File.OpenWrite(Path.Combine(Global.OutPath, "ui", $"{path.Split("/").Last()}.png"));
                data.SaveTo(stream);
                stream.Close();

                Global.Print(ConsoleColor.Green, "UI Manager", $"Saved image of {path.Split("/").Last()}");
            }

            return Task.CompletedTask;
        }
    }
}
