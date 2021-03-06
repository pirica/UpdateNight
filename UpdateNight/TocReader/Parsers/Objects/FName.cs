using Newtonsoft.Json;
using UsmapNET.Classes;

namespace UpdateNight.TocReader.Parsers.Objects
{
    public readonly struct FName
    {
        //readonly FNameEntrySerialized Name;
        [JsonIgnore]
        public readonly int Index;
        [JsonIgnore]
        public readonly int Number;

        public readonly string String;

        [JsonIgnore]
        public bool IsNone => String == null || String == "None";

        internal FName(FNameEntrySerialized name, int index, int number)
        {
            //Name = name;
            String = name.Name;
            Index = index;
            Number = number;
        }

        public FName(UsmapPropertyData name, int index = 0, int number = 0)
        {
            String = name != null ? name.Type.ToString() : "None";
            Index = index;
            Number = number;
        }

        public FName(string name, int index = 0, int number = 0)
        {
            String = name;
            Index = index;
            Number = number;
        }

        public override string ToString() => String;
    }
}
