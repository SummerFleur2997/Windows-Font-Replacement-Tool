using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Windows_Font_Replacement_Tool.Framework;

/// <summary>
/// 精细制作模式任务代码。
/// </summary>
public class MultipleReplace : ReplaceTask
{
    /// <summary>
    /// 构造函数，初始化精细制作任务。
    /// </summary>
    public MultipleReplace()
    {
        InitCacheDir(CacheDirPath);
    }

    /// <summary>
    /// 初始化主界面选择文件按钮右边的提示标签。
    /// </summary>
    public void InitInterface()
    {
        foreach (var replaceThread in ReplaceThreads)
        {
            if (replaceThread.HintSign == null) continue;
            replaceThread.HintSign.Visibility = Visibility.Collapsed;
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="customFont"></param>
    /// <param name="button"></param>
    /// <param name="textBlock"></param>
    /// <returns></returns>
    public bool AddReplaceThread(string customFont, Button button, TextBlock textBlock)
    {
        var customFontName = Path.GetFileNameWithoutExtension(customFont);
        var indexes = button.Tag?.ToString()?.Split(new[] { ',' })
            .Select(s => {
                int.TryParse(s.Trim(), out var result);
                return result; });
        if (indexes == null) return false;
        foreach (var index in indexes)
        {
            var sha1 = Sha2File.Keys.ToList()[index];
            ReplaceThreads[index] = new ReplaceThread(customFontName, customFont, sha1, textBlock);
            if (index == 0)
                TaskName = customFontName;
        }
        textBlock.Visibility = Visibility.Visible;
        return MultipleFontCheck();
    }
    
    /// <summary>
    /// 判断选择的字体文件是否合法
    /// </summary>
    /// <returns></returns>
    public bool MultipleFontCheck()
    {
        var returnFlag = true;
            
        foreach (var replaceThread in ReplaceThreads)
        {
            if (replaceThread == null!)
            {
                returnFlag = false;
                continue;
            }
            if (!File.Exists(replaceThread.FontResource) && replaceThread.HintSign != null)
            {
                replaceThread.HintSign.Style = Application.Current.FindResource("OmitIcon") as Style;
                replaceThread.HintSign.ToolTip = new TextBlock { Text="未能找到字体文件，该文件路径可能不正确。" };
                returnFlag = false;
                continue;
            }

            if (replaceThread.HintSign == null)
            {
                returnFlag = false;
            }
            else if (!FontValidation.IsValidFontFile(replaceThread.FontResource))
            {
                replaceThread.HintSign.Style = Application.Current.FindResource("ErrorIcon") as Style;
                replaceThread.HintSign.ToolTip = new TextBlock { Text="该字体文件不合法！" };
                returnFlag = false;
            }
            else
            {
                replaceThread.HintSign.Style = Application.Current.FindResource("VerifiedIcon") as Style;
                var textBlock = new TextBlock
                {
                    MaxWidth=200, TextWrapping=TextWrapping.Wrap,
                    Text=$"当前选择的字体文件是：\n{replaceThread.ThreadName}"
                };
                replaceThread.HintSign.ToolTip = textBlock;
            }
        }
        return returnFlag;
    }
}