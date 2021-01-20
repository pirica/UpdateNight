using UpdateNight.TocReader.Parsers.Objects;

namespace UpdateNight.TocReader.IO
{
    public class FExportDesc
    {
        public FPackageDesc Package = null;
        public FName Name;
        public FName FullName;
        public FPackageObjectIndex OuterIndex;
        public FPackageObjectIndex ClassIndex;
        public FPackageObjectIndex SuperIndex;
        public FPackageObjectIndex TemplateIndex;
        public FPackageObjectIndex GlobalImportIndex;
    }
}