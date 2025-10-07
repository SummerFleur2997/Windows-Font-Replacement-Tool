using System.Buffers.Binary;
using System.IO;

namespace FontReader.Framework;

public static class Utilities
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
}