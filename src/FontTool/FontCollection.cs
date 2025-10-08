using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FontTool.Framework;

namespace FontTool;

public class FontCollection : IEnumerable<Font>
{
    /// <summary>
    /// TTC文件版本
    /// </summary>
    public uint Version { get; private set; }

    /// <summary>
    /// 包含的字体数量
    /// </summary>
    private uint FontCount { get; }

    /// <summary>
    /// 解析的字体列表
    /// </summary>
    public List<Font> Fonts { get; }

    /// <summary>
    /// 字体偏移量表
    /// </summary>
    private uint[] Offsets { get; }

    /// <summary>
    /// 从TTC文件创建字体集合
    /// </summary>
    /// <param name="filePath">TTC文件路径</param>
    public FontCollection(string filePath)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        Fonts = new List<Font>();

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var reader = new BinaryReader(stream);

        // 读取TTC文件头
        var header = reader.ReadString(4);
        if (header != "ttcf")
            throw new NotSupportedException("Unsupported font format!");

        Version = reader.ReadUInt32BigEndian();
        FontCount = reader.ReadUInt32BigEndian();

        // 读取字体偏移量表
        Offsets = new uint[FontCount];
        for (var i = 0; i < FontCount; i++)
            Offsets[i] = reader.ReadUInt32BigEndian();

        // 读取每个字体
        foreach (var offset in Offsets)
        {
            stream.Seek(offset, SeekOrigin.Begin);
            var font = new Font("/@stream", stream);
            Fonts.Add(font);
        }
    }

    public bool SaveFonts(string dirPath, string fontName)
    {
        var flag = true;
        foreach (var font in Fonts)
        {
            var name = string.Concat(fontName, Fonts.IndexOf(font), ".ttf");
            var path = Path.Combine(dirPath, name);
            if (!font.Save(path, true)) flag = false;
        }

        return flag;
    }

    /// <summary>
    /// 显式接口实现，用于支持 foreach 循环等需要 IEnumerable 接口的场景
    /// </summary>
    /// <returns>返回枚举器</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// 显式接口实现，用于获取字体枚举器
    /// </summary>
    /// <returns>返回字体集合的枚举器</returns>
    public IEnumerator<Font> GetEnumerator() => Fonts.GetEnumerator();
}