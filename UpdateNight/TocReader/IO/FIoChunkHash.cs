using System.IO;

namespace UpdateNight.TocReader.IO
{
    public struct FIoChunkHash
    {
        public byte[] Hash;

        public FIoChunkHash(BinaryReader reader)
        {
            Hash = reader.ReadBytes(32);
        }
    }
}