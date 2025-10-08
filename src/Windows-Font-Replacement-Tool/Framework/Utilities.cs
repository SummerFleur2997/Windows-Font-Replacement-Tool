using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Windows_Font_Replacement_Tool.Framework;

public static class Utilities
{
    /// <summary>
    /// 获取当前计算机线程数。
    /// </summary>
    private static readonly int MaxDegree = Environment.ProcessorCount;

    /// <summary>
    /// 用于提示报错信息弹窗。
    /// </summary>
    /// <param name="message">错误提示</param>
    /// <param name="ex">错误实例</param>
    /// <param name="exit">是否应当结束进程</param>
    public static void HandleError(string message, Exception ex, bool exit = false)
    {
        var warning = new StringBuilder();
        warning.AppendLine(message);
        warning.AppendLine("错误信息：" + ex.Message);

        MessageBox.Show(warning.ToString(), "错误",
            MessageBoxButton.OK, MessageBoxImage.Error);

        if (exit) Application.Current.Shutdown();
    }

    /// <summary>
    /// 以并行方式对集合中的每个元素执行指定操作，最大线程数基于处理器线程数。
    /// 若任意元素操作的退出值为 99 或 -1，则停止整个操作并记录错误。
    /// </summary>
    /// <param name="items">需要并行处理的元素集合</param>
    /// <param name="action">元素的操作函数，其应当为 functions.py 的命令行调用函数</param>
    /// <typeparam name="T">集合元素的类型参数</typeparam>
    /// <returns>当任意操作返回错误退出值（99 或 -1）时返回 true，否则返回 false</returns>
    public static bool ParallelRun<T>(IEnumerable<T> items, Func<T, int> action)
    {
        var hasError = false;
        var lockObj = new object();

        Parallel.ForEach(
            items,
            new ParallelOptions { MaxDegreeOfParallelism = MaxDegree },
            (item, state) =>
            {
                var exitCode = action(item);
                if (exitCode is 99 or -1)
                {
                    lock (lockObj)
                    {
                        hasError = true;
                    }

                    state.Stop();
                }
            }
        );
        return hasError;
    }

    /// <summary>
    /// 使用大端序列读取一个 16 位短整型无符号数。
    /// </summary>
    public static ushort ReadUInt16BigEndian(this BinaryReader br)
    {
        var bytes = br.ReadBytes(2);
        return BinaryPrimitives.ReadUInt16BigEndian(bytes);
    }
}