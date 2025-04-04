using System;
using System.IO;
using System.Text;
using System.Windows.Media;

namespace Windows_Font_Replacement_Tool.Framework;

public static class Validation
{
    public static bool IsValidFontFile(string fontPath)
    {
        try
        {
            using var stream = new FileStream(fontPath, FileMode.Open, FileAccess.Read);
            
            byte[] headerBytes = new byte[4];
            int bytesRead = stream.Read(headerBytes, 0, 4);

            if (bytesRead < 4)
                return false;

            // 转换为大端序字符串（适用于常见字体签名）
            if (BitConverter.IsLittleEndian)
                Array.Reverse(headerBytes);
            string header = Encoding.ASCII.GetString(headerBytes);

            // 检查常见字体文件签名
            return header switch
            {
                "ttcf" => true,
                "OTTO" => true,
                _ => BitConverter.ToUInt32(headerBytes, 0) == 0x00010000
            };
            
        }
        catch
        {
            return false;
        }
    }
    
    public static int GetCjkCharacterCount(string fontPath)
    {
        try
        {
            var uri = new Uri($"file:///{fontPath}");
            var glyphTypeface = new GlyphTypeface(uri);
            const int start = 0x4E00;
            const int end = 0x9FFF;
            int count = 0;

            for (int code = start; code <= end; code++)
            {
                if (glyphTypeface.CharacterToGlyphMap.ContainsKey((char)code))
                    count++;
            }
            return count;
        }
        catch
        {
            return -1;
        }
    }
}