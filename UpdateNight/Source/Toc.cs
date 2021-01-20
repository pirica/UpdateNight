using System;
using System.Collections.Generic;
using System.Text;
using UpdateNight.TocReader.IO;
using System.Linq;

namespace UpdateNight.Source
{
    class Toc
    {
        public static IoPackage GetAsset(string path)
        {
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
