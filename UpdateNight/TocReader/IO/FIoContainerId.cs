using System.IO;

namespace UpdateNight.TocReader.IO
{
    public struct FIoContainerId
    {
        public ulong Id;

        public FIoContainerId(BinaryReader reader)
        {
            Id = reader.ReadUInt64();
        }
    }
}