using UpdateNight.TocReader.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using UsmapNET.Classes;
using UpdateNight.TocReader.Parsers.Objects;
using System.Diagnostics;
using UpdateNight.Source;

public class Global
{
#if DEBUG
    public static bool debugMode = true;
#else
    public static bool debugMode = false;
#endif

    public static string Version = "";
    public static string CurrentPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);
    public static string OutPath = "";
    public static string AssetsPath = Path.Combine(CurrentPath, "assets");
    public static readonly Dictionary<string, FFileIoStoreReader> IoFiles = new Dictionary<string, FFileIoStoreReader>();
    public static readonly Dictionary<string, FUnversionedType> CachedSchemas = new Dictionary<string, FUnversionedType>();

    // utoc file -> asset path
    public static Dictionary<string, string> filemapping = new Dictionary<string, string>();
    // asset path -> entry
    public static Dictionary<string, FIoStoreEntry> assetmapping = new Dictionary<string, FIoStoreEntry>();

    public static FIoGlobalData GlobalData = null; 
    public static Usmap Usmap = null;

    public static void Print(ConsoleColor color, string title, string message, bool line = false, bool debug = false)
    {
        if (debug && !debugMode) return;
        if (line) Console.WriteLine();
        Console.Write($"[{BuildTime()}] ");
        Console.ForegroundColor = color;
        Console.Write($"[{title}] ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(message);
        if (line) Console.WriteLine();
    }

    public static string BuildTime() => BuildTime(DateTime.UtcNow);
    public static string BuildTime(DateTime time) => time.ToString("T") + "." + time.ToString("fff");

    public static void Check(string OldVersion)
    {
        bool oodleExists = File.Exists(Path.Combine(CurrentPath, "oo2core_8_win64.dll"));
        if (!oodleExists) Exit(0, "Could not locate oodle dll (oo2core_8_win64.dll) in the working directory");

        bool assetsExists = Directory.Exists(AssetsPath);
        if (!assetsExists) Exit(0, "Assets folder does not exists");

        bool outExists = Directory.Exists(Path.Combine(CurrentPath, "out"));
        if (!outExists) Exit(0, "Output folder doesnt exists");

        bool outbopExists = Directory.Exists(Path.Combine(CurrentPath, "out", (OldVersion.Length == 5 ? OldVersion : OldVersion.Substring(19, 5)).Replace(".", "_")));
        if (!outbopExists) Exit(0, "Could not find the files to compare");
    }

    public static void CreateOut()
    {
        OutPath = Path.Combine(CurrentPath, "out", (Version.Length == 5 ? Version : Version.Substring(19, 5)).Replace(".", "_"));
        Directory.CreateDirectory(OutPath);
        Directory.CreateDirectory(Path.Combine(OutPath, "icons"));
        Directory.CreateDirectory(Path.Combine(OutPath, "collages"));
        Directory.CreateDirectory(Path.Combine(OutPath, "ui"));
    }

    public static void Exit(int code) => Exit(code, null);
    public static void Exit(int code, string message)
    {
        if (message != null) Print(ConsoleColor.Magenta, "Update Night", message);
        Thread.Sleep(10000);
        Environment.Exit(code);
    }
}