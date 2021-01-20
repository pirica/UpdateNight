using System.IO;

namespace UpdateNight.TocReader.Parsers.Objects
{
    public readonly struct FIntPoint : IUStruct
    {
        public readonly int X;
        public readonly int Y;

        internal FIntPoint(BinaryReader reader)
        {
            X = reader.ReadInt32();
            Y = reader.ReadInt32();
        }
    }
}
