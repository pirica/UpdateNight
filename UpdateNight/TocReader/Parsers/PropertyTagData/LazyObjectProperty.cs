using System;
using UpdateNight.TocReader.Parsers.Objects;

namespace UpdateNight.TocReader.Parsers.PropertyTagData
{
    public sealed class LazyObjectProperty : BaseProperty<object>
    {
        internal LazyObjectProperty(PackageReader reader, FPropertyTag tag)
        {
            throw new NotImplementedException(string.Format("Parsing of {0} types isn't supported yet.", "LazyObjectProperty"));
        }
    }
}
