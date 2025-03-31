using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace Windows_Font_Replacement_Tool.Framework;

/// <summary>
/// 类 SingleReplace与 MultipleReplace的基类，仅用于存储类属性与类函数，不应当被实例化。
/// </summary>
public abstract class ReplaceTask
{
    /// <summary>
    /// 任务的名称，用于命名输出文件夹。
    /// </summary>
    protected string TaskName { get; init; } = "NULL";

    /// <summary>
    /// 任务的输出文件夹绝对路径。
    /// </summary>
    protected string OutputDirPath { get; set; } = "NULL";
    
    /// <summary>
    /// 任务的缓存文件夹绝对路径。
    /// </summary>
    protected string CacheDirPath  { get; } = CreateCacheDir(false);
    
    /// <summary>
    /// 用于存储 19种字形的处理进程。
    /// </summary>
    protected ReplaceThread[] ReplaceThreads = new ReplaceThread[19];
    
    /// <summary>
    /// 获取当前计算机线程数。
    /// </summary>
    protected readonly int MaxDegree = Environment.ProcessorCount;
    
    /// <summary>
    /// 给定任务名称，新建导出文件夹。
    /// </summary>
    /// <param name="taskName">任务名称</param>
    /// <returns>导出文件夹绝对路径</returns>
    protected static string CreateOutputDir(string taskName)
    {
        var outputDir = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + taskName;
        var outputDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output", outputDir);
        Directory.CreateDirectory(outputDirPath);
        return outputDirPath;
    }
    
    /// <summary>
    /// 新建 cache文件夹。
    /// </summary>
    /// <param name="create">是否创建文件夹</param>
    /// <returns>cache文件夹绝对路径</returns>
    protected static string CreateCacheDir(bool create=true)
    {
        var cacheDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output", "cache");
        if (create) Directory.CreateDirectory(cacheDirPath);
        return cacheDirPath;
    }
    
    /// <summary>
    /// 初始化 cache文件夹。
    /// </summary>
    protected static void InitCacheDir(string cacheDirPath)
    {
        if (!Directory.Exists(cacheDirPath)) return;
        Directory.Delete(cacheDirPath, true);
    }
    
    /// <summary>
    /// 给定文件名，返回该文件最终的存储路径。
    /// </summary>
    /// <param name="fileName">给定的文件名</param>
    /// <returns></returns>
    protected string OutputFilePath(string fileName)
    {
        return Path.Combine(OutputDirPath, fileName);
    }
    
    /// <summary>
    /// Python程序，合并两个 ttf为 ttc。
    /// </summary>
    /// <returns>Python程序退出代码</returns>
    protected int RunMerge(string filePrefix)
    {
        var outputDirName = Path.GetFileName(OutputDirPath);
        try
        {
            // 设定进程运行参数
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(HashTab.ResourcePath, "functions.exe"),
                Arguments = $"mergeTTC \"{outputDirName}\" \"{filePrefix}\"",
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
    /// 存储 xmls文件夹内每一个哈希值对应的文件名。
    /// </summary>
    protected static readonly Dictionary<string, string> Sha2File = new()
    {
        {"d46344fc3841184ac741685d53f0b01cd11865e7", "msyh01.ttf"},
        {"791e18622ff1011b9a6c68bfb8796258a3a1cf85", "msyh02.ttf"},
        {"7d70dd165648425f19346947a268a8ee58262eb2", "msyhl01.ttf"},
        {"83eb72dd5f5285488ed2ca4d72810e266bc2fd4e", "msyhl02.ttf"},
        {"ea87b3ddadc073d04b25039f9e41bfbefd8b0eba", "msyhbd01.ttf"},
        {"e49a970410fe1b22ac10bdc7c656117c74f22b39", "msyhbd02.ttf"},
        
        {"5213a5c8e131255d461ebc4e8b5ed71eaedf01dc", "segoeui.ttf"},    // Regular
        {"12144c399e57c776e7b3ed8f689cce09df5c1a53", "segoeuil.ttf"},   // Light
        {"dde72eaa7ba41cac65e21350b613b95f0c04bd4d", "segoeuisl.ttf"},  // SemiLight
        {"f80b4059b145408bc7948ff57aa2d326a08afa31", "seguisb.ttf"},    // SemiBold
        {"c6adb891613704c322580732b92b6566a7e80684", "segoeuib.ttf"},   // Bold
        {"b6379d63dd25e0c01c09dec61e56b4e2a4fc2456", "seguibl.ttf"},    // Black
        
        {"f8808c6fcf9ce4a74a6682fa5d886fe25a60f11b", "segoeuii.ttf"},   // Italic
        {"b6ba87aa742964e7103ff654516a017ddf6031e0", "seguili.ttf"},    // Light Italic
        {"cd0bed1303626bf6cfc7412206744c5bd794be1b", "seguisli.ttf"},   // SemiLight Italic
        {"27fa3fd2cf0ad25dc9d87e4892898e6813916719", "seguisbi.ttf"},   // SemiBold Italic
        {"8f4a51f221db950112948a897deacf6d45c10b1a", "segoeuiz.ttf"},   // Bold Italic
        {"bb6f5d72997a1cd118f67dfc5335943ea600cd1b", "seguibli.ttf"},   // Black Italic
        
        {"4ebb195ad99add8fa11308749363a1599e29e0c7", "SegUIVar.ttf"}
    };
}