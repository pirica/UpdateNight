using System;

namespace UpdateNight.TocReader.Parsers.Objects
{
    public readonly struct FBodyInstance : IUStruct
    {
        internal FBodyInstance(PackageReader reader)
        {
            throw new NotImplementedException(string.Format("Parsing of {0} types isn't supported yet.", "FBodyInstance"));
        }
    }
}
