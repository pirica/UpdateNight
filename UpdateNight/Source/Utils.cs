using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UpdateNight.TocReader.Parsers.PropertyTagData;
using UpdateNight.TocReader.IO;
using UpdateNight.TocReader.Parsers.Class;
using UpdateNight.TocReader.Parsers.Objects;

namespace UpdateNight.Source
{
    class Utils
    {
        public static List<string> GetNewFiles(List<string> old, List<string> current) => current.Except(old).ToList();
        public static List<string> BuildFileList() => Global.assetmapping.Keys.ToList();

        public static int BuildRarity(string rarity)
        {
            if (rarity == "Legendary") return 6;
            if (rarity == "Epic") return 4;
            if (rarity == "Rare") return 3;
            if (rarity == "Uncommon") return 2;
            if (rarity == "Common") return 1;
            return 5;
        }

        public static string BuildSource(string source)
        {
            if (source.StartsWith("Cosmetics.Source.Season"))
            {
                string tag = Regex.Replace(source, @"[0-9]", "");
                if (Localization.SourceNames.TryGetValue(tag, out var text))
                    return text;
            }

            if (source.StartsWith("Cosmetics.Source.Platform.")) return source.Replace("Cosmetics.Source.Platform.", "");
            if (source.StartsWith("Cosmetics.Source.RMT.")) return source.Replace("Cosmetics.Source.RMT.", "");

            return Regex.Replace(source.Replace("Cosmetics.Source.", ""), "[.]", " ");
        }

        public static class Localization
        {
            public static Dictionary<string, Dictionary<string, string>> _cachedValues = new Dictionary<string, Dictionary<string, string>>();
            public static Dictionary<string, string> SourceNames = new Dictionary<string, string>();
            public static Dictionary<string, string> SetsName = new Dictionary<string, string>();

            public static void PreLoad()
            {
                // Cosmetics sets
                IoPackage asset = Toc.GetAsset("/FortniteGame/Content/Athena/Items/Cosmetics/Metadata/CosmeticSets");
                if (asset != null)
                {
                    Global.Print(ConsoleColor.Magenta, "Update Night", "Pre loading Cometic Sets", false, true);

                    var data = asset.GetExport<UDataTable>();
                    foreach (var tag in data.Keys)
                     if (data.TryGetValue(tag, out var seta) && seta is UObject set
                            && set.GetExport<TextProperty>("DisplayName") is { } name
                            && name.Value.Text is FTextHistory.Base text)
                        SetsName.Add(tag, text.SourceString);
                }
                else Global.Print(ConsoleColor.Red, "Error", "Could not get the asset for Cosmetic Sets");

                // cosmetic source (couldnt find it in locres /shrug), kinda of hardcoded
                SourceNames.Add("Cosmetics.Source.ItemShop", "Item Shop");
                SourceNames.Add("Cosmetics.Source.StarterPack", "Starter Pack");
                // battle pass
                SourceNames.Add("Cosmetics.Source.BattlePass.Paid", "Battle Pass (Paid)");
                SourceNames.Add("Cosmetics.Source.BattlePass.Free", "Battle Pass (Free)");
                // raw would be `Cosmetics.Source.Season{season number}...` but we revome that
                SourceNames.Add("Cosmetics.Source.Season.BattlePass.Paid", "Battle Pass (Paid)");
                SourceNames.Add("Cosmetics.Source.Season.BattlePass.Free", "Battle Pass (Free)");
                SourceNames.Add("Cosmetics.Source.Season.BattlePass.Paid.Additional", "Battle Pass (Paid) Additional");
                SourceNames.Add("Cosmetics.Source.Season.FirstWin", "Season First Win");
            }
        }
    }

    // idk, screw fmodel
    public static class Strings
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FixPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path.Length < 5)
                return string.Empty;

            string trigger;
            {
                if (path.Contains("/"))
                {
                    string tempPath = path[1..];
                    trigger = tempPath.Substring(0, tempPath.IndexOf('/'));
                }
                else
                    trigger = path;
            }

            Regex regex = new Regex(trigger);
            if (trigger.Equals("SrirachaRanchCore") || trigger.Equals("SrirachaRanchHoagie") || trigger.Equals("SrirachaRanchValet"))
                trigger = "SrirachaRanch/" + trigger;

            string fixedPath = trigger switch
            {
                "Game" => regex.Replace(path, "FortniteGame/Content", 1),
                "RegionCN" => regex.Replace(path, $"FortniteGame/Plugins/{trigger}/Content", 1),
                _ => regex.Replace(path, $"FortniteGame/Plugins/GameFeatures/{trigger}/Content", 1)
            };

            int sep = fixedPath.LastIndexOf('.');
            return fixedPath.Substring(0, sep > 0 ? sep : fixedPath.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringAfterLast(this string s, char delimiter)
        {
            var index = s.LastIndexOf(delimiter);
            return index == -1 ? s : s.Substring(index + 1, s.Length - index - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringAfterLast(this string s, string delimiter, StringComparison comparisonType = StringComparison.Ordinal)
        {
            var index = s.LastIndexOf(delimiter, comparisonType);
            return index == -1 ? s : s.Substring(index + delimiter.Length, s.Length - index - delimiter.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringBeforeLast(this string s, char delimiter)
        {
            var index = s.LastIndexOf(delimiter);
            return index == -1 ? s : s.Substring(0, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringBeforeLast(this string s, string delimiter, StringComparison comparisonType = StringComparison.Ordinal)
        {
            var index = s.LastIndexOf(delimiter, comparisonType);
            return index == -1 ? s : s.Substring(0, index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetReadableSize(double size)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return string.Format("{0:# ###.##} {1}", size, sizes[order]).TrimStart();
        }
    }
}
