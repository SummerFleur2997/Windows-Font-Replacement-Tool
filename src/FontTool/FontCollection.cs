using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FontTool.Framework;
using JetBrains.Annotations;

namespace FontTool;

/// <summary>
/// A library for reading ttc font collections files and collecting their
/// basic information (especially name table information). Can be used for
/// merge/split files.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class FontCollection : IEnumerable<Font>, IDisposable
{
    /// <summary>
    /// Path to the font file.
    /// </summary>
    public readonly string FontPath;

    /// <summary>
    /// Reader for reading binary data.
    /// </summary>
    public BinaryReader Reader
    {
        get
        {
            if (FontPath.StartsWith("/@") || _reader is null)
                throw new FormatException("This instance is not a file. Please use the stream constructor instead.");
            return _reader;
        }
        set => _reader = value;
    }

    /// <inheritdoc cref="Reader"/>
    private BinaryReader? _reader;

    /// <summary>
    /// Font signature, 4 bytes long.
    /// </summary>
    public const uint Sfnt = 0x74746366;

    /// <summary>
    /// The version of the TTC file.
    /// </summary>
    public uint Version { get; private set; }

    /// <summary>
    /// The number of fonts in the collection.
    /// </summary>
    public uint FontCount { get; private set; }

    /// <summary>
    /// An array that contains the offsets of the fonts in the collection.
    /// </summary>
    public uint[] Offsets { get; }

    /// <summary>
    /// List of fonts in the collection.
    /// </summary>
    public List<Font> Fonts { get; }

    /// <summary>
    /// Creates a font collection from a TTC file.
    /// </summary>
    /// <param name="fontPath">The path to the font file. If using a stream, please use "/@stream". </param>
    /// <param name="stream">The stream of the font file. </param>
    /// <exception cref="FileNotFoundException">Cannot find the font file. </exception>
    /// <exception cref="NotSupportedException">Unsupported font format. </exception>
    /// <exception cref="FileLoadException">Corrupted font file. </exception>
    public FontCollection(string fontPath, FileStream? stream = null)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // Check if the file exists
        if (fontPath != "/@stream" && !File.Exists(fontPath))
            throw new FileNotFoundException("File does not exist.", fontPath);
        FontPath = fontPath;

        // Initialize binary stream
        stream ??= new FileStream(fontPath, FileMode.Open, FileAccess.Read);
        _reader = new BinaryReader(stream);

        Fonts = new List<Font>();

        // Read the header of the TTC file
        var header = Reader.ReadUInt32BigEndian();
        if (header != Sfnt)
            throw new NotSupportedException("Unsupported font format!");

        Version = Reader.ReadUInt32BigEndian();
        FontCount = Reader.ReadUInt32BigEndian();

        // Add the offset of every font to the array
        Offsets = new uint[FontCount];
        for (var i = 0; i < FontCount; i++)
            Offsets[i] = Reader.ReadUInt32BigEndian();

        // Read the font data from the stream
        foreach (var offset in Offsets)
        {
            stream.Seek(offset, SeekOrigin.Begin);
            var font = new Font("/@stream", stream);
            Fonts.Add(font);
        }
    }

    /// <summary>
    /// Creates a font collection from a list of fonts. And set the
    /// <see cref="Version"/> field to 0x00010000 by default.
    /// </summary>
    /// <param name="fonts">List of fonts to be added to the collection. </param>
    public FontCollection(IEnumerable<Font> fonts)
    {
        FontPath = "/@font_collection";
        Version = 0x00010000;
        Fonts = fonts.ToList();
        FontCount = (uint)Fonts.Count;
        Offsets = new uint[FontCount];

        var offset = FontCount * 4 + 12;
        for (var i = 0; i < FontCount; i++)
        {
            Offsets[i] = offset;
            offset += (uint)Fonts[i].FontTables.Count * 16 + 12;
        }
    }

    /// <summary>
    /// Get the body offset to the head of the file.
    /// </summary>
    public uint BodyOffset() => (uint)(12 + FontCount * 4 + Fonts.Sum(font => font.BodyOffset()));

    /// <summary>
    /// Save the collection binaries to the specified path.
    /// </summary>
    /// <param name="path">The full path to save the collection file. </param>
    /// <returns>True if the collection is saved successfully; otherwise, false. </returns>
    public bool Save(string path)
    {
        try
        {
            var binary = new List<byte>();
            var body = new List<byte>();
            var bodyOffset = BodyOffset();
            var tableData = new Dictionary<string, uint>();

            foreach (var font in Fonts)
            {
                font.LoadTableData();
                var tables = new List<Table>();
                foreach (var table in font.FontTables)
                {
                    // Check if the table is duplicated
                    var hashCode = table.GetHashCode();
                    if (tableData.TryGetValue(hashCode, out var existOffset))
                    {
                        // Set the offset of the table
                        table.Offset = existOffset;
                        tables.Add(table);
                    }
                    else
                    {
                        // Create a new record for this table
                        var offset = (uint)body.Count;
                        tableData.Add(hashCode, offset);
                        body.AddRange(table.Bytes);

                        // Set the offset of the table
                        table.Offset = offset;
                        tables.Add(table);

                        // Align the offset to 4 bytes
                        var padding = (4 - table.Length % 4) % 4;
                        for (var i = 0; i < padding; i++) body.Add(0);
                    }
                }

                font.ReplaceTable(tables);
                font.UpdateTableOffset(bodyOffset, Font.OpMode.Shift);
            }

            binary.AddRange(Sfnt.ToBigEndianBytes());
            binary.AddRange(Version.ToBigEndianBytes());
            binary.AddRange(FontCount.ToBigEndianBytes());
            foreach (var offset in Offsets)
                binary.AddRange(offset.ToBigEndianBytes());

            foreach (var font in Fonts)
            {
                // Add sfnt header, font table count, and font header in big-endian byte representation
                binary.AddRange(font.Sfnt.ToBigEndianBytes());
                binary.AddRange(((ushort)font.FontTables.Count).ToBigEndianBytes());
                binary.AddRange(font.Header);

                // Add font table directory in big-endian byte representation
                foreach (var table in font.FontTables)
                {
                    binary.AddRange(table.Tag.ToBigEndianBytes());
                    binary.AddRange(table.CheckSum.ToBigEndianBytes());
                    binary.AddRange(table.Offset.ToBigEndianBytes());
                    binary.AddRange(table.Length.ToBigEndianBytes());
                }
            }

            // Add the body of the font, and then save the binary data to the specified path
            binary.AddRange(body);
            File.WriteAllBytes(path, binary.ToArray());
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Save all the fonts in this collection to individual files.
    /// </summary>
    /// <param name="dirPath">The directory path to save the font files. </param>
    /// <param name="fontName">The base name of the font files. </param>
    /// <returns>True if the collection is saved successfully; otherwise, false. </returns>
    public bool SaveFonts(string dirPath, string fontName)
    {
        var flag = true;
        foreach (var font in Fonts)
        {
            // 设置文件名
            var name = string.Concat(fontName, Fonts.IndexOf(font), ".ttf");
            var path = Path.Combine(dirPath, name);

            // 计算偏移量并更新表信息
            var offset = font.BodyOffset();
            font.LoadTableData();
            font.UpdateTableOffset(offset);

            if (!font.Save(path)) flag = false;
        }

        return flag;
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    public IEnumerator<Font> GetEnumerator() => Fonts.GetEnumerator();

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var font in Fonts)
            font.Dispose();

        if (!FontPath.StartsWith("/@") && _reader != null)
        {
            Reader.Close();
            Reader.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}