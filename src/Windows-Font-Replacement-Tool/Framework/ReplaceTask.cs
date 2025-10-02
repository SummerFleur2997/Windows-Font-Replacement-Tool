using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Windows_Font_Replacement_Tool.Framework;

/// <summary>
/// 类 SingleReplace 与 MultipleReplace 的基类，仅用于存储类属性与类函数，不应当被实例化。
/// </summary>
public class ReplaceTask
{
    /// <summary>
    /// 任务的名称，用于命名输出文件夹。
    /// </summary>
    protected string TaskName { get; set; } = null!;

    /// <summary>
    /// 任务的输出文件夹绝对路径。
    /// </summary>
    public string OutputDirPath { get; private set; } = null!;

    /// <summary>
    /// 任务的缓存文件夹绝对路径。
    /// </summary>
    protected string CacheDirPath => GetCacheDir();

    /// <summary>
    /// 用于存储 19 种字形的处理进程，索引对应值列表如下：
    /// <list type="table">
    ///   <item><term>[0]</term><description>msyh - Regular</description></item>
    ///   <item><term>[1]</term><description>msyhUI - Regular</description></item>
    ///   <item><term>[2]</term><description>msyh - Light</description></item>
    ///   <item><term>[3]</term><description>msyhUI - Light</description></item>
    ///   <item><term>[4]</term><description>msyh - Bold</description></item>
    ///   <item><term>[5]</term><description>msyhUI - Bold</description></item>
    ///   <item><term>[6]</term><description>SegoeUI - Regular</description></item>
    ///   <item><term>[7]</term><description>SegoeUI - Light</description></item>
    ///   <item><term>[8]</term><description>SegoeUI - SemiLight</description></item>
    ///   <item><term>[9]</term><description>SegoeUI - SemiBold</description></item>
    ///   <item><term>[10]</term><description>SegoeUI - Bold</description></item>
    ///   <item><term>[11]</term><description>SegoeUI - Black</description></item>
    ///   <item><term>[12]</term><description>SegoeUI - Italic</description></item>
    ///   <item><term>[13]</term><description>SegoeUI - Light Italic</description></item>
    ///   <item><term>[14]</term><description>SegoeUI - SemiLight Italic</description></item>
    ///   <item><term>[15]</term><description>SegoeUI - SemiBold Italic</description></item>
    ///   <item><term>[16]</term><description>SegoeUI - Bold Italic</description></item>
    ///   <item><term>[17]</term><description>SegoeUI - Black Italic</description></item>
    ///   <item><term>[18]</term><description>Segoe Variable</description></item>
    /// </list>
    /// </summary>
    protected readonly ReplaceThread[] ReplaceThreads = new ReplaceThread[19];

    /// <summary>
    /// 给定任务名称，新建导出文件夹。
    /// </summary>
    /// <param name="taskName">任务名称</param>
    /// <returns>导出文件夹绝对路径</returns>
    private static string CreateOutputDir(string taskName)
    {
        // 获取当前的系统时间
        var outputDir = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + taskName;
        // 创建导出文件夹
        var outputDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output", outputDir);
        Directory.CreateDirectory(outputDirPath);
        return outputDirPath;
    }

    /// <summary>
    /// 新建 cache 文件夹。
    /// </summary>
    /// <returns>cache文件夹绝对路径</returns>
    private static string GetCacheDir()
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output", "cache");
    }

    /// <summary>
    /// 初始化 cache 文件夹。
    /// </summary>
    protected static void InitCacheDir(string cacheDirPath)
    {
        // 正常情况下每次运行任务前 cache 文件夹应该不存在。
        if (!Directory.Exists(cacheDirPath)) return;
        // 若 cache 文件夹存在，则代表前一次运行出现了问题或被手动停止，需要删除该目录
        Directory.Delete(cacheDirPath, true);
    }

    /// <summary>
    /// 给定文件名，返回该文件最终的存储路径。
    /// </summary>
    /// <param name="fileName">给定的文件名</param>
    /// <returns>导出文件绝对路径</returns>
    private string OutputFilePath(string fileName)
    {
        return Path.Combine(OutputDirPath, fileName);
    }

    /// <summary>
    /// Python 程序，合并两个 ttf 为 ttc。
    /// </summary>
    /// <param name="filePrefix">导出的 ttf 文件名前缀</param>
    /// <returns>Python程序退出代码</returns>
    private int RunMerge(string filePrefix)
    {
        var outputDirName = Path.GetFileName(OutputDirPath);
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(App.ResourcePath, "functions.exe"),
                Arguments = $"mergeTTC \"{outputDirName}\" \"{filePrefix}\"",
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
    /// 多线程运行 Python 程序进行属性替换。
    /// </summary>
    public async Task TaskStartPropRep()
    {
        // 初始化导出文件夹和缓存文件夹
        OutputDirPath = CreateOutputDir(TaskName);
        Directory.CreateDirectory(CacheDirPath);
        // 调用 ParallelRun 方法进行多线程处理
        var hasError = await Task.Run(() => Utilities.ParallelRun(ReplaceThreads, thread => thread.RunPropertyRep()));
        if (hasError) throw new Exception("关键依赖文件缺失，请重新下载并安装本工具");
        // 重命名所有字体名称为人类可以看懂的文字
        foreach (var sha1 in Sha2File.Keys)
        {
            var originalFilePath = Path.Combine(CacheDirPath, sha1);
            var newFilePath = Path.Combine(CacheDirPath, Sha2File[sha1]);
            File.Move(originalFilePath, newFilePath);
        }
    }

    /// <summary>
    /// 多线程运行 Python程序合并字体为 ttc。
    /// </summary>
    public async Task TaskMergeFont()
    {
        // 默认的需要合并为 ttc 的字体前缀（雅黑三件套）
        string[] filePrefixes = { "msyh", "msyhl", "msyhbd" };
        // 调用 ParallelRun 方法进行多线程处理
        var hasError = await Task.Run(() => Utilities.ParallelRun(filePrefixes, RunMerge));
        if (hasError) throw new Exception("关键依赖文件缺失，请重新下载并安装本工具");
    }

    /// <summary>
    /// 将字体从 cache 目录移动至导出目录，然后删除 cache 目录。
    /// </summary>
    public void TaskFinishing()
    {
        foreach (var sha1 in Sha2File.Keys)
        {
            // 获取导出字体文件的前三个字母
            var seg = Sha2File[sha1][..3];
            // 判断该字体是否是 Segoe 系列字体，若不是，continue
            if (!string.Equals(seg, "seg", StringComparison.OrdinalIgnoreCase)) continue;
            // 若是，将其移动至导出目录
            var originalFilePath = Path.Combine(CacheDirPath, Sha2File[sha1]);
            var newFilePath = OutputFilePath(Sha2File[sha1]);
            File.Move(originalFilePath, newFilePath);
        }

        // 删除现有的 cache 文件夹
        InitCacheDir(CacheDirPath);
    }

    /// <summary>
    /// 存储 xmls 文件夹内每一个哈希值对应的文件名。
    /// </summary>
    protected static readonly Dictionary<string, string> Sha2File = new()
    {
        { "d46344fc3841184ac741685d53f0b01cd11865e7", "msyh01.ttf" },       //  0.Regular
        { "791e18622ff1011b9a6c68bfb8796258a3a1cf85", "msyh02.ttf" },       //  1.Regular UI
        { "7d70dd165648425f19346947a268a8ee58262eb2", "msyhl01.ttf" },      //  2.Light
        { "83eb72dd5f5285488ed2ca4d72810e266bc2fd4e", "msyhl02.ttf" },      //  3.Light UI
        { "ea87b3ddadc073d04b25039f9e41bfbefd8b0eba", "msyhbd01.ttf" },     //  4.Bold
        { "e49a970410fe1b22ac10bdc7c656117c74f22b39", "msyhbd02.ttf" },     //  5.Bold UI

        { "5213a5c8e131255d461ebc4e8b5ed71eaedf01dc", "segoeui.ttf" },      //  6.Regular
        { "12144c399e57c776e7b3ed8f689cce09df5c1a53", "segoeuil.ttf" },     //  7.Light
        { "dde72eaa7ba41cac65e21350b613b95f0c04bd4d", "segoeuisl.ttf" },    //  8.SemiLight
        { "f80b4059b145408bc7948ff57aa2d326a08afa31", "seguisb.ttf" },      //  9.SemiBold
        { "c6adb891613704c322580732b92b6566a7e80684", "segoeuib.ttf" },     // 10.Bold
        { "b6379d63dd25e0c01c09dec61e56b4e2a4fc2456", "seguibl.ttf" },      // 11.Black

        { "f8808c6fcf9ce4a74a6682fa5d886fe25a60f11b", "segoeuii.ttf" },     // 12.Italic
        { "b6ba87aa742964e7103ff654516a017ddf6031e0", "seguili.ttf" },      // 13.Light Italic
        { "cd0bed1303626bf6cfc7412206744c5bd794be1b", "seguisli.ttf" },     // 14.SemiLight Italic
        { "27fa3fd2cf0ad25dc9d87e4892898e6813916719", "seguisbi.ttf" },     // 15.SemiBold Italic
        { "8f4a51f221db950112948a897deacf6d45c10b1a", "segoeuiz.ttf" },     // 16.Bold Italic
        { "bb6f5d72997a1cd118f67dfc5335943ea600cd1b", "seguibli.ttf" },     // 17.Black Italic

        { "4ebb195ad99add8fa11308749363a1599e29e0c7", "SegUIVar.ttf" }
    };
}