using System.IO;

namespace UpdateNight.TocReader.Parsers.Objects
{
    public readonly struct FSHAHash
    {
        public readonly byte[] Hash;

        internal FSHAHash(BinaryReader reader)
        {
            Hash = reader.ReadBytes(20);
        }
    }
}
