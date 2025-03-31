using System;
using System.IO;
using System.Diagnostics;

namespace Windows_Font_Replacement_Tool.Framework;

/// <summary>
/// 单个字体文件的替换进程，包含 Python程序的启动函数。
/// </summary>
public class ReplaceThread
{
    /// <summary>
    /// 进程名称信息，应为字体的实际文件名。
    /// </summary>
    private string ThreadName { get; set; }
    
    /// <summary>
    /// 个性化字体资源的绝对路径。
    /// </summary>
    private string FontResource { get; }
    
    /// <summary>
    /// xml字体资源的绝对路径。
    /// </summary>
    private string XmlResource { get; }

    /// <summary>
    /// Python程序，替换字体属性。
    /// </summary>
    /// <returns>Python程序退出代码</returns>
    public int RunPropertyRep()
    {
        try
        {
            // 设定进程运行参数
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(HashTab.ResourcePath, "functions.exe"),
                Arguments = $"propertyRep \"{FontResource}\" \"{XmlResource}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            // 运行进程并返回进程退出值
            using var process = new Process { StartInfo = startInfo };
            process.Start();
            process.WaitForExit(); 
            return process.ExitCode;
        }
        // 报错，需完善错误代码 TODO
        catch (Exception ex)
        {
            Console.WriteLine($"执行失败: {ex.Message}");
            return -1;
        }
    }

    /// <summary>
    /// 构造函数，初始化进程。
    /// </summary>
    /// <param name="threadName"></param>
    /// <param name="fontResource">个性化字体文件绝对路径</param>
    /// <param name="sha1">xml字体文件名</param>
    public ReplaceThread(string threadName, string fontResource, string sha1)
    {
        ThreadName = threadName;
        FontResource = fontResource;
        XmlResource = Path.Combine(HashTab.XmlsPath, sha1);
    }
}