namespace UpdateNight.TocReader.IO
{
    public enum FIoStoreTocEntryMetaFlags : byte
    {
        None,
        Compressed		= (1 << 0),
        MemoryMapped	= (1 << 1)
    }
}