using System;
using System.Collections.Generic;
using System.Linq;
using UpdateNight.TocReader.IO;
using EpicManifestParser.Objects;
using UpdateNight.Source;
using UpdateNight.TocReader;
using System.Threading.Tasks;

namespace UpdateNight.Grabbers
{
    // aka pak grabber
    class Toc
    {
        public static async Task GrabTocsAsync(Manifest manifest)
        {
            List<string> filenames = new List<string>();

            // useless files (used for some translations)
            string[] unused_files = { "pakchunk2-WindowsClient", "pakchunk5-WindowsClient", "pakchunk7-WindowsClient", "pakchunk8-WindowsClient", "pakchunk9-WindowsClient" };
            
            foreach (FileManifest file in manifest.FileManifests)
            {
                if (!file.Name.StartsWith("FortniteGame/Content/Paks/") || file.Name.Contains("global") || file.Name.Contains("optional")) continue;
                if (unused_files.Contains(file.Name.Split("/").Last().Split(".").First())) continue;
                filenames.Add(file.Name.Split(".").First());
            }

            filenames = filenames.Distinct().ToList();

            await Aes.WaitUntilNewKey();

            DateTime astart = DateTime.UtcNow;
            
            foreach (string file in filenames)
            {
                DateTime start = DateTime.UtcNow;
                FileManifest ucas = manifest.FileManifests.Find(f => f.Name == file + ".ucas");
                FileManifest utoc = manifest.FileManifests.Find(f => f.Name == file + ".utoc");

                FFileIoStoreReader ioStore = new FFileIoStoreReader(utoc.Name.SubstringAfterLast('\\'), utoc.Name.SubstringBeforeLast('\\'), utoc.GetStream(), ucas.GetStream());

                if (ioStore.IsEncrypted)
                {
                    // await Aes.WaitUntilNewKey();
                    if (Aes.Keys.TryGetValue(file, out string key)) ioStore.AesKey = key.ToUpperInvariant().Trim().ToBytesKey();
                }

                if (!ioStore.IsEncrypted || (ioStore.IsEncrypted && ioStore.AesKey != null))
                {
                    ioStore.ReadDirectoryIndex();
                    Global.IoFiles.Add(utoc.Name.Replace(".utoc", ""), ioStore);
                }

                DateTime end = DateTime.UtcNow;
                TimeSpan dur = end.Subtract(start);
                
                Console.Write($"[{Global.BuildTime()}] ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[Toc Grabber] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(ioStore.IsEncrypted && ioStore.AesKey == null ? "Skipped " : "Mounted ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(file.Replace("FortniteGame/Content/Paks/", ""));
                Console.ForegroundColor = ConsoleColor.White;
                if (ioStore.IsEncrypted && ioStore.AesKey == null)
                {
                    Console.Write(", No key provided - [Version: ");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write((int)ioStore.TocResource.Header.Version);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(", Guid: ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(ioStore.TocResource.Header.EncryptionKeyGuid);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(", Size: ");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write(Strings.GetReadableSize((double)ioStore.ContainerFile.FileSize));
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("]");
                    
                    Console.WriteLine();
                }
                
                else
                {
                    Console.Write(" in ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(dur.ToString("G").Replace(",", "."));
                    Console.ForegroundColor = ConsoleColor.White;
#if DEBUG
                    Console.Write(" - [Packages: ");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write(ioStore.Files.Count);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(", Mount Point: ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write(ioStore.MountPoint);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(", Version: ");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write((int)ioStore.TocResource.Header.Version);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("]");
#endif
                    Console.WriteLine();
                }
                Console.ForegroundColor = ConsoleColor.White;
            }

            FileManifest globalucas = manifest.FileManifests.Find(f => f.Name.Contains("global.ucas"));
            FileManifest globalutoc = manifest.FileManifests.Find(f => f.Name.Contains("global.utoc"));

            FFileIoStoreReader globalioStore = new FFileIoStoreReader(globalutoc.Name.SubstringAfterLast('\\'), globalutoc.Name.SubstringBeforeLast('\\'), globalutoc.GetStream(), globalucas.GetStream());
            Global.GlobalData = new FIoGlobalData(globalioStore, Global.IoFiles.Values);

            foreach (var reader in Global.IoFiles.Values)
                foreach (var file in reader.Files)
                    if (!Global.assetmapping.ContainsKey(file.Key)) // idk how it can be more then just one file key but yeah
                        Global.assetmapping.Add(file.Key, file.Value);

            DateTime aend = DateTime.UtcNow;
            TimeSpan adur = aend.Subtract(astart);

            Console.Write($"[{Global.BuildTime()}] ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[Toc Grabber] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Loaded ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(Global.IoFiles.Count);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" files in ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(adur.ToString("G").Replace(",", "."));
            Console.ForegroundColor = ConsoleColor.White;
#if DEBUG
            Console.Write(" - [Packages: ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write(Global.assetmapping.Count);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("]");
#endif
            Console.WriteLine("\n");
        }
    }
}
