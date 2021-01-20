﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SkiaSharp;
using UpdateNight.Source.Models;

namespace UpdateNight.Source
{
    class Image
    {
        public static SKTypeface burbanktf = SKTypeface.FromFile(Path.Combine(Global.AssetsPath, "fonts", "BurbankBigCondensedBlack.ttf"));
        // public static SKTypeface luckiestguytf = SKTypeface.FromFile(Path.Combine(Global.AssetsPath, "fonts", "LuckiestGuy.ttf")); // not use.. yet :eyes:
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
                    TextSize = 90,
                    Typeface = burbanktf
                };
                canvas.DrawText(cosmetic.Name, new SKPoint(info.Width / 2, 895), NamePaint);

                SKPaint DescPaint = new SKPaint
                {
                    Color = SKColors.White,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill,
                    TextAlign = SKTextAlign.Center,
                    TextSize = 60,
                    Typeface = burbanktf
                };
                int textsize = (int)DescPaint.MeasureText(cosmetic.Description);
                if (textsize >= 991)
                {
                    int size = 60;
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
                Console.WriteLine($"{cosmetic.Id} {textsize}");
                canvas.DrawText(cosmetic.Description, new SKPoint(info.Width / 2, 960), DescPaint);
            }

            var image = surface.Snapshot();
            cosmetic.Canvas = image;
            var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var stream = File.OpenWrite(Path.Combine(Global.current_path, "out", Global.version.Substring(19, 5).Replace(".", "_"), "icons", cosmetic.Id + ".png"));
            data.SaveTo(stream);
            stream.Close();
            Global.Print(ConsoleColor.Green, "Cosmetic Manager", $"Saved image of {cosmetic.Id}");
        }

        public static void Collage(List<SKImage> images, string name) => Collage(images.ToArray(), name);
        public static void Collage(SKImage[] images, string name)
        {
            double width = Math.Ceiling(Math.Sqrt(images.Length));
            double height = Math.Ceiling(images.Length / width);

            SKImageInfo info = new SKImageInfo((int)width * 1024, (int)height * 1024);
            SKSurface surface = SKSurface.Create(info);
            SKCanvas canvas = surface.Canvas;

            // canvas.Clear(SKColors.DarkSlateGray);

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
                    var stream = File.OpenWrite(Path.Combine(Global.current_path, "out", Global.version.Substring(19, 5).Replace(".", "_"), "collages", name + ".png"));
                    data.SaveTo(stream);
                    stream.Close();
                    Global.Print(ConsoleColor.Green, "Collage Manager", $"Saved collage of {name}");
                }
            }
        }
    }
}
