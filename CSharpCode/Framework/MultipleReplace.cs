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
    /// 初始化主界面选择文件按钮右边的提示标签，将它们全部隐藏。
    /// </summary>
    public void InitInterface()
    {
        foreach (var replaceThread in ReplaceThreads)
            replaceThread.HintSign.Visibility = Visibility.Collapsed;
    }
    
    /// <summary>
    /// 向 <see cref="ReplaceTask.ReplaceThreads"/> 中添加字体处理进程。
    /// </summary>
    /// <param name="customFont">个性化字体文件绝对路径</param>
    /// <param name="button">调用该方法时按下的按钮</param>
    /// <param name="textBlock"></param>
    /// <returns>所有进程是否就绪</returns>
    public bool AddReplaceThread(string customFont, Button button, TextBlock textBlock)
    {
        // 获取当前字体文件的名称
        var customFontName = Path.GetFileNameWithoutExtension(customFont);
        
        // 每个按钮有一个 Tag 记录着当前进程应该存放到 ReplaceThreads 的哪个索引下，此处为解析该按钮的 Tag 值
        var indexes = button.Tag?.ToString()?.Split(new[] { ',' })
            .Select(s => {
                int.TryParse(s.Trim(), out var result);
                return result; });
        
        // 正常情况下应该不会触发
        if (indexes == null) return false;
        
        // 遍历按钮的 Tag 值列表，中文字体列表长度为 2，西文字体列表长度为 1
        foreach (var index in indexes)
        {
            // 从字典中获取该进程需要参考哪个 xml，然后将进程放到对应的索引下
            var sha1 = Sha2File.Keys.ToList()[index];
            ReplaceThreads[index] = new ReplaceThread(customFontName, customFont, sha1, textBlock);
            
            // 将 index=0 对应的文件名（msyh regular）作为当前任务的名称
            if (index == 0) TaskName = customFontName;
        }
        
        // 亮起当前按钮右侧的提示标签
        textBlock.Visibility = Visibility.Visible;
        return MultipleFontCheck();
    }
    
    /// <summary>
    /// 判断所有所有字体处理进程是否合法且就绪。
    /// </summary>
    public bool MultipleFontCheck()
    {
        var returnFlag = true;
        
        // 遍历所有进程判断它们是否合法且就绪
        foreach (var replaceThread in ReplaceThreads)
        {
            // 当前进程为空
            if (replaceThread == null!)
            {
                returnFlag = false;
                continue;
            }
            
            // 当前进程对应的字体文件不存在（为什么才过了几秒，字体就不存在了呢？你干了啥？）
            if (!File.Exists(replaceThread.FontResource))
            {
                replaceThread.HintSign.Style = Application.Current.FindResource("OmitIcon") as Style;
                replaceThread.HintSign.ToolTip = new TextBlock { Text="未能找到字体文件，该文件路径可能不正确。" };
                returnFlag = false;
            }
            // 字体文件解析结果为不合法（防止小白改拓展名）
            else if (!FontValidation.IsValidFontFile(replaceThread.FontResource))
            {
                replaceThread.HintSign.Style = Application.Current.FindResource("ErrorIcon") as Style;
                replaceThread.HintSign.ToolTip = new TextBlock { Text="该字体文件不合法！" };
                returnFlag = false;
            }
            // 没毛病的情况
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