using System.Collections.Generic;
using System.IO;

namespace FontReader.Framework;

public interface ITable
{
    /// <summary>
    /// 表的名称
    /// </summary>
    public string Tag { get; }

    /// <summary>
    /// 表的数据校验和
    /// </summary>
    public uint CheckSum { get; set; }

    /// <summary>
    /// 表头距离文件头的偏移量
    /// </summary>
    public uint Offset { get; set; }

    /// <summary>
    /// 表的长度（字节）
    /// </summary>
    public uint Length { get; set; }

    /// <summary>
    /// 表的原始二进制数据
    /// </summary>
    public List<byte> Bytes { get; }

    /// <summary>
    /// 通过文件流向表中写入二进制数据
    /// </summary>
    public void LoadBytes(BinaryReader reader);

    /// <summary>
    /// 通过给定数据向表中写入二进制数据
    /// </summary>
    public void LoadBytes(byte[] bytes);

    public void UpdateValue(uint? checkSum = null, uint? offset = null, uint? length = null);

    /// <summary>
    /// 计算字节数组的校验和
    /// </summary>
    /// <returns>返回计算得到的校验和值</returns>
    public uint CalculateCheckSum();
}