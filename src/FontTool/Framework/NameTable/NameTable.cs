using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FontTool.Framework.NameTable;

/// <summary>
/// 用于记录字体 name 表的详细信息的类
/// </summary>
public class NameTable : Table
{
    /// <summary>
    /// name 表下的各条信息记录
    /// </summary>
    public readonly List<NameTableRecord> Records = new();

    public NameTable(uint checkSum, uint offset, uint length)
        : base("name", checkSum, offset, length) { }

    public void ReadRecords(BinaryReader reader)
    {
        reader.BaseStream.Seek(Offset, SeekOrigin.Begin);
        reader.ReadUInt16BigEndian(); // format
        var count = reader.ReadUInt16BigEndian();
        var stringOffset = reader.ReadUInt16BigEndian();

        // 读取 name 表内每条记录的索引
        for (var i = 0; i < count; i++)
            Records.Add(new NameTableRecord
            {
                PlatformID = reader.ReadUInt16BigEndian(),
                EncodingID = reader.ReadUInt16BigEndian(),
                LanguageID = reader.ReadUInt16BigEndian(),
                NameID = reader.ReadUInt16BigEndian(),
                Length = reader.ReadUInt16BigEndian(),
                StringDataOffset = reader.ReadUInt16BigEndian()
            });

        // 读取每一条记录对应的二进制数据
        foreach (var record in Records)
        {
            // 计算文字偏移量
            var stringPos = Offset + stringOffset + record.StringDataOffset;
            reader.BaseStream.Seek(stringPos, SeekOrigin.Begin);
            var stringBytes = reader.ReadBytes(record.Length);

            // 将二进制数据编解码为文本信息
            record.StringData = record.PlatformID switch
            {
                1 => Encoding.GetEncoding("GBK").GetString(stringBytes),
                3 => Encoding.BigEndianUnicode.GetString(stringBytes),
                _ => Encoding.Default.GetString(stringBytes)
            };
        }
    }
}