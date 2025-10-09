using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;
using System.Windows;

namespace WFRT.Framework;

internal static class Utilities
{
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
    /// 使用大端序列读取一个 16 位短整型无符号数。
    /// </summary>
    public static ushort ReadUInt16BigEndian(this BinaryReader br)
    {
        var bytes = br.ReadBytes(2);
        return BinaryPrimitives.ReadUInt16BigEndian(bytes);
    }
}