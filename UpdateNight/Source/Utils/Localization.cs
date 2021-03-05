using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UpdateNight.TocReader.IO;
using UpdateNight.TocReader.Parsers.Class;
using UpdateNight.TocReader.Parsers.Objects;
using UpdateNight.TocReader.Parsers.PropertyTagData;

namespace UpdateNight.Source.Utils
{
    class Localization
    {
        public static Dictionary<string, Dictionary<string, string>> _cachedValues = new Dictionary<string, Dictionary<string, string>>();
        public static Dictionary<string, string> SourceNames = new Dictionary<string, string>();
        public static Dictionary<string, string> SetsName = new Dictionary<string, string>();

        public static async Task PreLoadAsync()
        {
            // Cosmetics sets
            IoPackage asset = Toc.GetAsset("/FortniteGame/Content/Athena/Items/Cosmetics/Metadata/CosmeticSets");
            if (asset != null)
            {
                Global.Print(ConsoleColor.Magenta, "Update Night", "Pre loading Cometic Sets", false, true);

                var hotfixes = await Helpers.Hotfixes.Grab();
                if (hotfixes != null)
                    foreach (var key in hotfixes.Keys)
                        SetsName.Add(key.Replace("_DisplayName", ""), hotfixes[key]);

                var data = asset.GetExport<UDataTable>();
                foreach (var tag in data.Keys)
                    if (!SetsName.ContainsKey(tag) && data.TryGetValue(tag, out var seta) && seta is UObject set
                            && set.GetExport<TextProperty>("DisplayName") is { } name
                            && name.Value.Text is FTextHistory.Base text)
                        SetsName.Add(tag, text.SourceString);
            }
            else Global.Print(ConsoleColor.Yellow, "Warn", "Could not get the asset for Cosmetic Sets");

            // cosmetic source (couldnt find it in locres /shrug), kinda of hardcoded
            
            SourceNames.Add("Cosmetics.Source.ItemShop", "Item Shop");
            SourceNames.Add("Cosmetics.Source.StarterPack", "Starter Pack");
            SourceNames.Add("Cosmetics.Source.Promo", "Promotial");
            SourceNames.Add("Cosmetics.Source.RMT", "Real Money Pack");

            // battle pass
            SourceNames.Add("Cosmetics.Source.BattlePass.Paid", "Battle Pass (Paid)");
            SourceNames.Add("Cosmetics.Source.BattlePass.Free", "Battle Pass (Free)");
            
            // raw would be `Cosmetics.Source.Season{season number}...` but we remove the number
            SourceNames.Add("Cosmetics.Source.Season.BattlePass.Paid", "Battle Pass (Paid)");
            SourceNames.Add("Cosmetics.Source.Season.BattlePass.Free", "Battle Pass (Free)");
            SourceNames.Add("Cosmetics.Source.Season.BattlePass.Paid.Additional", "Battle Pass (Paid) Additional");
            SourceNames.Add("Cosmetics.Source.Season.FirstWin", "Season First Win");
        }

        public static string BuildSource(string source)
        {
            if (source.StartsWith("Cosmetics.Source.Season"))
            {
                string tag = Regex.Replace(source, @"[0-9]", "");
                if (SourceNames.TryGetValue(tag, out var text))
                    return text;
            }

            if (source.StartsWith("Cosmetics.Source.CrewPack.")) return "Crew Pack";
            if (source.StartsWith("Cosmetics.Source.Platform.")) return source.Replace("Cosmetics.Source.Platform.", "");
            if (source.StartsWith("Cosmetics.Source.RMT.")) return source.Replace("Cosmetics.Source.RMT.", "");

            if (SourceNames.ContainsKey(source))
                return SourceNames[source];

            return Regex.Replace(source.Replace("Cosmetics.Source.", ""), "[.]", " ");
        }
    }
}
