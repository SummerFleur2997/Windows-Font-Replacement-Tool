using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WFRT.Framework;

/// <summary>
/// 类 SingleReplace 与 MultipleReplace 的基类，仅用于存储类属性与类函数，不应当被实例化。
/// </summary>
internal class ReplaceTask : IDisposable
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
    /// 用于存储 19 种字形的处理进程，索引对应值列表如下：
    /// </summary>
    /// <inheritdoc cref="ResourceHelper.NameTableData"/>
    protected readonly ReplaceThread[] ReplaceThreads = new ReplaceThread[19];

    /// <summary>
    /// 雅黑三件套的前缀。
    /// </summary>
    private readonly string[] _msyhThreeMusketeers = { "msyh", "msyhl", "msyhbd" };

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
    /// 启动属性替换任务
    /// </summary>
    public async Task TaskStartPropRep()
    {
        // 初始化导出文件夹和缓存文件夹
        OutputDirPath = CreateOutputDir(TaskName);
        // 使用Task.Run异步执行以下操作
        await Task.Run(() =>
        {
            // 执行属性替换
            foreach (var thread in ReplaceThreads)
                thread.RunPropertyRep(OutputDirPath);
        });
    }

    /// <summary>
    /// 多线程运行 Python程序合并字体为 ttc。
    /// </summary>
    public async Task TaskMergeFont()
    {
        // 调用 ParallelRun 方法进行多线程处理
        var hasError = await Task.Run(() => Utilities.ParallelRun(_msyhThreeMusketeers, RunMerge));
        if (hasError) throw new Exception("此字体文件不受支持！");
    }

    /// <summary>
    /// 将不需要的字体文件删除。。
    /// </summary>
    public void TaskFinishing()
    {
        foreach (var file in _msyhThreeMusketeers)
        foreach (var postfix in Enumerable.Range(1, 2))
        {
            var fileName = string.Concat(file, postfix.ToString("00"), ".ttf");
            var path = Path.Combine(OutputDirPath, fileName);
            File.Delete(path);
        }
    }

    public void Dispose()
    {
        // 释放字体资源
        foreach (var thread in ReplaceThreads)
            thread.Dispose();
        GC.SuppressFinalize(this);
    }
}