using UpdateNight.TocReader.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UsmapNET.Classes;
using System.Diagnostics;
using UpdateNight.Helpers;
using Newtonsoft.Json;

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

    public static DeviceAuth DeviceAuth = File.Exists(Path.Combine(CurrentPath, "deviceauth.json"))
        ? JsonConvert.DeserializeObject<DeviceAuth>(File.ReadAllText(Path.Combine(CurrentPath, "deviceauth.json")))
        : null;

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

    public static void Check()
    {
        bool oodleExists = File.Exists(Path.Combine(CurrentPath, "oo2core_8_win64.dll"));
        if (!oodleExists) Exit(0, "Could not locate oodle dll (oo2core_8_win64.dll) in the working directory", true);

        bool assetsExists = Directory.Exists(AssetsPath);
        if (!assetsExists) Exit(0, "Assets folder does not exists", true);

        bool runtimeExists = Directory.Exists(Path.Combine(CurrentPath, "Runtimes"));
        if (!runtimeExists) Exit(0, "Runtime folder doesnt exists", true);
    }

    public static void CreateOut()
    {
        OutPath = Path.Combine(CurrentPath, "out", (Version.Length == 5 ? Version : Version.Substring(19, 5)).Replace(".", "_"));
        Directory.CreateDirectory(Path.Combine(OutPath, "icons"));
        Directory.CreateDirectory(Path.Combine(OutPath, "challenges"));
        Directory.CreateDirectory(Path.Combine(OutPath, "weapons"));
        Directory.CreateDirectory(Path.Combine(OutPath, "collages"));
        Directory.CreateDirectory(Path.Combine(OutPath, "ui"));
    }

    public static void Exit(int code) => Exit(code, null, false);
    public static void Exit(int code, string message) => Exit(code, message, false);
    public static void Exit(int code, string message, bool error = false)
    {
        if (message != null) Print(error ? ConsoleColor.Red : ConsoleColor.Magenta, error ? "Error" : "Update Night", message);
        Thread.Sleep(10000);
        Environment.Exit(code);
    }
}