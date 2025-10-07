using System.IO;
using System.Windows.Controls;
using FontReader;

namespace Windows_Font_Replacement_Tool.Framework;

/// <summary>
/// 快速制作模式任务代码。
/// </summary>
public class SingleReplace : ReplaceTask
{
    public ReplaceThread MainThread => ReplaceThreads[0];

    /// <summary>
    /// 构造函数，初始化快速制作任务。
    /// </summary>
    /// <param name="customFont">个性化字体文件</param>
    /// <param name="textBlock"></param> todo
    public SingleReplace(Font customFont, TextBlock textBlock)
    {
        // 将用户选择的字体文件名作为任务的名称，然后初始化
        TaskName = Path.GetFileNameWithoutExtension(customFont.FontPath);
        InitCacheDir(CacheDirPath);

        // 为 ReplaceThreads 的 19 个进程填充相同的值
        var index = 0;
        foreach (var sha1 in Sha2File.Keys)
        {
            ReplaceThreads[index] = new ReplaceThread(TaskName, customFont, sha1, textBlock);
            index++;
        }
    }
}