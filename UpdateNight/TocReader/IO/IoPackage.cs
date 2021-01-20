using System.IO;
using UpdateNight.TocReader.Parsers;
using UpdateNight.TocReader.Parsers.Class;
using UpdateNight.TocReader.Parsers.Objects;
using Newtonsoft.Json;

namespace UpdateNight.TocReader.IO
{
    public class IoPackage : Package
    {
        private readonly byte[] UAsset;
        private readonly byte[] UBulk;
        private IoPackageReader _reader;
        private string _jsonData = null;

        internal IoPackage(byte[] asset, byte[] bulk)
        {
            UAsset = asset;
            UBulk = bulk;
        }

        public IoPackageReader Reader
        {
            get
            {
                if (_reader == null)
                {
                    var asset = new MemoryStream(UAsset);
                    var bulk = UBulk != null ? new MemoryStream(UBulk) : null;
                    asset.Position = 0;
                    if (bulk != null)
                        bulk.Position = 0;

                    return _reader = new IoPackageReader(asset, bulk, Global.GlobalData, true);
                }

                return _reader;
            }
        }

        public override string JsonData
        {
            get
            {
                if (string.IsNullOrEmpty(_jsonData))
                {
                    var ret = new JsonExport[Exports.Length];
                    for (int i = 0; i < ret.Length; i++)
                    {
                        ret[i] = new JsonExport
                        {
                            ExportType = ExportTypes[i].String,
                            ExportValue = Exports[i].GetJsonDict()
                        };
                    }
#if DEBUG
                    return JsonConvert.SerializeObject(ret, Formatting.Indented); 
#else
                    return _jsonData = JsonConvert.SerializeObject(ret, Formatting.Indented);
#endif
                }
                return _jsonData;
            }
        }
        public override FName[] ExportTypes => Reader.DataExportTypes;
        public override IUExport[] Exports => Reader.DataExports;
    }
}