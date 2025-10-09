using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FontTool;
using Path = System.IO.Path;

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
    /// 合并 msyh 三件套字体为 ttc，然后删除对应 ttf。
    /// </summary>
    public async Task TaskMergeFont() =>
        // 使用Task.Run异步执行以下操作
        await Task.Run(() =>
        {
            // 执行字体合并
            foreach (var pre in _msyhThreeMusketeers)
            {
                var fonts = Enumerable.Range(1, 2)
                    .Select(postfix => new Font(Path.Combine(OutputDirPath, $"{pre}{postfix:00}.ttf")))
                    .ToList();
                var ttc = new FontCollection(fonts);
                ttc.Save(Path.Combine(OutputDirPath, $"{pre}.ttc"));
                ttc.Dispose();
                foreach (var font in fonts) File.Delete(font.FontPath);
            }
        });

    public void Dispose()
    {
        // 释放字体资源
        foreach (var thread in ReplaceThreads)
            thread.Dispose();
        GC.SuppressFinalize(this);
    }
}