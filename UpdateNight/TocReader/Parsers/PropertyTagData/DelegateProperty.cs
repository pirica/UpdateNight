using System.Collections.Generic;
using UpdateNight.TocReader.Parsers.Objects;

namespace UpdateNight.TocReader.Parsers.PropertyTagData
{
    public sealed class DelegateProperty : BaseProperty
    {
        public int Object;
        public FName Name;

        internal DelegateProperty()
        {
            Object = 0;
            Name = new FName();
        }

        internal DelegateProperty(PackageReader reader)
        {
            Object = reader.ReadInt32();
            Name = reader.ReadFName();
        }

        public Dictionary<string, object> GetValue() => new Dictionary<string, object> { ["Object"] = Object, ["Name"] = Name.String };
    }
}
