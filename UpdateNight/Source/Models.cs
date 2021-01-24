using System;
using System.Collections.Generic;
using System.Linq;
using UpdateNight.TocReader.IO;
using UpdateNight.TocReader.Parsers.Class;
using UpdateNight.TocReader.Parsers.PropertyTagData;
using UpdateNight.TocReader.Parsers.Objects;
using SkiaSharp;
using System.IO;
using System.Text.RegularExpressions;

namespace UpdateNight.Source.Models
{
    class Cosmetic
    {
        public string Id { get; set; }
        public string Path { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Rarity { get; set; }

        public List<string> Tags { get; set; }
        public string Source { get; set; }
        public string Set { get; set; }

        public string ImagePath { get; set; }
        public SKImage Icon { get; set; }
        public SKImage Canvas { get; set; }
        private bool ForceQuestionMark { get; set; }

        public Cosmetic(IoPackage data, string path)
        {
            Id = path.Split("/").Last();
            Path = path;

            IUExport export = data.Exports[0];

            // basic stuff
            if (export.GetExport<TextProperty>("DisplayName") is { } n && n.Value is { } nt && nt.Text is FTextHistory.Base nb)
                Name = nb.SourceString.Replace("<Emphasized>", "").Replace("</>", "");

            if (export.GetExport<TextProperty>("Description") is { } d && d.Value is { } dt && dt.Text is FTextHistory.Base db)
                Description = db.SourceString.Replace("<Emphasized>", "").Replace("</>", "");

            if (export.GetExport<TextProperty>("ShortDescription") is { } t && t.Value is { } tt && tt.Text is FTextHistory.Base tb)
                Type = tb.SourceString.Replace("<Emphasized>", "").Replace("</>", "");
            else
                Type = "Wrap"; // risky.. but /shrug

            // get rarity
            if (export.GetExport<ObjectProperty>("Series") is { } series)
                Rarity = series.Value.Resource.ObjectName.String;
            else if (export.GetExport<EnumProperty>("Rarity") is { } rarity)
                Rarity = rarity.Value.String.Replace("EFortRarity::", "");
            else
                Rarity = "Uncommon";

            // tags
            Tags = new List<string>();
            if (export.GetExport<StructProperty>("GameplayTags") is { } gameplayTags && gameplayTags.Value is FGameplayTagContainer g)
                Tags = g.GameplayTags.Select(t => t.String).ToList();

            // set
            if (Tags.Any(t => t.StartsWith("Cosmetics.Set.")))
                Set = Tags.Find(t => t.StartsWith("Cosmetics.Set."));

            // source
            if (Tags.Any(t => t.StartsWith("Cosmetics.Source.")))
            {
                string source = Tags.Find(t => t.StartsWith("Cosmetics.Source."));
                Source = Utils.Localization.BuildSource(source);
            }

            // image path, the advanced part
            if (export.GetExport<SoftObjectProperty>("DisplayAssetPath") is { } dpath)
                ImagePath = dpath.Value.AssetPathName.String;
            else if (export.GetExport<SoftObjectProperty>("LargePreviewImage") is { } lpath)
                ImagePath = lpath.Value.AssetPathName.String;
            else if (export.GetExport<SoftObjectProperty>("SmallPreviewImage") is { } spath)
                ImagePath = spath.Value.AssetPathName.String;
            else
            {
                path = path.Split("/").Last();
                string tpath = "/FortniteGame/Content/UI/Foundation/Textures/BattleRoyale/FeaturedItems/Outfit/T-AthenaSoldiers-" + path;
                tpath = tpath.Replace("_" + tpath.Split("_").Last(), "");
                tpath = Regex.Replace(tpath, @"_", "-");
                var asset = Toc.GetAsset(tpath);

                if (asset == null)
                {
                    tpath = Regex.Replace(tpath, @"-(M|F)-", "-");
                    asset = Toc.GetAsset(tpath);
                }

                if (asset != null && asset.ExportTypes.Any(e => e.String == "Texture2D"))
                    ImagePath = tpath;
                else
                {
                    temppath = "/FortniteGame/Content/Catalog/MI_OfferImages/MI_" + path.Replace("Athena_Commando_", "");

                    var asset = Toc.GetAsset(temppath);

                    if (asset == null)
                    {
                        temppath = temppath.Replace("_" + temppath.Split("_").Last(), "");
                        asset = Toc.GetAsset(temppath);
                    }

                    if (asset != null)
                    {
                        IUExport aexport = asset.Exports[0];

                        if (aexport.GetExport<ArrayProperty>("TextureParameterValues") is ArrayProperty textureParameterValues)
                            foreach (StructProperty textureParameter in textureParameterValues.Value)
                                if (textureParameter.Value is UObject parameter && parameter.TryGetValue("ParameterValue", out var i) && i is ObjectProperty value)
                                    ImagePath = value.Value.Resource.OuterIndex.Resource.ObjectName.String;
                    }
                    else if (export.GetExport<ObjectProperty>("HeroDefinition", "WeaponDefinition") is { } hd)
                    {
                        string heropaath = hd.Value.Resource.OuterIndex.Resource.ObjectName.String.Replace("Game", "FortniteGame/Content");
                        var hasset = Toc.GetAsset(heropaath);

                        if (hasset != null)
                        {
                            IUExport hexport = hasset.Exports[0];

                            if (hexport.GetExport<SoftObjectProperty>("DisplayAssetPath") is { } hdpath)
                                ImagePath = hdpath.Value.AssetPathName.String;
                            else if (hexport.GetExport<SoftObjectProperty>("LargePreviewImage") is { } hlpath)
                                ImagePath = hlpath.Value.AssetPathName.String;
                            else if (export.GetExport<SoftObjectProperty>("SmallPreviewImage") is { } hspath)
                                ImagePath = hspath.Value.AssetPathName.String;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(ImagePath)) ImagePath = ImagePath.Replace("/Game/", "/FortniteGame/Content/").Split(".").First();

            // load image
            GetImage();
        }

        private void GetImage()
        {
            if (string.IsNullOrEmpty(ImagePath) || ForceQuestionMark) // looks dumb but i have no best way
            {
                Stream fileStream = File.OpenRead(System.IO.Path.Combine(Global.AssetsPath, "images", "QuestionMark.png"));
                SKBitmap bitmap = SKBitmap.Decode(fileStream);
                SKImage image = SKImage.FromBitmap(bitmap);
                Icon = image;
            }
            else
            {
                IoPackage asset = Toc.GetAsset(ImagePath.Replace("Loadingscreens", "LoadingScreens"));
                if (asset == null)
                {
                    Global.Print(ConsoleColor.Red, "Error", $"Could not get asset for {ImagePath}", true);
                    ForceQuestionMark = true;
                    GetImage();
                    return;
                }
                if (asset.ExportTypes.Any(e => e.String != "Texture2D"))
                {
                    Global.Print(ConsoleColor.Red, "Error", $"The Image Path Detected for {Id} ({ImagePath}) is not a Texture", true);
                    ForceQuestionMark = true;
                    GetImage();
                    return;
                }
                UTexture2D texture = asset.GetExport<UTexture2D>();
                SKImage image = texture.Image;
                Icon = image;
            }
        }
    }

    class POI
    {
        public string Name { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        public string Tag { get; set; }
        public bool IsBigPoi { get; set; } // dont know the name so lets call a big poi 
        public List<string> CalendarEventsRequired { get; set; }

        public POI(UObject data)
        {
            Name = "???";
            if (data.GetExport<TextProperty>("Text") is { } n && n.Value is { } nt && nt.Text is FTextHistory.Base nb)
                Name = nb.SourceString;

            if (data.GetExport<StructProperty>("LocationTag") is { } at && at.Value is UObject bt
                && bt.GetExport<NameProperty>("TagName") is { } ct)
                Tag = ct.Value.String;

            IsBigPoi = false;
            if (!string.IsNullOrEmpty(Tag)) IsBigPoi = !Tag.ToLower().Contains("unnamedpoi");
            
            X = 0;
            Y = 0;
            int WorldRadius = 135000;
            if (data.GetExport<StructProperty>("WorldLocation") is { } awl && awl.Value is FVector bop)
            {
                X = (bop.Y + WorldRadius) / (WorldRadius * 2) * 2048;
                Y = (1 - (bop.X + WorldRadius) / (WorldRadius * 2)) * 2048;
            }

            CalendarEventsRequired = new List<string>();
            if (data.GetExport<ArrayProperty>("CalendarEventsRequired") is { } cer)
                foreach (var info in cer.Value)
                    if (info is StrProperty str)
                        CalendarEventsRequired.Add(str.Value);
        }
    }
}
