using System.IO;

namespace UpdateNight.TocReader.Parsers.Objects
{
    public readonly struct FCustomVersion
    {
        public readonly FGuid Key;
        public readonly int Version;
        //public readonly int ReferenceCount; unused in serialization

        internal FCustomVersion(BinaryReader reader)
        {
            Key = new FGuid(reader);
            Version = reader.ReadInt32();
        }
    }
}
