using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FontReader.Framework;

/// <summary>
/// 用于记录字体表的基础信息的类
/// </summary>
public class Table : ITable, IDisposable
{
    /// <inheritdoc/>
    public string Tag { get; }

    /// <inheritdoc/>
    public uint CheckSum { get; set; }

    /// <inheritdoc/>
    public uint Offset { get; set; }

    /// <inheritdoc/>
    public uint Length { get; set; }

    /// <inheritdoc/>
    public List<byte> Bytes { get; private set; } = new();

    public Table(string tag, uint checkSum, uint offset, uint length)
    {
        Tag = tag;
        CheckSum = checkSum;
        Offset = offset;
        Length = length;
    }

    public void LoadBytes(BinaryReader reader)
    {
        if (Bytes.Count > 0) return;
        reader.BaseStream.Seek(Offset, SeekOrigin.Begin);
        Bytes = reader.ReadBytes((int)Length).ToList();
    }

    public void LoadBytes(byte[] bytes) => Bytes = bytes.ToList();

    public void UpdateValue(uint? checkSum = null, uint? offset = null, uint? length = null)
    {
        CheckSum = checkSum ?? CheckSum;
        Offset = offset ?? Offset;
        Length = length ?? Length;
    }

    public uint CalculateCheckSum() => CalculateCheckSum(Bytes);

    public static uint CalculateCheckSum(List<byte> bytes)
    {
        uint sum = 0; // 初始化校验和为0
        var chips = (bytes.Count + 3) / 4; // 向上取整到4字节的倍数

        for (var i = 0; i < chips; i++)
        {
            var offset = i * 4;
            uint value = 0;

            // 处理不足4字节的情况
            if (offset < bytes.Count)
            {
                value = (uint)(bytes[offset] << 24);
                if (offset + 1 < bytes.Count)
                    value |= (uint)(bytes[offset + 1] << 16);
                if (offset + 2 < bytes.Count)
                    value |= (uint)(bytes[offset + 2] << 8);
                if (offset + 3 < bytes.Count)
                    value |= bytes[offset + 3];
            }

            sum += value;
        }

        return sum;
    }

    public void Dispose()
    {
        Bytes.Clear();
        GC.SuppressFinalize(this);
    }
}