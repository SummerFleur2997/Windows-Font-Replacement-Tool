using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Windows_Font_Replacement_Tool.Framework;

public static class FontExtension
{
    /// <summary>
    /// 处理创建字体过程中产生的错误。
    /// </summary>
    /// <param name="ex">错误实例</param>
    /// <param name="textBlock">用于交互式响应的控件，告诉用户发生了什么。</param>
    public static void HandleFontException(Exception ex, TextBlock textBlock)
    {
        var(text, style) = ex switch
        {
            FileNotFoundException => ("未能找到字体文件！", Application.Current.FindResource("OmitIcon") as Style),
            NotSupportedException => ("字体格式不受支持！", Application.Current.FindResource("ErrorIcon") as Style),
            FileLoadException => ("字体文件已损坏！", Application.Current.FindResource("ErrorIcon") as Style),
            _ => ("未知异常！", Application.Current.FindResource("ErrorIcon") as Style)
        };
        textBlock.Style = style;
        textBlock.ToolTip = new TextBlock
        {
            MaxWidth = 200, TextWrapping = TextWrapping.Wrap,
            Text = text
        };
    }
}