using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FontTool.Framework.NameTable;

/// <summary>
/// A class for storing detailed information about the name table of the font.
/// </summary>
public class NameTable : Table
{
    /// <summary>
    /// The records in the name table.
    /// </summary>
    public readonly List<NameTableRecord> Records = new();

    public NameTable(uint checkSum, uint offset, uint length)
        : base("name", checkSum, offset, length) { }

    /// <summary>
    /// Convert the name table as a human-readable representation.
    /// </summary>
    /// <param name="reader">The binary reader of this font.</param>
    public void ReadRecords(BinaryReader reader)
    {
        reader.BaseStream.Seek(Offset, SeekOrigin.Begin);
        reader.ReadUInt16BigEndian(); // format
        var count = reader.ReadUInt16BigEndian();
        var stringOffset = reader.ReadUInt16BigEndian();

        // Read each record in the name table
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

        // Read the binary data for each record
        foreach (var record in Records)
        {
            // Calculate the offset of the string
            var stringPos = Offset + stringOffset + record.StringDataOffset;
            reader.BaseStream.Seek(stringPos, SeekOrigin.Begin);
            var stringBytes = reader.ReadBytes(record.Length);

            // Decode the binary data into text information
            record.StringData = record.PlatformID switch
            {
                1 => Encoding.GetEncoding("GBK").GetString(stringBytes),
                3 => Encoding.BigEndianUnicode.GetString(stringBytes),
                _ => Encoding.Default.GetString(stringBytes)
            };
        }
    }
}