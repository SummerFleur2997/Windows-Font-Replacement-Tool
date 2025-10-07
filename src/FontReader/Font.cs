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
public class Font
{
    public readonly string FontPath;
    public readonly NameTable? NameTable;
    public readonly CmapTable? CmapTable;

    private readonly BinaryReader _binary;

    /// <summary>
    /// 用于存储字体文件各个表的基础信息的列表。
    /// </summary>
    private readonly List<Table> _fontTables;

    /// <summary>
    /// 读取字体文件，解析其关键信息。
    /// </summary>
    /// <param name="fontPath"></param>
    /// <exception cref="FileNotFoundException">未能找到字体文件。</exception>
    /// <exception cref="NotSupportedException">字体格式不受支持。</exception>
    /// <exception cref="FileLoadException">损坏的字体文件。</exception>
    public Font(string fontPath)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // 检查文件是否存在，
        if (!File.Exists(fontPath))
            throw new FileNotFoundException("未找到文件！", fontPath);
        FontPath = fontPath;

        // 初始化数据表和二进制流
        var stream = new FileStream(fontPath, FileMode.Open, FileAccess.Read);
        _binary = new BinaryReader(stream);

        // 读取表目录信息
        var header = _binary.ReadUInt32();
        if (header is not (0x00000100 or 0x4f54544f))
            throw new NotSupportedException("不支持的字体文件！");

        // 准备开始读取字体
        var numTables = _binary.ReadUInt16BigEndian();
        _binary.BaseStream.Seek(6, SeekOrigin.Current);

        // 初始化所有表信息
        _fontTables = new List<Table>();
        for (var i = 0; i < numTables; i++)
        {
            var tag = new string(_binary.ReadChars(4));
            var checksum = _binary.ReadUInt32BigEndian();
            var offset = _binary.ReadUInt32BigEndian();
            var length = _binary.ReadUInt32BigEndian();

            _fontTables.Add(new Table(tag, checksum, offset, length));

            switch (tag)
            {
                case "name":
                    NameTable = new NameTable(checksum, offset, length);
                    break;
                case "cmap":
                    CmapTable = new CmapTable(checksum, offset, length);
                    break;
            }
        }

        // 检测关键表是否存在
        if (NameTable is null || CmapTable is null)
            throw new FileLoadException("字体文件已损坏！");

        // 先向 NameTable 中写入原始二进制数据
        _binary.BaseStream.Seek(NameTable.Offset, SeekOrigin.Begin);
        NameTable.Bytes = _binary.ReadBytes((int)NameTable.Length);

        // 然后再解析 name 表的表头
        _binary.BaseStream.Seek(NameTable.Offset, SeekOrigin.Begin);
        _binary.ReadUInt16BigEndian(); // format
        var count = _binary.ReadUInt16BigEndian();
        var stringOffset = _binary.ReadUInt16BigEndian();

        // 读取 name 表内每条记录的索引
        for (var i = 0; i < count; i++)
            NameTable.Records.Add(new NameTableRecord
            {
                PlatformID = _binary.ReadUInt16BigEndian(),
                EncodingID = _binary.ReadUInt16BigEndian(),
                LanguageID = _binary.ReadUInt16BigEndian(),
                NameID = _binary.ReadUInt16BigEndian(),
                Length = _binary.ReadUInt16BigEndian(),
                StringDataOffset = _binary.ReadUInt16BigEndian()
            });

        // 读取每一条记录对应的二进制数据
        foreach (var record in NameTable.Records)
        {
            // 计算文字偏移量
            var stringPos = NameTable.Offset + stringOffset + record.StringDataOffset;
            _binary.BaseStream.Seek(stringPos, SeekOrigin.Begin);
            var stringBytes = _binary.ReadBytes(record.Length);

            // 将二进制数据编解码为文本信息
            record.StringData = record.PlatformID switch
            {
                1 => Encoding.GetEncoding("GBK").GetString(stringBytes),
                3 => Encoding.BigEndianUnicode.GetString(stringBytes),
                _ => Encoding.Default.GetString(stringBytes)
            };
        }
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

    /// <summary>
    /// 获取 CJK 统一表意字符（0x4E00~0x9FFF）范围内的字符数量。
    /// </summary>
    /// <exception cref="NotSupportedException">文件不受支持。</exception>
    public uint GetCjkCharacterCount()
    {
        // 读取 cmap 表头
        _binary.BaseStream.Seek(CmapTable!.Offset, SeekOrigin.Begin);
        _binary.ReadUInt16BigEndian(); // version
        var numSubtables = _binary.ReadUInt16BigEndian();

        // 遍历子表目录，寻找最佳子表
        var cmapSubTables = new List<CmapSubTable>();
        for (var i = 0; i < numSubtables; i++)
        {
            var platformID = _binary.ReadUInt16BigEndian();
            var encodingID = _binary.ReadUInt16BigEndian();
            var subtableOffset = _binary.ReadUInt32BigEndian();
            var precedence = (platformID, encodingID) switch
            {
                (3, 1) => 1, // Windows Unicode BMP
                (3, 10) => 2, // Windows Unicode Full
                (0, 3) => 3, // Unicode BMP
                (0, 4) => 4, // Unicode Full
                _ => 100
            };
            cmapSubTables.Add(new CmapSubTable(platformID, encodingID, subtableOffset, precedence));
        }

        foreach (var subTable in cmapSubTables.OrderBy(t => t.Precedence))
        {
            _binary.BaseStream.Seek(CmapTable.Offset + subTable.TableDataOffset, SeekOrigin.Begin);
            var format = _binary.ReadUInt16BigEndian();
            if (format == 4) goto ReadAndPhase;
        }

        throw new NotSupportedException("不支持的字体文件！");

        ReadAndPhase:
        // 读取 Format 4 的表头
        _binary.ReadUInt16BigEndian(); // length
        _binary.ReadUInt16BigEndian(); // language
        var segCountX2 = _binary.ReadUInt16BigEndian();
        var segCount = segCountX2 / 2;

        // 跳过二分查找字段
        _binary.BaseStream.Seek(6, SeekOrigin.Current);

        // 预加载 endCode 数组，这个数组存储了每个字符段的结束 Unicode 码点
        var endCodes = new ushort[segCount];
        for (var i = 0; i < segCount; i++)
            endCodes[i] = _binary.ReadUInt16BigEndian();

        // 读取并丢弃 2 字节的保留字段，用于确保数据对齐
        _binary.ReadUInt16();

        // 预加载 startCode 数组，这个数组存储了每个字符段的开始 Unicode 码点，与 endCode 数组一一对应
        var startCodes = new ushort[segCount];
        for (var i = 0; i < segCount; i++)
            startCodes[i] = _binary.ReadUInt16BigEndian();

        // 预加载 idDelta 数组。这个数组存储了用于计算字形索引的增量值。对于简单映射，字形索引 = (charCode + idDelta) % 65536
        var idDeltas = new short[segCount];
        for (var i = 0; i < segCount; i++)
            idDeltas[i] = (short)_binary.ReadUInt16BigEndian();

        // 预加载 idRangeOffset 数组。当它不为 0 时，表示该段的映射是“复杂的”，字形索引需要从另一个数组（glyphIndexArray）中查找
        // 它的值是一个偏移量，指向该段第一个字符对应字形索引的位置
        var idRangeOffsets = new ushort[segCount];
        for (var i = 0; i < segCount; i++)
            idRangeOffsets[i] = _binary.ReadUInt16BigEndian();

        // 记录 glyphIndexArray 的起始位置。
        var glyphIndexArrayPos = _binary.BaseStream.Position;

        // 统计 CJK 字符
        uint cjkGlyphCount = 0;

        // 遍历 CJK 统一表意文字 0x4E00~0x9FFF 的 Unicode
        for (uint charCode = 0x4E00; charCode <= 0x9FFF; charCode++)
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
                cjkGlyphCount++;
            }
            // 复杂映射，需要从 glyphIndexArray 中动态查找字形索引。
            else
            {
                var offset = glyphIndexArrayPos + 2 * (min - segCount) + idRangeOffsets[min];
                offset += 2 * (charCode - startCodes[min]);

                _binary.BaseStream.Seek(offset, SeekOrigin.Begin);
                var glyphIndex = _binary.ReadUInt16BigEndian();
                if (glyphIndex != 0 && (ushort)(charCode + idDeltas[min]) != 0) cjkGlyphCount++;
            }
        }

        return cjkGlyphCount;
    }
}