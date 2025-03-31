using System;
using System.IO;
using System.Threading.Tasks;

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

    /// <summary>
    /// 多线程运行 Python程序进行属性替换。
    /// </summary>
    public async Task SingleStartPropRep()
    {
        OutputDirPath = CreateOutputDir(TaskName);
        CreateCacheDir();
        await Task.Run(() => 
        {
            Parallel.ForEach(
                ReplaceThreads,
                new ParallelOptions { MaxDegreeOfParallelism = MaxDegree },
                task => task.RunPropertyRep()
            );
        });
        foreach (var sha1 in Sha2File.Keys)
        {
            var originalFilePath = Path.Combine(CacheDirPath, sha1);
            var newFilePath = Path.Combine(CacheDirPath, Sha2File[sha1]);
            File.Move(originalFilePath, newFilePath);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public async Task SingleMerge()
    {
        string[] filePrefixes = { "msyh", "msyhl", "msyhbd" };
        await Task.Run(() =>
        {
            Parallel.ForEach(
                filePrefixes,
                new ParallelOptions { MaxDegreeOfParallelism = MaxDegree },
                filePrefix => RunMerge(filePrefix)
            );
        });
    }

    public void SingleFinishing()
    {
        foreach (var sha1 in Sha2File.Keys)
        {
            var seg = Sha2File[sha1].Substring(0, 3);
            switch (seg)
            {
                case "Seg" or "seg":
                    var originalFilePath = Path.Combine(CacheDirPath, Sha2File[sha1]);
                    var newFilePath = OutputFilePath(Sha2File[sha1]);
                    File.Move(originalFilePath, newFilePath);
                    break;
            }
        }
        InitCacheDir(CacheDirPath);
    }
}