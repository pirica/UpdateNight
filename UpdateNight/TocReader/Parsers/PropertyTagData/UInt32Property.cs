namespace UpdateNight.TocReader.Parsers.PropertyTagData
{
    public sealed class UInt32Property : BaseProperty<uint>
    {
        internal UInt32Property()
        {
            Value = 0;
        }
        internal UInt32Property(PackageReader reader)
        {
            Position = reader.Position;
            Value = reader.ReadUInt32();
        }

        public uint GetValue() => Value;
    }
}
