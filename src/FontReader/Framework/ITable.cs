namespace FontReader.Framework;

public interface ITable
{
    /// <summary>
    /// 表的数据校验和
    /// </summary>
    public uint CheckSum { get; }

    /// <summary>
    /// 表头距离文件头的偏移量
    /// </summary>
    public uint Offset { get; }

    /// <summary>
    /// 表的长度（字节）
    /// </summary>
    public uint Length { get; }
}