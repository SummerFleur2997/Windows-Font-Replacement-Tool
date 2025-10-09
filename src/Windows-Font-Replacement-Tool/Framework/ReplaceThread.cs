using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using FontTool;

namespace WFRT.Framework;

/// <summary>
/// 一种字形的替换进程，包含 Python 程序的启动函数。
/// </summary>
internal class ReplaceThread : IDisposable
{
    /// <summary>
    /// 进程名称信息，应为字体的实际文件名。
    /// </summary>
    public string ThreadName { get; }

    /// <summary>
    /// 个性化字体资源的绝对路径。
    /// </summary>
    public Font FontResource { get; }

    /// <summary>
    /// 字体表的索引值。
    /// </summary>
    private int Index { get; }

    /// <summary>
    /// 提示标签，用于反馈字体检验的合法性。
    /// </summary>
    public TextBlock HintSign { get; }

    private List<NameTableData> DataList { get; }

    /// <summary>
    /// 对于中文字体文件，检测 CJK 字符集数量，防止使用英文字体集创建字体导致的显示不正常。
    /// </summary>
    public void VerifyCjkCharacterCount()
    {
        // 获取 CJK 统一表意字符（0x4E00~0x9FFF）范围内的字符数量。
        var cjkCharacterCount = FontResource.GetCharacterCountFromTo(0x4E00, 0x9FFF);
        switch (cjkCharacterCount)
        {
            // CJK 字符集数量小于 256，直接认为该字体内不包含 CJK 字符，在 UI 内绘制严重警告标志
            case < 256:
                HintSign.Style = Application.Current.FindResource("ErrorIcon") as Style;
                HintSign.ToolTip = new TextBlock
                {
                    MaxWidth = 200, TextWrapping = TextWrapping.Wrap,
                    Text = "该字体内未检测到汉字字符，请通过预览窗格确定是否需要使用该字体进行制作！"
                };
                break;
            // CJK 字符集数量小于 7000，可能存在缺字问题，在 UI 内绘制警告标志
            case < 7000:
                HintSign.Style = Application.Current.FindResource("WarningIcon") as Style;
                HintSign.ToolTip = new TextBlock
                {
                    MaxWidth = 200, TextWrapping = TextWrapping.Wrap,
                    Text = $"当前选择的字体文件是：\n{ThreadName}\n" +
                           $"该字体汉字字符数量较少（仅 {cjkCharacterCount} 个），可能存在显示缺字问题。"
                };
                break;
            // 没毛病的情况
            default:
                HintSign.Style = Application.Current.FindResource("VerifiedIcon") as Style;
                var textBlock = new TextBlock
                {
                    MaxWidth = 200, TextWrapping = TextWrapping.Wrap,
                    Text = $"当前选择的字体文件是：\n{ThreadName}"
                };
                HintSign.ToolTip = textBlock;
                break;
        }
    }

    /// <summary>
    /// Python 程序，替换字体属性。
    /// </summary>
    /// <returns>Python程序退出代码</returns>
    public void RunPropertyRep(string saveDir)
    {
        var data = DataList[Index];
        var savePath = Path.Combine(saveDir, data.FontName + ".ttf");

        FontResource.LoadTableData();
        FontResource.ReplaceTable(data.NameTable);
        FontResource.UpdateTableOffset();
        FontResource.Save(savePath);
    }

    /// <summary>
    /// 初始化一种字形的替换进程。
    /// </summary>
    /// <param name="threadName">去除拓展名后的字体文件名</param>
    /// <param name="fontResource">个性化字体文件</param>
    /// <param name="index">字体表的索引值</param>
    /// <param name="hintSign">精细制作模式下的提示标志</param>
    public ReplaceThread(string threadName, Font fontResource, int index, TextBlock hintSign)
    {
        ThreadName = threadName;
        FontResource = fontResource;
        Index = index;
        HintSign = hintSign;
        DataList = ResourceHelper.NameTableData();
    }

    public void Dispose()
    {
        FontResource.Dispose();
        DataList.Clear();
        GC.SuppressFinalize(this);
    }
}