using System.IO;

namespace UpdateNight.TocReader.IO
{
    public readonly struct FArc
    {
        public readonly uint FromNodeIndex;
        public readonly uint ToNodeIndex;

        public FArc(BinaryReader reader)
        {
            FromNodeIndex = reader.ReadUInt32();
            ToNodeIndex = reader.ReadUInt32();
        }
    }
}