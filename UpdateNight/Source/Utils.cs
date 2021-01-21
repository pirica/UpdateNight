using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UpdateNight.TocReader;
using UpdateNight.TocReader.IO;
using UpdateNight.TocReader.Parsers.Class;

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
