using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FontTool.Framework;
using FontTool.Framework.CmapTable;
using FontTool.Framework.NameTable;
using JetBrains.Annotations;

namespace FontTool;

/// <summary>
/// A library for reading true type and open type font files and collecting their
/// basic information (especially name table information).
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class Font : IDisposable
{
    /// <summary>
    /// Path to the font file.
    /// </summary>
    public readonly string FontPath;

    /// <summary>
    /// Reader for reading binary data.
    /// </summary>
    public BinaryReader Reader { get; }

    /// <summary>
    /// Font signature, 4 bytes long.
    /// </summary>
    public uint Sfnt { get; }

    /// <summary>
    /// Font header data, 6 bytes long.
    /// </summary>
    public byte[] Header { get; }

    /// <summary>
    /// The name table, which contains font name information.
    /// </summary>
    public NameTable? NameTable => FontTables.First(t => t.Tag == "name") as NameTable;

    /// <summary>
    /// The cmap table, which contains font encoding information.
    /// </summary>
    public CmapTable? CmapTable => FontTables.First(t => t.Tag == "cmap") as CmapTable;

    /// <summary>
    /// A list used to store the basic information of each table in the font file.
    /// </summary>
    public List<Table> FontTables { get; private set; }

    /// <summary>
    /// Reads the font file and parses its key information.
    /// </summary>
    /// <param name="fontPath">The path to the font file. If using a stream, please use "/@stream". </param>
    /// <param name="stream">The stream of the font file. </param>
    /// <exception cref="FileNotFoundException">Cannot find the font file. </exception>
    /// <exception cref="NotSupportedException">Unsupported font format. </exception>
    /// <exception cref="FileLoadException">Corrupted font file. </exception>
    public Font(string fontPath, FileStream? stream = null)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // Check if the file exists
        if (fontPath != "/@stream" && !File.Exists(fontPath))
            throw new FileNotFoundException("File does not exist.", fontPath);
        FontPath = fontPath;

        // Initialize binary stream
        stream ??= new FileStream(fontPath, FileMode.Open, FileAccess.Read);
        Reader = new BinaryReader(stream);

        // Read table directory information
        Sfnt = Reader.ReadUInt32BigEndian();
        if (Sfnt is not (0x00010000 or 0x4f54544f))
            throw new NotSupportedException("Unsupported font format!");

        // Prepare to read the font
        var numTables = Reader.ReadUInt16BigEndian();
        Header = Reader.ReadBytes(6);

        // Initialize all table information
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

        // Check if the key tables exist
        if (NameTable is null || CmapTable is null)
            throw new FileLoadException("The font file is corrupted!");

        // Write raw binary data to NameTable
        LoadTableData("name");

        // Parse the header of the name table
        NameTable.ReadRecords(Reader);
    }

    /// <summary>
    /// Get the font family name, with an option to include the subfamily name.
    /// </summary>
    /// <param name="showSubfamily">Whether to return the subfamily. By default, it is false. </param>
    /// <returns>
    /// The font family name string, if showSubfamily is true, returns the format "familyName-subfamilyName";
    /// if the NameTable is null, returns "-"
    /// </returns>
    public string FontFamily(bool showSubfamily = false)
    {
        // A check to make IDE happy
        if (NameTable == null) return "-";

        var family = NameTable.Records
            .FirstOrDefault(t => t is { NameID: 1, LanguageID: 0x409 })?
            .StringData ?? "";

        var subfamily = NameTable.Records
            .FirstOrDefault(t => t is { NameID: 2, LanguageID: 0x409 })?
            .StringData ?? "";

        return showSubfamily
            ? string.Concat(family, "-", subfamily)
            : family;
    }

    /// <summary>
    /// Get the body offset to the head of the file.
    /// </summary>
    public uint BodyOffset() => (uint)(12 + FontTables.Count * 16);

    /// <summary>
    /// Write the binary data to <see cref="Table.Bytes"/>.
    /// </summary>
    /// <param name="tableNames">
    /// The names of the tables to be written, if null, load all tables by default.
    /// </param>
    public void LoadTableData(IEnumerable<string>? tableNames = null)
    {
        var tables = tableNames == null
            ? FontTables
            : FontTables.Where(t => tableNames.Contains(t.Tag));

        foreach (var table in tables)
            table.LoadBytes(Reader);
    }

    /// <summary>
    /// Write the binary data to <see cref="Table.Bytes"/>.
    /// </summary>
    /// <param name="tableName">The name of the tables to be written. </param>
    public void LoadTableData(string tableName)
    {
        var table = FontTables.First(t => t.Tag == tableName);
        table.LoadBytes(Reader);
    }

    /// <summary>
    /// Get the number of valid characters in the specified Unicode range.
    /// </summary>
    /// <param name="start">The start index of the Unicode range. </param>
    /// <param name="end">The end index of the Unicode range. </param>
    /// <returns>The number of valid characters in the specified Unicode range. </returns>
    /// <exception cref="NotSupportedException">Unsupported font format.</exception>
    public uint GetCharacterCountFromTo(uint start, uint end)
    {
        // 不会触发
        if (CmapTable == null) return 0;

        // 读取 cmap 表头
        Reader.BaseStream.Seek(CmapTable.Offset, SeekOrigin.Begin);
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
            if (format == 4) goto ReadAndParse;
        }

        // 如果没有找到格式4的子表，抛出不支持异常
        throw new NotSupportedException("Unsupported font format!");

        ReadAndParse:
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

    /// <summary>
    /// Replace the specified table in the font table. When finished, you should call
    /// <see cref="UpdateTableOffset"/> manually to update the offsets of all tables in the font.
    /// </summary>
    /// <param name="newTable">The instance of the new table object to replace the existing table. </param>
    public void ReplaceTable(Table newTable)
    {
        // Find the target table
        var targetTable = FontTables.First(t => t.Tag == newTable.Tag);

        // Update the table's data
        newTable.Offset = targetTable.Offset;
        FontTables[FontTables.IndexOf(targetTable)] = newTable;
    }

    /// <summary>
    /// Use a new list of tables to replace all the tables. When finished, you should call
    /// <see cref="UpdateTableOffset"/> manually to update the offsets of all tables in the font.
    /// </summary>
    /// <param name="newTables">A table list to replace the existing tables. </param>
    public void ReplaceTable(List<Table> newTables) => FontTables = newTables;

    /// <summary>
    /// Save the font binaries to the specified path.
    /// </summary>
    /// <param name="path">The full path to save the font file. </param>
    /// <returns>True if the font is saved successfully; otherwise, false. </returns>
    public bool Save(string path)
    {
        try
        {
            var binary = new List<byte>();
            var body = new List<byte>();
            var tables = FontTables.OrderBy(t => t.Offset).ToList();
            foreach (var table in tables)
            {
                // Add the binary data of the table
                body.AddRange(table.Bytes);
                // Align the table data to 4-byte boundary
                var padding = (4 - table.Length % 4) % 4;
                for (var i = 0; i < padding; i++) body.Add(0);
            }

            // Add sfnt header, font table count, and font header in big-endian byte representation
            binary.AddRange(Sfnt.ToBigEndianBytes());
            binary.AddRange(((ushort)FontTables.Count).ToBigEndianBytes());
            binary.AddRange(Header);

            // Add font table directory in big-endian byte representation
            foreach (var table in FontTables)
            {
                binary.AddRange(table.Tag.ToBigEndianBytes());
                binary.AddRange(table.CheckSum.ToBigEndianBytes());
                binary.AddRange(table.Offset.ToBigEndianBytes());
                binary.AddRange(table.Length.ToBigEndianBytes());
            }

            // Add the body of the font, and then save the binary data to the specified path
            binary.AddRange(body);
            File.WriteAllBytes(path, binary.ToArray());
            return true;
        }
        catch (Exception)
        {
            // If an exception occurs, return false
            return false;
        }
    }

    /// <summary>
    /// Update the offsets of all tables in the font.
    /// </summary>
    /// <param name="bodyOffset">The body offset of the font.
    /// If null, the offset of the first table will be used. </param>
    /// <param name="opMode">The method to update the offset. </param>
    /// <seealso cref="OpMode"/>
    public void UpdateTableOffset(uint? bodyOffset = null, OpMode opMode = OpMode.Reorganize)
    {
        switch (opMode)
        {
            case OpMode.Shift:
                if (bodyOffset is null)
                    throw new ArgumentException("The body offset must be specified when using the Shift mode.");
                foreach (var table in FontTables)
                    table.Offset += (uint)bodyOffset;
                return;
            case OpMode.Reorganize:
            default:
                var tables = FontTables.OrderBy(t => t.Offset).ToList();
                var offset = bodyOffset ?? tables.First().Offset;
                foreach (var table in tables)
                {
                    FontTables.First(t => t.Tag == table.Tag).UpdateValue(offset: offset);
                    offset += table.Length + (4 - table.Length % 4) % 4;
                }

                return;
        }
    }

    public override string ToString() => $"{FontFamily(true)} at {FontPath}";

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var table in FontTables)
            table.Dispose();

        Reader.Close();
        Reader.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Signify the operation method when update the offset of the table.
    /// </summary>
    /// <remarks>
    /// <para><b>Reorganize</b>: Indicate the start offset of the first table, 
    /// then calculate the next offset based on the length of the current table.</para>
    /// <para><b>Shift</b>: Add the specified offset to the current offset.</para>
    /// </remarks>
    public enum OpMode
    {
        Reorganize,
        Shift
    }
}