using System.IO;
using System.Diagnostics;
using System.Windows.Controls;

namespace Windows_Font_Replacement_Tool.Framework;

/// <summary>
/// 一种字形的替换进程，包含 Python 程序的启动函数。
/// </summary>
public class ReplaceThread
{
    /// <summary>
    /// 进程名称信息，应为字体的实际文件名。
    /// </summary>
    public string ThreadName { get; }
    
    /// <summary>
    /// 个性化字体资源的绝对路径。
    /// </summary>
    public string FontResource { get; }
    
    /// <summary>
    /// xml 字体资源的绝对路径。
    /// </summary>
    private string XmlResource { get; }
    
    /// <summary>
    /// 提示标签，用于反馈字体检验的合法性。
    /// </summary>
    public TextBlock HintSign { get; }

    /// <summary>
    /// Python 程序，替换字体属性。
    /// </summary>
    /// <returns>Python程序退出代码</returns>
    public int RunPropertyRep()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(HashTab.ResourcePath, "functions.exe"),
                Arguments = $"propertyRep \"{FontResource}\" \"{XmlResource}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using var process = new Process();
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit(); 
            return process.ExitCode;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// 初始化一种字形的替换进程。
    /// </summary>
    /// <param name="threadName">去除拓展名后的字体文件名</param>
    /// <param name="fontResource">个性化字体文件绝对路径</param>
    /// <param name="sha1">xml 字体文件名</param>
    /// <param name="hintSign">精细制作模式下的提示标志</param>
    public ReplaceThread(string threadName, string fontResource, string sha1, TextBlock hintSign)
    {
        ThreadName = threadName;
        FontResource = fontResource;
        XmlResource = Path.Combine(HashTab.XmlsPath, sha1);
        // if (hintSign == null) return;
        HintSign = hintSign;
    }
}