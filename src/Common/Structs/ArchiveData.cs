using System;

namespace Common.Structs
{
    public struct ArchiveData
    {
        public ArchiveData(string sourceFileName, BlockOfArchive[] blocks, long sourceFileSize)
        {
            SourceFileName = sourceFileName;
            Blocks = blocks;
            SourceFileSize = sourceFileSize;
            Timestamp = DateTime.Now;
        }
        
        public string SourceFileName { get; set; }
        public long SourceFileSize { get; set; }
        public BlockOfArchive[] Blocks { get; set; }
        public DateTime Timestamp { get; set; }
    }
}