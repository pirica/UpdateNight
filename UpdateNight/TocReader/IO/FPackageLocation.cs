using UpdateNight.TocReader.Parsers.Objects;

namespace UpdateNight.TocReader.IO
{
    public readonly struct FPackageLocation
    {
        public readonly FName ContainerName;
        public readonly ulong Offset;
    }
}