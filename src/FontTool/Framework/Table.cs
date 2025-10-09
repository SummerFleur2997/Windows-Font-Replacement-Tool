using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace FontTool.Framework;

/// <summary>
/// A class for storing basic information about a font table.
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

    /// <inheritdoc/>
    public void LoadBytes(BinaryReader reader)
    {
        if (Bytes.Count > 0) return;
        reader.BaseStream.Seek(Offset, SeekOrigin.Begin);
        Bytes = reader.ReadBytes((int)Length).ToList();
    }

    /// <inheritdoc/>
    public void LoadBytes(byte[] bytes) => Bytes = bytes.ToList();

    /// <inheritdoc/>
    public void UpdateValue(uint? checkSum = null, uint? offset = null, uint? length = null)
    {
        CheckSum = checkSum ?? CheckSum;
        Offset = offset ?? Offset;
        Length = length ?? Length;
    }

    /// <inheritdoc/>
    public uint CalculateCheckSum() => CalculateCheckSum(Bytes);

    /// <summary>
    /// A static method to calculate the checksum of given bytes.
    /// </summary>
    /// <param name="bytes">The array of bytes to calculate the checksum for. </param>
    /// <returns>The calculated checksum as a 32-bit unsigned integer. </returns>
    public static uint CalculateCheckSum(List<byte> bytes)
    {
        // Initialize the checksum to 0, then iterate over the bytes in chunks of 4 bytes.
        uint sum = 0;
        var chunks = (bytes.Count + 3) / 4;

        for (var i = 0; i < chunks; i++)
        {
            var offset = i * 4;
            uint value = 0;

            // For each chunk, read 4 bytes and combine them into a 32-bit integer.
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

    public new string GetHashCode()
    {
        var sha1 = SHA1.Create();
        var hashBytes = sha1.ComputeHash(Bytes.ToArray());
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    public override string ToString() => $"{Tag}: {Offset}+{Length} ({CheckSum})";

    /// <inheritdoc/>
    public void Dispose()
    {
        Bytes.Clear();
        GC.SuppressFinalize(this);
    }
}