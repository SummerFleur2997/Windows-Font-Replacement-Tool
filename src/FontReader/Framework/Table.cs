namespace FontReader.Framework;

/// <summary>
/// 用于记录字体表的基础信息的类
/// </summary>
internal struct Table : ITable
{
    /// <summary>
    /// 表的名称
    /// </summary>
    public string Tag { get; }

    /// <inheritdoc/>
    public uint CheckSum { get; }

    /// <inheritdoc/>
    public uint Offset { get; }

    /// <inheritdoc/>
    public uint Length { get; }

    public Table(string tag, uint checkSum, uint offset, uint length)
    {
        Tag = tag;
        CheckSum = checkSum;
        Offset = offset;
        Length = length;
    }
}