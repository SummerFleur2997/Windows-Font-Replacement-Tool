using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FontReader.Framework;
using FontReader.Framework.CmapTable;
using FontReader.Framework.NameTable;

namespace FontReader;

/// <summary>
/// 用于读取字体文件并获取其基础信息（尤其是 name 表信息）的库。
/// </summary>
public class Font : IDisposable
{
    /// <summary>
    /// 字体文件的路径
    /// </summary>
    public readonly string FontPath;

    /// <summary>
    /// name 表，包含字体名称相关信息
    /// </summary>
    public NameTable? NameTable => FontTables.First(t => t.Tag == "name") as NameTable;

    /// <summary>
    /// cmap 表
    /// </summary>
    public CmapTable? CmapTable => FontTables.First(t => t.Tag == "cmap") as CmapTable;

    /// <summary>
    /// 用于存储字体文件各个表的基础信息的列表。
    /// </summary>
    public readonly List<Table> FontTables;

    /// <summary>
    /// 用于读取二进制数据的读取器
    /// </summary>
    public BinaryReader Reader { get; }

    public readonly uint Sfnt;

    public readonly byte[] Header;

    /// <summary>
    /// 读取字体文件，解析其关键信息。
    /// </summary>
    /// <exception cref="FileNotFoundException">未能找到字体文件。</exception>
    /// <exception cref="NotSupportedException">字体格式不受支持。</exception>
    /// <exception cref="FileLoadException">损坏的字体文件。</exception>
    public Font(string fontPath, FileStream? stream = null)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // 检查文件是否存在，
        if (fontPath != "/@stream" && !File.Exists(fontPath))
            throw new FileNotFoundException("未找到文件！", fontPath);
        FontPath = fontPath;

        // 初始化数据表和二进制流
        stream ??= new FileStream(fontPath, FileMode.Open, FileAccess.Read);
        Reader = new BinaryReader(stream);

        // 读取表目录信息
        Sfnt = Reader.ReadUInt32BigEndian();
        if (Sfnt is not (0x00010000 or 0x4f54544f))
            throw new NotSupportedException("Unsupported font format!");

        // 准备开始读取字体
        var numTables = Reader.ReadUInt16BigEndian();
        Header = Reader.ReadBytes(6);

        // 初始化所有表信息
        FontTables = new List<Table>();
        for (var i = 0; i < numTables; i++)
        {
            var tag = Reader.ReadString(4);
            var checksum = Reader.ReadUInt32BigEndian();
            var offset = Reader.ReadUInt32BigEndian();
            var length = Reader.ReadUInt32BigEndian();

            var table = tag switch
            {
                "name" => new NameTable(checksum, offset, length),
                "cmap" => new CmapTable(checksum, offset, length),
                _ => new Table(tag, checksum, offset, length)
            };
            FontTables.Add(table);
        }

        // 检测关键表是否存在
        if (NameTable is null || CmapTable is null)
            throw new FileLoadException("The font file is corrupted!");

        // 向 NameTable 中写入原始二进制数据
        Reader.BaseStream.Seek(NameTable.Offset, SeekOrigin.Begin);
        NameTable.LoadBytes(Reader.ReadBytes((int)NameTable.Length));

        // 解析 name 表的表头
        NameTable.ReadRecords(Reader);
    }

    public string FontFamily(bool showSubfamily = false)
    {
        var family = NameTable?.Records
            .FirstOrDefault(t => t is { NameID: 1, LanguageID: 0x409 })?
            .StringData ?? "";

        var subfamily = NameTable?.Records
            .FirstOrDefault(t => t is { NameID: 2, LanguageID: 0x409 })?
            .StringData;

        return showSubfamily
            ? string.Concat(family, "-", subfamily)
            : family;
    }

    public void LoadAllTableData()
    {
        foreach (var table in FontTables)
            table.LoadBytes(Reader);
    }

    /// <summary>
    /// 获取指定 Unicode 范围内的字符数量
    /// </summary>
    /// <param name="start">Unicode 范围的起始值</param>
    /// <param name="end">Unicode 范围的结束值</param>
    /// <returns>指定范围内的有效字符数量</returns>
    /// <exception cref="NotSupportedException">文件不受支持。</exception>
    public uint GetCharacterCountFromTo(uint start, uint end)
    {
        // 读取 cmap 表头
        Reader.BaseStream.Seek(CmapTable!.Offset, SeekOrigin.Begin);
        Reader.ReadUInt16BigEndian(); // version
        var numSubtables = Reader.ReadUInt16BigEndian(); // 获取子表数量

        // 遍历子表目录，寻找最佳子表
        var cmapSubTables = new List<CmapSubTable>();
        for (var i = 0; i < numSubtables; i++)
        {
            var platformID = Reader.ReadUInt16BigEndian();
            var encodingID = Reader.ReadUInt16BigEndian();
            var subtableOffset = Reader.ReadUInt32BigEndian();
            var precedence = (platformID, encodingID) switch
            {
                (3, 1) => 1, // Windows Unicode BMP
                (3, 10) => 2, // Windows Unicode Full
                (0, 3) => 3, // Unicode BMP
                (0, 4) => 4, // Unicode Full
                _ => 100 // 其他情况优先级最低
            };
            cmapSubTables.Add(new CmapSubTable(platformID, encodingID, subtableOffset, precedence));
        }

        // 按优先级排序并遍历子表，寻找 Format 4 的子表
        foreach (var subTable in cmapSubTables.OrderBy(t => t.Precedence))
        {
            Reader.BaseStream.Seek(CmapTable.Offset + subTable.TableDataOffset, SeekOrigin.Begin);
            var format = Reader.ReadUInt16BigEndian();
            if (format == 4) goto ReadAndPhase;
        }

        // 如果没有找到格式4的子表，抛出不支持异常
        throw new NotSupportedException("不支持的字体文件！");

        ReadAndPhase:
        // 读取 Format 4 的表头
        Reader.ReadUInt16BigEndian(); // length
        Reader.ReadUInt16BigEndian(); // language
        var segCountX2 = Reader.ReadUInt16BigEndian();
        var segCount = segCountX2 / 2;

        // 跳过二分查找字段
        Reader.BaseStream.Seek(6, SeekOrigin.Current);

        // 预加载 endCode 数组，这个数组存储了每个字符段的结束 Unicode 码点
        var endCodes = new ushort[segCount];
        for (var i = 0; i < segCount; i++)
            endCodes[i] = Reader.ReadUInt16BigEndian();

        // 读取并丢弃 2 字节的保留字段，用于确保数据对齐
        Reader.ReadUInt16();

        // 预加载 startCode 数组，这个数组存储了每个字符段的开始 Unicode 码点，与 endCode 数组一一对应
        var startCodes = new ushort[segCount];
        for (var i = 0; i < segCount; i++)
            startCodes[i] = Reader.ReadUInt16BigEndian();

        // 预加载 idDelta 数组。这个数组存储了用于计算字形索引的增量值。对于简单映射，字形索引 = (charCode + idDelta) % 65536
        var idDeltas = new short[segCount];
        for (var i = 0; i < segCount; i++)
            idDeltas[i] = (short)Reader.ReadUInt16BigEndian();

        // 预加载 idRangeOffset 数组。当它不为 0 时，表示该段的映射是“复杂的”，字形索引需要从另一个数组（glyphIndexArray）中查找
        // 它的值是一个偏移量，指向该段第一个字符对应字形索引的位置
        var idRangeOffsets = new ushort[segCount];
        for (var i = 0; i < segCount; i++)
            idRangeOffsets[i] = Reader.ReadUInt16BigEndian();

        // 记录 glyphIndexArray 的起始位置。
        var glyphIndexArrayPos = Reader.BaseStream.Position;

        // 遍历目标区域文字的 Unicode，统计目标区域字符
        uint glyphCount = 0;
        for (var charCode = start; charCode <= end; charCode++)
        {
            // 使用二分查找来定位字符所在的段
            int min = 0, max = segCount - 1;
            while (min <= max)
            {
                var mid = (min + max) / 2;

                if (charCode <= endCodes[mid])
                    max = mid - 1;
                else
                    min = mid + 1;
            }

            // 验证字符是否在有效段内
            if (min >= segCount || charCode < startCodes[min])
                continue;

            // 简单映射，判断索引是否为 0，若索引为 0，则当前字符无效
            if (idRangeOffsets[min] == 0 && (ushort)(charCode + idDeltas[min]) != 0)
            {
                glyphCount++;
            }
            // 复杂映射，需要从 glyphIndexArray 中动态查找字形索引。
            else
            {
                var offset = glyphIndexArrayPos + 2 * (min - segCount) + idRangeOffsets[min];
                offset += 2 * (charCode - startCodes[min]);

                Reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                var glyphIndex = Reader.ReadUInt16BigEndian();
                if (glyphIndex != 0 && (ushort)(charCode + idDeltas[min]) != 0) glyphCount++;
            }
        }

        return glyphCount;
    }

    public void ReplaceNameTable(NameTable newTable)
    {
        // 不会触发
        if (NameTable == null) return;

        // 更新 NameTable 的值
        newTable.Offset = NameTable.Offset;
        FontTables[FontTables.IndexOf(NameTable)] = newTable;

        // 获取表副本
        var tables = FontTables.OrderBy(t => t.Offset).ToList();
        var nameTableIndex = tables.IndexOf(newTable);

        // 对齐和修正偏移量数据
        var offset = newTable.Offset;
        for (var i = nameTableIndex; i < tables.Count; i++)
        {
            var currentTable = tables[i];
            var length = currentTable.Length;
            if (length % 4 != 0) length += 4 - length % 4;

            FontTables[FontTables.IndexOf(currentTable)].Offset = offset;

            offset += length;
        }
    }

    /// <summary>
    /// 将字体数据保存到指定路径
    /// </summary>
    /// <param name="path">保存文件的路径</param>
    /// <param name="shouldUpdateTableInfo">是否应当更新表信息</param>
    /// <returns>保存成功返回true，失败返回false</returns>
    public bool Save(string path, bool shouldUpdateTableInfo = false)
    {
        var binary = new List<byte>();
        try
        {
            var body = new List<byte>();
            var tables = FontTables.OrderBy(t => t.Offset).ToList();
            foreach (var table in tables)
            {
                // 添加表的实际字节
                body.AddRange(table.Bytes);
                // 更新实际偏移量
                var padding = (4 - table.Length % 4) % 4;
                for (var i = 0; i < padding; i++) body.Add(0);
            }

            // 添加 sfnt 头部、字体表数量和字体头部的大端字节表示
            binary.AddRange(Sfnt.ToBigEndianBytes());
            binary.AddRange(((ushort)FontTables.Count).ToBigEndianBytes());
            binary.AddRange(Header);

            // 对于从 ttc 保存的情况，计算偏移量并更新表信息，然后添加每个表的信息（标签、校验和、偏移量、长度）
            if (shouldUpdateTableInfo)
            {
                long offset = binary.Count + FontTables.Count * 16;
                UpdateTableInfo(offset);
            }

            foreach (var table in FontTables)
            {
                binary.AddRange(table.Tag.ToBigEndianBytes());
                binary.AddRange(table.CheckSum.ToBigEndianBytes());
                binary.AddRange(table.Offset.ToBigEndianBytes());
                binary.AddRange(table.Length.ToBigEndianBytes());
            }

            binary.AddRange(body);
            File.WriteAllBytes(path, binary.ToArray());
            return true;
        }
        catch (Exception)
        {
            // 发生异常时返回false
            return false;
        }
    }

    private void UpdateTableInfo(long bodyOffset)
    {
        var offset = (uint)bodyOffset;
        foreach (var table in FontTables)
        {
            FontTables.First(t => t.Tag == table.Tag).UpdateValue(offset: offset);
            var length = table.Length;
            if (length % 4 != 0) length += 4 - length % 4;
            offset += length;
        }
    }

    public void Dispose()
    {
        foreach (var table in FontTables) table.Dispose();
        GC.SuppressFinalize(this);
    }
}