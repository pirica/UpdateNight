using UpdateNight.TocReader.IO;

namespace UpdateNight.TocReader.Parsers.Objects
{
    readonly struct FPropertyTag
    {
        public readonly int ArrayIndex;
        public readonly long Position;
        public readonly byte BoolVal;
        public readonly FName EnumName;
        public readonly FName EnumType;
        public readonly byte HasPropertyGuid;
        public readonly FName InnerType;
        public readonly FName Name;
        //public readonly UProperty* Prop; // Transient
        public readonly FGuid PropertyGuid;
        public readonly int Size;
        public readonly long SizeOffset; // TODO: not set, check code
        public readonly FGuid StructGuid;
        public readonly FName StructName;
        public readonly FName Type; // Variables
        public readonly FName ValueType;

        public FPropertyTag(FUnversionedProperty info)
        {
            BoolVal = 0;

            Name = new FName(info.Name);
            Type = new FName(info.Data);
            StructName = new FName(info.Data.StructType);
            EnumName = new FName(info.Data.EnumName);
            EnumType = new FName(info.Data.InnerType);
            InnerType = new FName(info.Data.InnerType);
            ValueType = new FName(info.Data.ValueType);

            if (InnerType.String == "StructProperty")
            {
                StructName = new FName(info.Data.InnerType.StructType);
            }
            else if (ValueType.String == "StructProperty")
            {
                StructName = new FName(info.Data.ValueType.StructType);
            }

            ArrayIndex = 0;
            Position = 0;
            HasPropertyGuid = 0;
            PropertyGuid = default;
            Size = 0;
            SizeOffset = 0;
            StructGuid = default;
        }
        internal FPropertyTag(PackageReader reader)
        {
            ArrayIndex = 0;
            Position = 0; // default
            BoolVal = 0;
            EnumName = default;
            EnumType = default;
            HasPropertyGuid = 0;
            InnerType = default;
            Name = default;
            PropertyGuid = default;
            Size = 0;
            SizeOffset = 0;
            StructGuid = default;
            StructName = default;
            Type = default;
            ValueType = default;

            Name = reader.ReadFName();
            if (Name.IsNone)
                return;

            Type = reader.ReadFName();
            Size = reader.ReadInt32();
            ArrayIndex = reader.ReadInt32();

            Position = reader.Position; // actual
            if (Type.Number == 0)
            {
                switch (Type.String)
                {
                    case "StructProperty":
                        StructName = reader.ReadFName();
                        // Serialize if version is past VER_UE4_STRUCT_GUID_IN_PROPERTY_TAG
                        StructGuid = new FGuid(reader);
                        break;
                    case "BoolProperty":
                        BoolVal = reader.ReadByte();
                        break;
                    case "ByteProperty":
                    case "EnumProperty":
                        EnumName = reader.ReadFName();
                        break;
                    case "ArrayProperty":
                        // Serialize if version is past VAR_UE4_ARRAY_PROPERTY_INNER_TAGS
                        InnerType = reader.ReadFName();
                        break;
                    // Serialize the following if version is past VER_UE4_PROPERTY_TAG_SET_MAP_SUPPORT
                    case "SetProperty":
                        InnerType = reader.ReadFName();
                        break;
                    case "MapProperty":
                        InnerType = reader.ReadFName();
                        ValueType = reader.ReadFName();
                        break;
                }
            }

            // Property tags to handle renamed blueprint properties effectively.
            // Serialize if version is past VER_UE4_PROPERTY_GUID_IN_PROPERTY_TAG
            HasPropertyGuid = reader.ReadByte();
            if (HasPropertyGuid != 0)
            {
                PropertyGuid = new FGuid(reader);
            }
        }
    }
}
