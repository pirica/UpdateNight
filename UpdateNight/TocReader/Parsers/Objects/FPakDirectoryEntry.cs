using System.IO;

namespace UpdateNight.TocReader.Parsers.Objects
{
	public readonly struct FPakDirectoryEntry
	{
		public string Directory { get; }

		public FPathHashIndexEntry[] Entries { get; }

		internal FPakDirectoryEntry(BinaryReader reader)
		{
			Directory = reader.ReadFString();
			Entries = reader.ReadTArray(() => new FPathHashIndexEntry(reader));
		}
	}
}
