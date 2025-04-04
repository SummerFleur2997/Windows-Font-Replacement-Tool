using System;
using System.IO;


namespace Windows_Font_Replacement_Tool.Framework;

/// <summary>
/// 快速制作模式任务代码。
/// </summary>
public class SingleReplace: ReplaceTask
{
    /// <summary>
    /// 构造函数，初始化快速制作任务。
    /// </summary>
    /// <param name="customFont">由 </param>
    public SingleReplace(string customFont)
    {
        TaskName = Path.GetFileNameWithoutExtension(customFont);
        InitCacheDir(CacheDirPath);
        var index = 0;
        foreach (var sha1 in Sha2File.Keys)
        {
            ReplaceThreads[index] = new ReplaceThread(TaskName, customFont, sha1);
            index++;
        }
    }
}