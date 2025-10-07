using System;
using System.Collections.Generic;

namespace FontReader.Framework.NameTable;

/// <summary>
/// 用于记录字体 name 表的详细信息的类
/// </summary>
public class NameTable : ITable
{
    /// <inheritdoc/>
    public uint CheckSum { get; }

    /// <inheritdoc/>
    public uint Offset { get; }

    /// <inheritdoc/>
    public uint Length { get; }

    /// <summary>
    /// name 表下的各条信息记录
    /// </summary>
    public readonly List<NameTableRecord> Records = new();

    /// <summary>
    /// name 表的原始二进制数据
    /// </summary>
    public byte[] Bytes = Array.Empty<byte>();

    public NameTable(uint checkSum, uint offset, uint length)
    {
        CheckSum = checkSum;
        Offset = offset;
        Length = length;
    }
}