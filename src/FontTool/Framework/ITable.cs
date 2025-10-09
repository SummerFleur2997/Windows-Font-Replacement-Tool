using System.Collections.Generic;
using System.IO;

namespace FontTool.Framework;

public interface ITable
{
    /// <summary>
    /// The tag of this font table.
    /// </summary>
    public string Tag { get; }

    /// <summary>
    /// The checksum of this font table.
    /// </summary>
    public uint CheckSum { get; set; }

    /// <summary>
    /// The offset of the table header from the beginning of the file.
    /// </summary>
    public uint Offset { get; set; }

    /// <summary>
    /// The length of this font table in bytes.
    /// </summary>
    public uint Length { get; set; }

    /// <summary>
    /// The raw binary data of this font table.
    /// </summary>
    public List<byte> Bytes { get; }

    /// <summary>
    /// Load binary data to this table via a file stream.
    /// </summary>
    public void LoadBytes(BinaryReader reader);

    /// <summary>
    /// Load binary data to this table via a byte array.
    /// </summary>
    public void LoadBytes(byte[] bytes);

    /// <summary>
    /// Update the checksum, offset or length of this table.
    /// </summary>
    public void UpdateValue(uint? checkSum = null, uint? offset = null, uint? length = null);

    /// <summary>
    /// Calculate the checksum of this table.
    /// </summary>
    public uint CalculateCheckSum();
}