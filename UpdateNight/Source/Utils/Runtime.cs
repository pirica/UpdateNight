using System;
using System.IO;
using System.Linq;
using System.Reflection;
using K4os.Compression.LZ4;
using UpdateNight.Exceptions;
using System.Collections.Generic;
using K4os.Compression.LZ4.Streams;

namespace UpdateNight.Source.Utils
{
    public class Runtime
    {
        private static readonly string RuntimeFolder = Directory.CreateDirectory(Path.Combine(Global.CurrentPath, "Runtimes")).FullName;

        public static UNRuntime GetLastRuntime() // most used for force /shrug
        {
            DirectoryInfo directory = new DirectoryInfo(RuntimeFolder);
            FileInfo[] files = directory.GetFiles();
            FileInfo file = files.Where(f => f.Name.EndsWith(".unrtm")).OrderByDescending(f => f.LastWriteTime).First();

            return UNRuntime.Read(file.Name);
        }

        public static UNRuntime GetRuntimeByVersion(string version)
        {
            DirectoryInfo directory = new DirectoryInfo(RuntimeFolder);
            FileInfo[] files = directory.GetFiles("*.unrtm");

            string path = null;
            for (int i = 0; i < files.Length; i++)
            {
                var runtime = UNRuntime.Read(files[i].Name, false);
                if (runtime.FNVersion.Substring(19, 5) == version)
                {
                    path = files[i].Name;
                    break;
                }
            }

            if (string.IsNullOrEmpty(path))
                throw new RuntimeNotFoundException($"Could not find runtime for version {version}");

            return UNRuntime.Read(path);
        }

        public static string SaveCurrentRuntime()
        {
            string filename = "Runtime_" + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + ".unrtm";

            var runtime = new UNRuntime
            {
                Version = 2,
                UNVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                FNVersion = Global.Version,
                Date = DateTimeOffset.Now.ToUnixTimeSeconds(),
                Files = Files.BuildFileList()
            };

            runtime.Save(filename);

            return filename;
        }

        public class UNRuntime
        {
            public int Version { get; set; } // runtime version
            public string UNVersion { get; set; } // un version
            public string FNVersion { get; set; } // fn version
            public long Date { get; set; } // date
            public int FileCount { get; set; } // file list
            public List<string> Files { get; set; } // file list

            public void Save(string filename)
            {
                FileStream fstream = new FileStream(Path.Combine(RuntimeFolder, filename), FileMode.Create);
                LZ4EncoderStream cstream = LZ4Stream.Encode(fstream, LZ4Level.L00_FAST);
                BinaryWriter writer = new BinaryWriter(cstream);

                writer.Write(0x49B13E0C); // magic
                writer.Write(Version);
                writer.Write(UNVersion);
                writer.Write(FNVersion);
                writer.Write(Date.ToString());

                writer.Write(Files.Count);
                foreach (var path in Files)
                    writer.Write(path);

                writer.Close();
                cstream.Close();
                fstream.Close();
            }

            public static UNRuntime Read(string filename, bool readFiles = true)
            {
                FileStream fstream = new FileStream(Path.Combine(RuntimeFolder, filename), FileMode.Open);
                LZ4DecoderStream dstream = LZ4Stream.Decode(fstream);
                BinaryReader reader = new BinaryReader(dstream);

                var runtime = new UNRuntime();

                var magic = reader.ReadInt32();

                if (magic != 0x49B13E0C)
                    throw new InvalidRuntimeException("[Magic Missmatch] Runtime magic missmatch with update nigth runtime magic");

                runtime.Version = reader.ReadInt32();
                runtime.UNVersion = reader.ReadString();
                runtime.FNVersion = reader.ReadString();

                if (runtime.Version == 1)
                    runtime.Date = reader.ReadInt64();
                else
                    runtime.Date = long.Parse(reader.ReadString());

                var count = reader.ReadInt32();
                runtime.FileCount = count;
                runtime.Files = new List<string>();

                if (readFiles)
                {
                    var i = 0;
                    while (i < count)
                    {
                        i++;
                        runtime.Files.Add(reader.ReadString());
                    }
                }

                reader.Close();
                dstream.Close();
                fstream.Close();

                return runtime;
            }
        }
    }
}
