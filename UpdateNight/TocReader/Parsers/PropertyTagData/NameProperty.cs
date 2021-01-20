using UpdateNight.TocReader.Parsers.Objects;

namespace UpdateNight.TocReader.Parsers.PropertyTagData
{
    public sealed class NameProperty : BaseProperty<FName>
    {
        internal NameProperty()
        {
            Value = new FName();
        }
        internal NameProperty(PackageReader reader)
        {
            Position = reader.Position;
            Value = reader.ReadFName();
        }

        public string GetValue() => Value.String;
    }
}
