using UpdateNight.TocReader.Parsers.Objects;

namespace UpdateNight.TocReader.Parsers.PropertyTagData
{
    public sealed class ObjectProperty : BaseProperty<FPackageIndex>
    {
        internal ObjectProperty(PackageReader reader, int index)
        {
            Value = new FPackageIndex(reader, index);
        }

        internal ObjectProperty(PackageReader reader)
        {
            Position = reader.Position;
            Value = new FPackageIndex(reader);
        }

        public object GetValue() => Value.GetValue();
    }
}
