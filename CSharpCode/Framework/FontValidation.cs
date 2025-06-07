using System;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace Windows_Font_Replacement_Tool.Framework;

public static class FontValidation
{
    /// <summary>
    /// 判断字体文件是否为合法的字体文件（.ttf 或 .otf）。
    /// </summary>
    /// <param name="fontPath">字体文件绝对路径</param>
    /// <returns></returns>
    public static bool IsValidFontFile(string fontPath)
    {
        try
        {
            using var stream = new FileStream(fontPath, FileMode.Open, FileAccess.Read);
            
            var headerBytes = new byte[4];
            var bytesRead = stream.Read(headerBytes, 0, 4);

            if (bytesRead < 4)
                return false;
            
            var header = Encoding.ASCII.GetString(headerBytes);

            // 检查字体文件签名
            return header switch
            {
                "OTTO" => true,
                _ => BitConverter.ToUInt32(headerBytes, 0) == 0x00000100
            };
            
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// 获取 U+4E00 ~ U+9FFF 内的字符数量。
    /// </summary>
    /// <param name="fontPath">字体文件绝对路径</param>
    /// <returns>CJK Unified Ideographs内的字符数量</returns>
    public static int GetCjkCharacterCount(string fontPath)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(HashTab.ResourcePath, "functions.exe"),
                Arguments = $"getCjk \"{fontPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };
            using var process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            var exitCode = process.ExitCode switch
            {
                0 => int.Parse(output.Trim()),
                _ => -1
            };
            return exitCode;
        }
        catch
        {
            return -1;
        }
    }

    public static string GetFontFamily(string fontPath)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(HashTab.ResourcePath, "functions.exe"),
                Arguments = $"fontFamily \"{fontPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };
            using var process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(); 
            
            switch (process.ExitCode)
            {
                case 0:
                    return output.Trim(); 
                default:
                    return "**Error**";
            }
        }
        catch
        {
            return "**Error**";
        }
    }
}