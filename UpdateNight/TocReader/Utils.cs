using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UpdateNight.TocReader.IO;
using UpdateNight.Source.Utils;

namespace UpdateNight.TocReader
{
    public class Utils
    {
        public static void Merge<T>(Dictionary<string, T> tempFiles, out Dictionary<string, T> files, string mount) where T : ReaderEntry
        {
            files = new Dictionary<string, T>();
            foreach (var entry in tempFiles.Values)
            {
                if (files.ContainsKey(mount + entry.GetPathWithoutExtension()) ||
                    entry.GetExtension().Equals(".uptnl") ||
                    entry.GetExtension().Equals(".uexp") ||
                    entry.GetExtension().Equals(".ubulk"))
                    continue;

                if (entry.IsUE4Package()) // if .uasset
                {
                    if (!tempFiles.ContainsKey(Path.ChangeExtension(entry.Name, ".umap"))) // but not including a .umap
                    {
                        string e = Path.ChangeExtension(entry.Name, ".uexp");
                        var uexp = tempFiles.ContainsKey(e) ? tempFiles[e] : null; // add its uexp
                        if (uexp != null)
                            entry.Uexp = uexp;

                        string u = Path.ChangeExtension(entry.Name, ".ubulk");
                        var ubulk = tempFiles.ContainsKey(u) ? tempFiles[u] : null; // add its ubulk
                        if (ubulk != null)
                            entry.Ubulk = ubulk;
                        else
                        {
                            string f = Path.ChangeExtension(entry.Name, ".ufont");
                            var ufont = tempFiles.ContainsKey(f) ? tempFiles[f] : null; // add its ufont
                            if (ufont != null)
                                entry.Ubulk = ufont;
                        }
                    }
                }
                else if (entry.IsUE4Map()) // if .umap
                {
                    string e = Path.ChangeExtension(entry.Name, ".uexp");
                    string u = Path.ChangeExtension(entry.Name, ".ubulk");
                    var uexp = tempFiles.ContainsKey(e) ? tempFiles[e] : null; // add its uexp
                    if (uexp != null)
                        entry.Uexp = uexp;
                    var ubulk = tempFiles.ContainsKey(u) ? tempFiles[u] : null; // add its ubulk
                    if (ubulk != null)
                        entry.Ubulk = ubulk;
                }

                files[mount + entry.GetPathWithoutExtension()] = entry;
            }
        }

        public static string GetFullPath(FPackageId id)
        {
            foreach (var ioStore in Global.IoFiles.Values)
            {
                if (ioStore.Chunks.TryGetValue(id.Id, out string path))
                {
                    return ioStore.MountPoint + path;
                }
            }

            return null;
        }

        public static Package GetPackage(ReaderEntry entry, string mount)
        {
            TryGetPackage(entry, mount, out var p);
            return p;
        }
        private static bool TryGetPackage(ReaderEntry entry, string mount, out IoPackage package)
        {
            if (entry is FIoStoreEntry ioStoreEntry)
            {
                var uasset = ioStoreEntry.GetData();
                var ubulk = (ioStoreEntry.Ubulk as FIoStoreEntry)?.GetData();
                package = new IoPackage(uasset, ubulk);

                return true;
            }

            package = null;
            return false;
        }

        public static Package GetPropertyPakPackage(string value)
        {
            string path = Strings.FixPath(value);
            foreach (var ioStoreReader in Global.IoFiles.Values)
                if (ioStoreReader.TryGetCaseInsensiteveValue(path, out var entry))
                {
                    string mount = path.Substring(0, path.Length - entry.Name.Substring(0, entry.Name.LastIndexOf('.')).Length);
                    return GetPackage(entry, mount);
                }
            return default;
        }
    }

    public class Localizations
    {
        // { namespace = { key = string }}
        private static readonly Dictionary<string, Dictionary<string, string>> _hotfixLocalizationDict = new Dictionary<string, Dictionary<string, string>>();
        private static readonly Dictionary<string, Dictionary<string, string>> _fortniteLocalizationDict = new Dictionary<string, Dictionary<string, string>>();

        public static void LoadLocalization()
        {
            // haha no, locres are stored in paks
            /* 
            foreach (var reader in Global.IoFiles.Values)
            {
                if (!reader.FileName.Contains("s23")) continue;
                Console.WriteLine(reader.MountPoint);
                foreach (var file in reader.Files)
                {
                    if (file.Key.Contains("Localization")) Console.WriteLine(path);
                    if (!file.Key.StartsWith("/FortniteGame/Content/Localization/")) continue;
                    if (!file.Key.Contains("en")) continue;
                    if (!file.Key.EndsWith(".locres")) continue;
                    Console.WriteLine($"{reader.FileName} - {file.Key}");
                }
            }
            */
        }

        public static string GetLocalization(string sNamespace, string sKey, string defaultText)
        {
            if (_hotfixLocalizationDict.Count > 0 &&
                _hotfixLocalizationDict.TryGetValue(sNamespace, out var dDict) &&
                dDict.TryGetValue(sKey, out var dRet))
            {
                return dRet;
            }

            if (_fortniteLocalizationDict.Count > 0 &&
                _fortniteLocalizationDict.TryGetValue(sNamespace, out var dict) &&
                dict.TryGetValue(sKey, out var ret))
            {
                return ret;
            }

            return defaultText;
        }
    }

    public static class Enums
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAnyFlags<T>(this T flags, T contains) where T : System.Enum, IConvertible
        {
            return (flags.ToInt32(null) & contains.ToInt32(null)) != 0;
        }
    }

    public static class MathUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DivideAndRoundUp(this int dividend, int divisor) => (dividend + divisor - 1) / divisor;
    }

    public static class BitArrays
    {

        public static bool Contains(this BitArray array)
        {
            for (var i = 0; i < array.Count; i++)
            {
                if (array[i])
                    return true;
            }

            return false;
        }
    }

    public static class ByteOrderSwap
    {
        public static ulong IntelOrder64(this ulong value)
        {
            value = ((value << 8) & 0xFF00FF00FF00FF00UL) | ((value >> 8) & 0x00FF00FF00FF00FFUL);
            value = ((value << 16) & 0xFFFF0000FFFF0000UL) | ((value >> 16) & 0x0000FFFF0000FFFFUL);
            return (value << 32) | (value >> 32);
        }
    }
}
