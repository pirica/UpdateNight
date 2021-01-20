using System.IO;
using UpdateNight.TocReader.Parsers.Objects;

namespace UpdateNight.TocReader.IO
{
    public struct FFileIoStoreContainerFile
    {
        public Stream FileHandle;
        public long CompressionBlockSize;
        public string[] CompressionMethods;
        public FIoStoreTocCompressedBlockEntry[] CompressionBlocks;
        public FGuid EncryptionKeyGuid;
        public byte[] EncryptionKey;
        public EIoContainerFlags ContainerFlags;
        public FSHAHash[] BlockSignatureHashes;
        
        public long FileSize => FileHandle.Length;
    }
}