using UpdateNight.TocReader.IO;
using System.Linq;

namespace UpdateNight.Source
{
    class Toc
    {
        public static IoPackage GetAsset(string path)
        {
            path = path.Split(".").First();

            if (path.StartsWith("/Game/"))
                path = path.Replace("/Game", "/FortniteGame/Content");
            else if (!path.StartsWith("/FortniteGame/"))
            {
                var split = path.Split("/").ToList();
                split.RemoveAt(0);
                path = $"/FortniteGame/Plugins/GameFeatures/{split.ElementAt(0)}/Content/";
                split.RemoveAt(0);
                path += string.Join("/", split);
            }

            if (!Global.assetmapping.ContainsKey(path))
                return null;

            FIoStoreEntry entry = Global.assetmapping.First(p => p.Key == path).Value;

            return GetAsset(entry);
        }

        public static IoPackage GetAsset(FIoStoreEntry entry)
        {
            byte[] uasset = entry.GetData();
            byte[] ubulk = (entry.Ubulk as FIoStoreEntry)?.GetData();

            return new IoPackage(uasset, ubulk);
        }
    }
}
