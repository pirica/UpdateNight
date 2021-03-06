using System.IO;
using System.Text;
using UpdateNight.TocReader.Parsers.Objects;

namespace UpdateNight.TocReader.IO
{
    public enum EIoStoreTocReadOptions
    {
        Default,
        ReadDirectoryIndex = 1 << 0,
        ReadTocMeta        = 1 << 1,
        ReadAll            = ReadDirectoryIndex | ReadTocMeta
    }

    public class FIoStoreTocResource
    {
        public readonly FIoStoreTocHeader Header;
        public readonly FIoChunkId[] ChunkIds;
        public readonly FIoOffsetAndLength[] ChunkOffsetLengths;
        public readonly FIoStoreTocCompressedBlockEntry[] CompressionBlocks;
        public readonly string[] CompressionMethods;
        public readonly FSHAHash[] ChunkBlockSignatures;
        public readonly byte[] DirectoryIndexBuffer;
        public readonly FIoStoreTocEntryMeta[] ChunkMetas;
        
        public FIoStoreTocResource(Stream tocStream, EIoStoreTocReadOptions readOptions = EIoStoreTocReadOptions.Default)
        {
            var streamBuffer = new byte[tocStream.Length];
            tocStream.Read(streamBuffer, 0, streamBuffer.Length);
            using var reader = new BinaryReader(new MemoryStream(streamBuffer));
            Header = new FIoStoreTocHeader(reader);
            reader.BaseStream.Position = Header.TocHeaderSize;

            var totalTocSize = tocStream.Length - Header.TocHeaderSize;
            var tocMetaSize = Header.TocEntryCount * FIoStoreTocEntryMeta.SIZE;
            var defaultTocSize = totalTocSize - Header.DirectoryIndexSize - tocMetaSize;

            var tocSize = defaultTocSize;
            if (readOptions.HasAnyFlags(EIoStoreTocReadOptions.ReadTocMeta))
            {
                tocSize = totalTocSize; // Meta data is at the end of the TOC file
            }

            if (readOptions.HasAnyFlags(EIoStoreTocReadOptions.ReadDirectoryIndex))
            {
                tocSize = defaultTocSize + Header.DirectoryIndexSize;
            }

            // Chunk IDs
            ChunkIds = new FIoChunkId[Header.TocEntryCount];
            for (var i = 0; i < Header.TocEntryCount; i++)
            {
                ChunkIds[i] = new FIoChunkId(reader);
            }

            // Chunk offsets
            ChunkOffsetLengths = new FIoOffsetAndLength[Header.TocEntryCount];
            for (var i = 0; i < Header.TocEntryCount; i++)
            {
                ChunkOffsetLengths[i] = new FIoOffsetAndLength(reader);
            }

            // Compression blocks
            CompressionBlocks = new FIoStoreTocCompressedBlockEntry[Header.TocCompressedBlockEntryCount];
            for (var i = 0; i < Header.TocCompressedBlockEntryCount; i++)
            {
                CompressionBlocks[i] = new FIoStoreTocCompressedBlockEntry(reader);
            }

            // Compression methods
            CompressionMethods = new string[Header.CompressionMethodNameCount]; // Not doing +1 nor adding CompressionMethod none here since the FPakInfo implementation doesn't as well
            for (var i = 0; i < Header.CompressionMethodNameCount; i++)
            {
                CompressionMethods[i] = Encoding.ASCII.GetString(reader.ReadBytes((int) Header.CompressionMethodNameLength)).TrimEnd('\0');
            }

            // Chunk block signatures
            if (Header.ContainerFlags.HasAnyFlags(EIoContainerFlags.Signed))
            {
                var hashSize = reader.ReadInt32();
                reader.BaseStream.Position += hashSize; // actually: var tocSignature = reader.ReadBytes(hashSize);
                reader.BaseStream.Position += hashSize; // actually: var blockSignature = reader.ReadBytes(hashSize);
                
                ChunkBlockSignatures = new FSHAHash[Header.TocCompressedBlockEntryCount];
                for (var i = 0; i < Header.TocCompressedBlockEntryCount; i++)
                {
                    ChunkBlockSignatures[i] = new FSHAHash(reader);
                }

                // You could verify hashes here but nah
            }

            // Directory index
            if (Header.Version >= EIoStoreTocVersion.DirectoryIndex &&
                readOptions.HasAnyFlags(EIoStoreTocReadOptions.ReadDirectoryIndex) &&
                Header.ContainerFlags.HasAnyFlags(EIoContainerFlags.Indexed) && 
                Header.DirectoryIndexSize > 0)
            {
                DirectoryIndexBuffer = reader.ReadBytes((int) Header.DirectoryIndexSize);
            }
            
            // Meta
            if (readOptions.HasAnyFlags(EIoStoreTocReadOptions.ReadTocMeta))
            {
                ChunkMetas = new FIoStoreTocEntryMeta[Header.TocEntryCount];
                for (var i = 0; i < Header.TocEntryCount; i++)
                {
                    ChunkMetas[i] = new FIoStoreTocEntryMeta(reader);
                }
            }
        }
    }
}