using System.IO;

namespace UpdateNight.TocReader.Parsers.Objects
{
    public readonly struct FMD5Hash : IUStruct
    {
        public readonly byte[] Hash;

        internal FMD5Hash(BinaryReader reader)
        {
            Hash = reader.ReadUInt32() != 0 ? reader.ReadBytes(16) : null;
        }
    }
}
