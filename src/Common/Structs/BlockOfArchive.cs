namespace Common.Structs
{
    public struct BlockOfArchive
    {
        public BlockOfArchive(int index, long size)
        {
            Index = index;
            Size = size;
        }
        public int Index { get; set; }
        public long Size { get; set; }
    }
}