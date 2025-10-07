using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Windows_Font_Replacement_Tool.Framework;

public static class Utilities
{
    /// <summary>
    /// 获取当前计算机线程数。
    /// </summary>
    private static readonly int MaxDegree = Environment.ProcessorCount;

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
}