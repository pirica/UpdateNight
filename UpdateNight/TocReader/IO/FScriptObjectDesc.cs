using UpdateNight.TocReader.Parsers.Objects;

namespace UpdateNight.TocReader.IO
{
    public class FScriptObjectDesc
    {
        public readonly FName Name;
        public FName FullName;
        public readonly FPackageObjectIndex GlobalImportIndex;
        public readonly FPackageObjectIndex OuterIndex;

        public FScriptObjectDesc(FNameEntrySerialized name, FMappedName fMappedName, FScriptObjectEntry fScriptObjectEntry)
        {
            Name = new FName(name.Name, (int)fMappedName.Index, (int)fMappedName.Number);
            FullName = default;
            GlobalImportIndex = fScriptObjectEntry.GlobalIndex;
            OuterIndex = fScriptObjectEntry.OuterIndex;
        }
    }
}
