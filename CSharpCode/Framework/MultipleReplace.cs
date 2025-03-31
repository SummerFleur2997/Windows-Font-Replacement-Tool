using System;
using System.IO;
using System.Threading.Tasks;

namespace Windows_Font_Replacement_Tool.Framework;

/// <summary>
/// 精细制作模式任务代码。
/// </summary>
public class MultipleReplace : ReplaceTask
{
    /// <summary>
    /// 构造函数，初始化精细制作任务。
    /// </summary>
    public MultipleReplace()
    {
        // TaskName = Path.GetFileNameWithoutExtension(customFont);
        InitCacheDir(CacheDirPath);
    }

    public void InitReplaceThread(int index, string customFont)
    {
        var customFontName = Path.GetFileNameWithoutExtension(customFont);
        ReplaceThreads[index] = new ReplaceThread(customFontName, customFont, "sha1");
    }
}