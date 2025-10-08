using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace FontReader.Framework;

internal static class Utilities
{
    /// <summary>
    /// 使用大端序列读取一个 16 位短整型无符号数。
    /// </summary>
    public static ushort ReadUInt16BigEndian(this BinaryReader br)
    {
        var bytes = br.ReadBytes(2);
        return BinaryPrimitives.ReadUInt16BigEndian(bytes);
    }

    /// <summary>
    /// 使用大端序列读取一个 32 位整型无符号数。
    /// </summary>
    public static uint ReadUInt32BigEndian(this BinaryReader br)
    {
        var bytes = br.ReadBytes(4);
        return BinaryPrimitives.ReadUInt32BigEndian(bytes);
    }

    /// <summary>
    /// 从二进制读取器中读取指定数量的字符并转换为字符串
    /// </summary>
    public static string ReadString(this BinaryReader br, int count)
    {
        // 使用ReadChars方法读取指定数量的字符
        var chars = br.ReadChars(count);
        // 将字符数组转换为字符串并返回
        return new string(chars);
    }

    public static byte[] ToBigEndianBytes(this uint value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return bytes;
    }

    public static byte[] ToBigEndianBytes(this ushort value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return bytes;
    }

    public static byte[] ToBigEndianBytes(this string value)
    {
        // 将字符串转换为字节数组
        var bytes = Encoding.UTF8.GetBytes(value);
        return bytes;
    }
}