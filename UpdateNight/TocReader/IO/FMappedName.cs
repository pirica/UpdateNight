using UpdateNight.TocReader.Parsers;
using UpdateNight.TocReader.Parsers.Objects;

namespace UpdateNight.TocReader.IO
{
    public readonly struct FMappedName
    {
        public const uint InvalidIndex = ~0u;
        public const uint IndexBits = 30u;
        public const uint IndexMask = (1u << (int)IndexBits) - 1u;
        public const uint TypeMask = ~IndexMask;
        public const uint TypeShift = IndexBits;

        private readonly IoPackageReader _reader;

#pragma warning disable IDE1006 // fuck vs bop bop bop
        private readonly FNameEntrySerialized[] _globalNameMap =>
            _reader != null ? _reader.GlobalData.GlobalNameMap : __globalNameMap;
        private readonly FNameEntrySerialized[] __globalNameMap;
        private readonly FNameEntrySerialized[] _localNameMap =>
            _reader != null ? _reader.NameMap : __localNameMap;
        private readonly FNameEntrySerialized[] __localNameMap;
#pragma warning restore IDE1006

        public readonly uint Index;
        public readonly uint Number;

        public string String
        {
            get
            {
                var index = GetIndex();
                var nameMap = IsGlobal() ? _globalNameMap : _localNameMap;
                if (nameMap != null && index < _globalNameMap.Length)
                {
                    return nameMap[index].Name;
                }

                return null;
            }
        }

        public FMappedName(IoPackageReader reader)
        {
            Index = reader.ReadUInt32();
            Number = reader.ReadUInt32();
            _reader = reader;
            __globalNameMap = null;
            __localNameMap = null;
        }

        public FMappedName(FMinimalName minimalName, FNameEntrySerialized[] globalNameMap, FNameEntrySerialized[] localNameMap)
        {
            Index = minimalName.Index.Value;
            Number = (uint)minimalName.Number;
            _reader = null;
            __globalNameMap = globalNameMap;
            __localNameMap = localNameMap;
        }

        public uint GetIndex()
        {
            return Index & IndexMask;
        }

        public bool IsGlobal()
        {
            return (Index & TypeMask) >> (int)TypeShift != 0;
        }

        public override string ToString() => String;
    }
}