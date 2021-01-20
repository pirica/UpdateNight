using System.IO;

namespace UpdateNight.TocReader.Parsers.Objects
{
    public readonly struct FQuat : IUStruct
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;
        public readonly float W;

        internal FQuat(BinaryReader reader)
        {
            X = reader.ReadSingle();
            Y = reader.ReadSingle();
            Z = reader.ReadSingle();
            W = reader.ReadSingle();
        }
    }
}
