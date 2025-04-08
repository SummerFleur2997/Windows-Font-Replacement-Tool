using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Windows_Font_Replacement_Tool.Framework;

/// <summary>
/// 快速制作模式任务代码。
/// </summary>
public class SingleReplace: ReplaceTask
{
    /// <summary>
    /// 构造函数，初始化快速制作任务。
    /// </summary>
    /// <param name="customFont">个性化字体文件绝对路径</param>
    /// <param name="textBlock"></param> todo
    public SingleReplace(string customFont, TextBlock textBlock)
    {
        TaskName = Path.GetFileNameWithoutExtension(customFont);
        InitCacheDir(CacheDirPath);
        var index = 0;
        foreach (var sha1 in Sha2File.Keys)
        {
            ReplaceThreads[index] = new ReplaceThread(TaskName, customFont, sha1, textBlock);
            index++;
        }
    }

    /// <summary>
    /// 判断选择的字体文件是否合法
    /// </summary>
    /// <returns></returns>
    public bool SingleFontCheck(string fontFamilyName)
    {
        var replaceThread = ReplaceThreads[0];
        if (!File.Exists(replaceThread.FontResource) && replaceThread.HintSign != null)
        {
            replaceThread.HintSign.Style = Application.Current.FindResource("OmitIcon") as Style;
            replaceThread.HintSign.ToolTip = new TextBlock { Text="未能找到字体文件，该文件路径可能不正确。" };
            return false;
        }
        if (replaceThread.HintSign == null)
        {
            return false;
        }
        if (!FontValidation.IsValidFontFile(replaceThread.FontResource))
        {
            replaceThread.HintSign.Style = Application.Current.FindResource("ErrorIcon") as Style;
            replaceThread.HintSign.ToolTip = new TextBlock { Text="该字体文件不合法！" };
            return false;
        }
        if (fontFamilyName == "**Error**")
        {
            replaceThread.HintSign.Style = Application.Current.FindResource("ErrorIcon") as Style;
            replaceThread.HintSign.ToolTip = new TextBlock { Text="未能获取当前字体信息！" };
            return false;
        }
        
        var cjkCharacterCount = FontValidation.GetCjkCharacterCount(replaceThread.FontResource);
        switch (cjkCharacterCount)
        {
            case -1:
                replaceThread.HintSign.Style = Application.Current.FindResource("ErrorIcon") as Style;
                replaceThread.HintSign.ToolTip = new TextBlock
                {
                    MaxWidth=200, TextWrapping=TextWrapping.Wrap,
                    Text="未能解析该字体文件！"
                };
                return false;
            case < 256:
                replaceThread.HintSign.Style = Application.Current.FindResource("ErrorIcon") as Style;
                replaceThread.HintSign.ToolTip = new TextBlock
                {
                    MaxWidth=200, TextWrapping=TextWrapping.Wrap,
                    Text="该字体内未检测到汉字字符，请通过预览窗格确定是否需要使用该字体进行制作！"
                };
                return true;
            case < 7000:
                replaceThread.HintSign.Style = Application.Current.FindResource("WarningIcon") as Style;
                replaceThread.HintSign.ToolTip = new TextBlock
                {
                    MaxWidth=200, TextWrapping=TextWrapping.Wrap,
                    Text=$"当前选择的字体文件是：\n{replaceThread.ThreadName}\n" +
                         $"该字体汉字字符数量较少（仅 {cjkCharacterCount} 个），可能存在显示缺字问题。"
                };
                return true;
            default:
                replaceThread.HintSign.Style = Application.Current.FindResource("VerifiedIcon") as Style;
                var textBlock = new TextBlock
                {
                    MaxWidth=200, TextWrapping=TextWrapping.Wrap,
                    Text=$"当前选择的字体文件是：\n{replaceThread.ThreadName}"
                };
                replaceThread.HintSign.ToolTip = textBlock;
                return true;
        }
    }
}