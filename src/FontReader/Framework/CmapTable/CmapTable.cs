namespace FontReader.Framework.CmapTable;

public class CmapTable : ITable
{
    /// <inheritdoc/>
    public uint CheckSum { get; }

    /// <inheritdoc/>
    public uint Offset { get; }

    /// <inheritdoc/>
    public uint Length { get; }

    /// <summary>
    /// 解析 CJK 字符集数量时，目标子表相对于表头的偏移量。
    /// </summary>
    public uint SubtableOffset { get; set; }

    public CmapTable(uint checkSum, uint offset, uint length)
    {
        CheckSum = checkSum;
        Offset = offset;
        Length = length;
        SubtableOffset = 0;
    }
}