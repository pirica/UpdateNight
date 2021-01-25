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
        public static void PreLoad()
        {
            DirectoryInfo d = new DirectoryInfo(Path.Combine(Global.AssetsPath, "images", "rarities"));
            FileInfo[] Files = d.GetFiles("*.png");
            foreach (FileInfo file in Files)
            {
                Stream fileStream = File.OpenRead(file.FullName);
                SKBitmap bitmap = SKBitmap.Decode(fileStream);
                SKImage image = SKImage.FromBitmap(bitmap);
                rarities.Add(file.Name.Replace(".png", ""), image);
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

            canvas.DrawImage(cosmetic.Icon, new SKRectI(0, 0, 1024, 1024));

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
                    TextSize = 50,
                    Typeface = burbanktf
                };
                int textsize = (int)DescPaint.MeasureText(cosmetic.Description);
                if (textsize >= 991)
                {
                    int size = 50;
                    bool FitIn = false;
                    while (!FitIn)
                    {
                        DescPaint = new SKPaint
                        {
                            Color = SKColors.White,
                            IsAntialias = true,
                            Style = SKPaintStyle.Fill,
                            TextAlign = SKTextAlign.Center,
                            TextSize = size,
                            Typeface = burbanktf
                        };
                        textsize = (int)DescPaint.MeasureText(cosmetic.Description);
                        if (textsize <= 990) FitIn = true;
                        else size--;
                    }
                }
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
        public static void Weapon(Weapon weapon)
        {
            SKImageInfo info = new SKImageInfo(1024, 1024);
            SKSurface surface = SKSurface.Create(info);
            SKCanvas canvas = surface.Canvas;

            SKImage rarity_bg = rarities[weapon.Rarity];
            SKImage rarity = rarities[weapon.Rarity + "-card"];

            canvas.DrawImage(rarity_bg, new SKPoint(0, 0));

            canvas.DrawImage(weapon.Icon, new SKRectI(0, 0, 1024, 1024));

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
                    TextSize = 50,
                    Typeface = burbanktf
                };
                int textsize = (int)DescPaint.MeasureText(weapon.Description);
                if (textsize >= 991)
                {
                    int size = 50;
                    bool FitIn = false;
                    while (!FitIn)
                    {
                        DescPaint = new SKPaint
                        {
                            Color = SKColors.White,
                            IsAntialias = true,
                            Style = SKPaintStyle.Fill,
                            TextAlign = SKTextAlign.Center,
                            TextSize = size,
                            Typeface = burbanktf
                        };
                        textsize = (int)DescPaint.MeasureText(weapon.Description);
                        if (textsize <= 990) FitIn = true;
                        else size--;
                    }
                }
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

            canvas.DrawImage(map, new SKRectI(0, 0, 2048, 2048));

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
                double width = Math.Ceiling(Math.Sqrt(images.Length));
                double height = Math.Ceiling(images.Length / width);

                SKImageInfo info = new SKImageInfo((int)width * 1024, (int)height * 1024);
                SKSurface surface = SKSurface.Create(info);
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColor.Parse("#292a29"));
                
                int x = 0;
                int y = 0;
                int count = 0;
                foreach (SKImage image in images)
                {
                    canvas.DrawImage(image, new SKPoint(x, y));

                    x += 1024;
                    if (info.Width <= x) // check if `x` is out of the image
                    {
                        x = 0;
                        y += 1024;
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
    }
}
