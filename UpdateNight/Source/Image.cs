using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SkiaSharp;
using UpdateNight.Source.Models;

namespace UpdateNight.Source
{
    class Image
    {
        public static SKTypeface burbanktf = SKTypeface.FromFile(Path.Combine(Global.AssetsPath, "fonts", "BurbankBigCondensedBlack.ttf"));
        public static SKTypeface luckiestguytf = SKTypeface.FromFile(Path.Combine(Global.AssetsPath, "fonts", "LuckiestGuy.ttf")); // not use.. yet :eyes:
        public static Dictionary<string, SKImage> rarities = new Dictionary<string, SKImage>();
        public static Dictionary<string, SKImage> images = new Dictionary<string, SKImage>();
        public static void PreLoad()
        {
            DirectoryInfo RD = new DirectoryInfo(Path.Combine(Global.AssetsPath, "images", "rarities"));
            FileInfo[] RF = RD.GetFiles("*.png");
            foreach (FileInfo file in RF)
            {
                Stream fileStream = File.OpenRead(file.FullName);
                SKBitmap bitmap = SKBitmap.Decode(fileStream);
                SKImage image = SKImage.FromBitmap(bitmap);
                rarities.Add(file.Name.Replace(".png", ""), image);
            }

            DirectoryInfo ID = new DirectoryInfo(Path.Combine(Global.AssetsPath, "images", "challenges"));
            FileInfo[] IF = ID.GetFiles("*.png");
            foreach (FileInfo file in IF)
            {
                Stream fileStream = File.OpenRead(file.FullName);
                SKBitmap bitmap = SKBitmap.Decode(fileStream);
                SKImage image = SKImage.FromBitmap(bitmap);
                images.Add(file.Name.Replace(".png", ""), image);
            }
        }

        public static void Cosmetic(Cosmetic cosmetic)
        {
            SKImageInfo info = new SKImageInfo(1024, 1024);
            SKSurface surface = SKSurface.Create(info);
            SKCanvas canvas = surface.Canvas;

            SKImage rarity_bg = rarities[cosmetic.Rarity];
            SKImage rarity = rarities[cosmetic.Rarity + "-card"];

            canvas.DrawImage(rarity_bg, new SKPoint(0, 0));

            canvas.DrawImage(Resize(cosmetic.Icon, 1024, 1024), new SKPoint(0, 0));

            if (!string.IsNullOrEmpty(cosmetic.Name) && !string.IsNullOrEmpty(cosmetic.Description))
            {
                canvas.DrawImage(rarity, new SKPoint(0, 0));

                SKPaint NamePaint = new SKPaint
                {
                    Color = SKColors.White,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                    TextAlign = SKTextAlign.Center,
                    TextSize = 80,
                    Typeface = burbanktf
                };
                canvas.DrawText(cosmetic.Name, new SKPoint(info.Width / 2, 880), NamePaint);

                SKPaint DescPaint = new SKPaint
                {
                    Color = SKColors.White,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                    TextAlign = SKTextAlign.Center,
                    TextSize = GetTextSizeThatFitsIn(cosmetic.Description, 50, 990),
                    Typeface = burbanktf
                };
                canvas.DrawText(cosmetic.Description, new SKPoint(info.Width / 2, 940), DescPaint);
            }

            // set
            if (!string.IsNullOrEmpty(cosmetic.Set)
                && Utils.Localization.SetsName.TryGetValue(cosmetic.Set, out var set))
            {
                SKPaint NamePaint = new SKPaint
                {
                    Color = SKColors.DimGray,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                    TextAlign = SKTextAlign.Left,
                    TextSize = 40,
                    Typeface = burbanktf
                };
                canvas.DrawText(set, new SKPoint(20, 1004), NamePaint);
            }

            // source
            if (!string.IsNullOrEmpty(cosmetic.Source))
            {
                SKPaint NamePaint = new SKPaint
                {
                    Color = SKColors.DimGray,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                    TextAlign = SKTextAlign.Right,
                    TextSize = 40,
                    Typeface = burbanktf
                };
                canvas.DrawText(cosmetic.Source, new SKPoint(1004, 1004), NamePaint);
            }

            var image = surface.Snapshot();
            cosmetic.Canvas = image;
            var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var stream = File.OpenWrite(Path.Combine(Global.OutPath, "icons", cosmetic.Id + ".png"));
            data.SaveTo(stream);
            stream.Close();
            Global.Print(ConsoleColor.Green, "Cosmetic Manager", $"Saved image of {cosmetic.Id}");
        }

        public static void Challenge(Challenge challenge)
        {
            SKImageInfo info = new SKImageInfo(1024, 133 + (136 * challenge.Quests.Count));
            SKSurface surface = SKSurface.Create(info);
            SKCanvas canvas = surface.Canvas;

            SKImage header = images["header"];
            SKImage background = images["background"];
            SKImage questi = images["quest"];

            if (info.Height > background.Height)
                canvas.DrawImage(SKImage.FromBitmap(SKBitmap.Decode(background.Encode())
                    .Resize(new SKImageInfo(1024, info.Height), SKFilterQuality.High)), new SKPoint(0, 0));
            else canvas.DrawImage(background, new SKPoint(0, 0));
            canvas.DrawImage(header, new SKPoint(0, 0));

            SKPaint HeaderPaint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.Fill,
                TextAlign = SKTextAlign.Left,
                TextSize = GetTextSizeThatFitsIn(challenge.Name, 70, 1000),
                Typeface = burbanktf
            };
            canvas.DrawText(challenge.Name, new SKPoint(40, 80), HeaderPaint);

            int y = 133;
            for (int i = 0; i < challenge.Quests.Count; i++)
            {
                var quest = challenge.Quests.ElementAt(i);

                canvas.DrawImage(questi, new SKPoint(0, y));

                SKPaint TextPaint = new SKPaint
                {
                    Color = SKColors.White,
                    Style = SKPaintStyle.Fill,
                    TextAlign = SKTextAlign.Left,
                    TextSize = GetTextSizeThatFitsIn(quest.Text, 35, 990),
                    Typeface = burbanktf
                };
                canvas.DrawText(quest.Text, new SKPoint(40, y + 58), TextPaint);


                SKPaint RewardTextPaint = new SKPaint
                {
                    Color = SKColors.White,
                    Style = SKPaintStyle.Fill,
                    TextAlign = SKTextAlign.Left,
                    TextSize = 25,
                    Typeface = burbanktf
                };
                canvas.DrawText(quest.Reward.Text, new SKPoint(140, y + 110), RewardTextPaint);

                canvas.DrawImage(SKImage.FromBitmap(SKBitmap.Decode(quest.Reward.Icon.Encode()) // to resize the image /shrug
                    .Resize(new SKImageInfo(92, 92), SKFilterQuality.High)),
                    new SKPoint(915, y + 25));

                y += 136;
            }

            var image = surface.Snapshot();
            var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var stream = File.OpenWrite(Path.Combine(Global.OutPath, "challenges", challenge.Id + ".png"));
            data.SaveTo(stream);
            stream.Close();
            Global.Print(ConsoleColor.Green, "Challenges Manager", $"Saved image of {challenge.Id}");
        }

        public static void Weapon(Weapon weapon)
        {
            SKImageInfo info = new SKImageInfo(1024, 1024);
            SKSurface surface = SKSurface.Create(info);
            SKCanvas canvas = surface.Canvas;

            SKImage rarity_bg = rarities[weapon.Rarity];
            SKImage rarity = rarities[weapon.Rarity + "-card"];

            canvas.DrawImage(rarity_bg, new SKPoint(0, 0));

            canvas.DrawImage(Resize(weapon.Icon, 1024, 1024), new SKPoint(0, 0));

            if (!string.IsNullOrEmpty(weapon.Name) && !string.IsNullOrEmpty(weapon.Description))
            {
                canvas.DrawImage(rarity, new SKPoint(0, 0));

                SKPaint NamePaint = new SKPaint
                {
                    Color = SKColors.White,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                    TextAlign = SKTextAlign.Center,
                    TextSize = 80,
                    Typeface = burbanktf
                };
                canvas.DrawText(weapon.Name, new SKPoint(info.Width / 2, 880), NamePaint);

                SKPaint DescPaint = new SKPaint
                {
                    Color = SKColors.White,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                    TextAlign = SKTextAlign.Center,
                    TextSize = GetTextSizeThatFitsIn(weapon.Description, 50, 990),
                    Typeface = burbanktf
                };
                canvas.DrawText(weapon.Description, new SKPoint(info.Width / 2, 940), DescPaint);
            }
            
            var image = surface.Snapshot();
            weapon.Canvas = image;
            var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var stream = File.OpenWrite(Path.Combine(Global.OutPath, "weapons", weapon.Id + ".png"));
            data.SaveTo(stream);
            stream.Close();
            Global.Print(ConsoleColor.Green, "Weapon Manager", $"Saved image of {weapon.Id}");
        }

        public static void Map(SKImage map, List<POI> pois)
        {
            SKImageInfo info = new SKImageInfo(2048, 2048);
            SKSurface surface = SKSurface.Create(info);
            SKCanvas canvas = surface.Canvas;

            canvas.DrawImage(Resize(map, 2048, 2048), new SKPoint(0, 0));

            int count = 0;
            pois = pois.OrderBy(p => p.IsBigPoi).ToList();
            foreach (var poi in pois)
            {
                SKPaint Paint = new SKPaint
                {
                    Color = poi.IsBigPoi ? SKColors.White : SKColors.Yellow,
                    Style = SKPaintStyle.Fill,
                    StrokeWidth = 2,
                    TextAlign = SKTextAlign.Center,
                    TextSize = poi.IsBigPoi ? 23 : 20,
                    Typeface = SKTypeface.FromFile(Path.Combine(Global.AssetsPath, "fonts", "italic.otf"))
                };
                canvas.DrawText(poi.Name, new SKPoint(poi.X + 15, poi.Y + 3), Paint);
                canvas.DrawText(".", new SKPoint(poi.X + 10, poi.Y + 15), Paint);

                count++;
                if (count == pois.Count)
                {
                    var image = surface.Snapshot();
                    var data = image.Encode(SKEncodedImageFormat.Png, 100);
                    var stream = File.OpenWrite(Path.Combine(Global.OutPath, "map_pois.png"));
                    data.SaveTo(stream);
                    stream.Close();
                    Global.Print(ConsoleColor.Green, "Map", "Saved map image with POIs");
                }
            }
        }

        public static void Collage(List<SKImage> images, string name) => Collage(images.ToArray(), name);
        public static void Collage(SKImage[] images, string name)
        {
            if (images != null && images.Length >= 1)
            {
                var width = Math.Ceiling(Math.Sqrt(images.Length));
                var height = Math.Ceiling(images.Length / width);
                width = (int)width; height = (int)height;

                int size = width > 4 ? 512 : 1024;
                int spacing = width > 4 ? 45 : 90;

                SKImageInfo info = new SKImageInfo((int)((width * size) + ((width + 1) * spacing)), (int)((height * size) + ((height + 1) * spacing)));
                SKSurface surface = SKSurface.Create(info);
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColor.Parse("#292a29"));
                
                int x = spacing;
                int y = spacing;
                int count = 0;
                foreach (SKImage image in images)
                {
                    canvas.DrawImage(Resize(image, size, size), new SKPoint(x, y));

                    x += size + spacing;
                    if (info.Width <= x) // check if `x` is out of the image
                    {
                        x = spacing;
                        y += size + spacing;
                    }

                    count++;
                    if (images.Length == count)
                    {
                        var aimage = surface.Snapshot();
                        var data = aimage.Encode(SKEncodedImageFormat.Png, 100);
                        var stream = File.OpenWrite(Path.Combine(Global.OutPath, "collages", name + ".png"));
                        data.SaveTo(stream);
                        stream.Close();
                        Global.Print(ConsoleColor.Green, "Collage Manager", $"Saved collage of {name}");
                    }
                }
            }
        }

        public static SKImage Resize(SKImage image, int width, int height)
        {
            SKBitmap bitmap = SKBitmap.FromImage(image);
            bitmap = bitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.High);
            return SKImage.FromBitmap(bitmap);
        }

        public static int GetTextSizeThatFitsIn(string text, int initialsize, int maximumsize) // TODO: better name
        {
            SKPaint Paint = new SKPaint
            {
                TextSize = initialsize
            };
            int textsize = (int)Paint.MeasureText(text);
            if (textsize >= maximumsize)
            {
                int size = initialsize;
                bool FitIn = false;
                while (!FitIn)
                {
                    Paint = new SKPaint
                    {
                        TextSize = size
                    };
                    textsize = (int)Paint.MeasureText(text);
                    if (textsize <= 990) FitIn = true;
                    else size--;
                }
                return size;
            }
            
            return initialsize;
        }
    }
}
