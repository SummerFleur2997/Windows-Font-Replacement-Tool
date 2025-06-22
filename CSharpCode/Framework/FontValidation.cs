using System;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace Windows_Font_Replacement_Tool.Framework;

/// <summary>
/// 用于进行各类验证的静态类。
/// </summary>
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
            
            // 获取字体文件签名
            var headerBytes = new byte[4];
            var bytesRead = stream.Read(headerBytes, 0, 4);
            if (bytesRead < 4) return false;
            var header = Encoding.ASCII.GetString(headerBytes);

            // 检查字体文件签名
            return header switch
            {
                // 对应 otf 字体集
                "OTTO" => true,
                // 对应 ttf 字体集
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
            // 配置进程启动选项
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
            
            // 启动进程并接收输出
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            
            // 等待进程结束并处理结束值
            process.WaitForExit();
            return process.ExitCode == 0
                // 进程正常结束时，返回字符集数量
                ? int.Parse(output.Trim())
                // 未能正常结束时，返回 -1
                : -1;
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
            // 配置进程启动选项
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
            
            // 启动进程并接收输出
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            
            // 等待进程结束并处理结束值
            process.WaitForExit();
            return process.ExitCode == 0
                // 进程正常结束时，返回字体 FontFamily
                ? output.Trim()
                // 未能正常结束时，返回 **Error**
                : "**Error**";
        }
        catch
        {
            return "**Error**";
        }
    }
}