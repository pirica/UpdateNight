namespace UpdateNight.TocReader.Parsers.PropertyTagData
{
    public sealed class InterfaceProperty : BaseProperty<uint>
    {
        internal InterfaceProperty()
        {
            Value = 0;
        }
        // Value is ObjectRef
        internal InterfaceProperty(PackageReader reader)
        {
            Position = reader.Position;
            Value = reader.ReadUInt32();
        }

        public uint GetValue() => Value;
    }
}
