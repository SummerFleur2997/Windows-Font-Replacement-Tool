using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using FontReader.Framework;
using FontReader.Framework.NameTable;

namespace Windows_Font_Replacement_Tool.Framework;

/// <summary>
/// 用于进行文件解压与哈希校验的类。
/// </summary>
internal static class ResourceHelper
{
    /// <list type="table">
    ///   <item><term>[0]</term><description>msyh - Regular</description></item>
    ///   <item><term>[1]</term><description>msyhUI - Regular</description></item>
    ///   <item><term>[2]</term><description>msyh - Light</description></item>
    ///   <item><term>[3]</term><description>msyhUI - Light</description></item>
    ///   <item><term>[4]</term><description>msyh - Bold</description></item>
    ///   <item><term>[5]</term><description>msyhUI - Bold</description></item>
    ///   <item><term>[6]</term><description>SegoeUI - Regular</description></item>
    ///   <item><term>[7]</term><description>SegoeUI - Light</description></item>
    ///   <item><term>[8]</term><description>SegoeUI - SemiLight</description></item>
    ///   <item><term>[9]</term><description>SegoeUI - SemiBold</description></item>
    ///   <item><term>[10]</term><description>SegoeUI - Bold</description></item>
    ///   <item><term>[11]</term><description>SegoeUI - Black</description></item>
    ///   <item><term>[12]</term><description>SegoeUI - Italic</description></item>
    ///   <item><term>[13]</term><description>SegoeUI - Light Italic</description></item>
    ///   <item><term>[14]</term><description>SegoeUI - SemiLight Italic</description></item>
    ///   <item><term>[15]</term><description>SegoeUI - SemiBold Italic</description></item>
    ///   <item><term>[16]</term><description>SegoeUI - Bold Italic</description></item>
    ///   <item><term>[17]</term><description>SegoeUI - Black Italic</description></item>
    ///   <item><term>[18]</term><description>Segoe Variable</description></item>
    /// </list>
    public static readonly List<NameTableData> NameTableData = new();

    static ResourceHelper()
    {
        try
        {
            var uri = new Uri("pack://application:,,,/Resources/.tablemap");
            var stream = Application.GetResourceStream(uri);
            if (stream == null) throw new Exception("未能解析 tablemap 文件");
            var reader = new BinaryReader(stream.Stream);

            for (byte i = 0; i < 19; i++)
            {
                reader.BaseStream.Seek(i * 16, SeekOrigin.Begin);
                if (i != reader.ReadByte())
                    throw new IndexOutOfRangeException("索引超出范围！");
                var l = reader.ReadByte();
                var fontName = Encoding.ASCII.GetString(reader.ReadBytes(l));

                reader.BaseStream.Seek(i * 16 + 12, SeekOrigin.Begin);
                var offset = reader.ReadUInt16BigEndian();
                var length = reader.ReadUInt16BigEndian();

                reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                var data = reader.ReadBytes(length);
                var checkSum = Table.CalculateCheckSum(data.ToList());

                var nameTable = new NameTable(checkSum, offset, length);
                nameTable.ReadRecords(reader);
                nameTable.LoadBytes(data);

                NameTableData.Add(new NameTableData(nameTable, fontName));
            }
        }
        catch (Exception ex)
        {
            Utilities.HandleError("文件校验失败，请尝试重新下载并安装本工具", ex, true);
        }
    }
}