using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows;

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
        // TaskName = Path.GetFileNameWithoutExtension(customFont);
        InitCacheDir(CacheDirPath);
    }

    public bool FontCheck()
    {
        bool returnFlag = true;
        bool returnValue = true;
            
        foreach (var replaceThread in ReplaceThreads)
        {
            if (replaceThread == null)
            {
                returnFlag = false;
                returnValue = returnFlag && returnValue;
                continue;
            }
            if (!File.Exists(replaceThread.FontResource))
            {
                replaceThread.HintSign.Style = Application.Current.FindResource("OmitIcon") as Style;
                returnFlag = false;
                returnValue = returnFlag && returnValue;
                continue;
            }
            if (!Validation.IsValidFontFile(replaceThread.FontResource))
            {
                replaceThread.HintSign.Style = Application.Current.FindResource("ErrorIcon") as Style;
                returnFlag = false;
                returnValue = returnFlag && returnValue;
            }
            else
                replaceThread.HintSign.Style = Application.Current.FindResource("VerifiedIcon") as Style;
            returnValue = returnFlag && returnValue;
        }
        return returnValue;
    }

    public void InitInterface()
    {
        foreach (var replaceThread in ReplaceThreads)
            replaceThread.HintSign.Visibility = Visibility.Collapsed;
    }
    
    public bool AddReplaceThread(string customFont, Button button, TextBlock textBlock)
    {
        var customFontName = Path.GetFileNameWithoutExtension(customFont);
        var indexes = button.Tag?.ToString()?.Split(new[] { ',' })
            .Select(s => {
                int.TryParse(s.Trim(), out int result);
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
        return FontCheck();
    }
}