using UpdateNight.TocReader.Parsers.Objects;

namespace UpdateNight.TocReader.Parsers.PropertyTagData
{
    public sealed class FieldPathProperty : BaseProperty<FFieldClass[]>
    {
        internal FieldPathProperty()
        {
            Value = null;
        }
        internal FieldPathProperty(PackageReader reader)
        {
            Position = reader.Position;
            Value = reader.ReadTArray(() => new FFieldClass(reader));
        }

        public string GetValue() => null;
    }
}
