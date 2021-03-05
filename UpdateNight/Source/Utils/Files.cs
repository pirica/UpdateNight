using System;
using System.Collections.Generic;
using System.Linq;

namespace UpdateNight.Source.Utils
{
    class Files
    {
        public static List<string> DiffFiles(List<string> old, List<string> current) => current.Except(old).ToList();
        public static List<string> BuildFileList() => Global.assetmapping.Keys.ToList();

        public static List<string> GetNewFiles(bool Force, string old_version)
        {
            DateTime Start = DateTime.UtcNow;

            Global.Print(ConsoleColor.Green, "File Manager", "Building file list...");

            List<string> CurrentFiles = BuildFileList();
            List<string> OldFiles = Force ? Runtime.GetLastRuntime().Files : Runtime.GetRuntimeByVersion(old_version).Files;
            List<string> NewFiles = DiffFiles(OldFiles, CurrentFiles);

            DateTime End = DateTime.UtcNow;
            TimeSpan Dur = End.Subtract(Start);
            Console.Write($"[{Global.BuildTime()}] ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[File Manager] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Built file list");
#if DEBUG
            Console.Write(" in ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(Dur.ToString("T").Replace(",", "."));
            Console.ForegroundColor = ConsoleColor.White;
#endif
            Console.WriteLine();

            Runtime.SaveCurrentRuntime();

            return NewFiles;
        }
    }
}
