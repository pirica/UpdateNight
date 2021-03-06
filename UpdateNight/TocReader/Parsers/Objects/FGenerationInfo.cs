using System.IO;

namespace UpdateNight.TocReader.Parsers.Objects
{
    public readonly struct FGenerationInfo
    {
        public readonly int ExportCount;
        public readonly int NameCount;

        internal FGenerationInfo(BinaryReader reader)
        {
            ExportCount = reader.ReadInt32();
            NameCount = reader.ReadInt32();
        }
    }
}
