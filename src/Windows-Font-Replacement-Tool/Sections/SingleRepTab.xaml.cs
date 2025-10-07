using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using FontReader;
using Microsoft.Win32;
using Windows_Font_Replacement_Tool.Framework;

namespace Windows_Font_Replacement_Tool.Sections;

public partial class SingleRepTab
{
    public SingleRepTab()
    {
        InitializeComponent();
        OutDirButton.Click += MainWindow.OutputDirectoryButton_Click;
    }

    /// <summary>
    /// 控制切换“快速制作”标签页右下角的控件显示内容，增强交互性。
    /// </summary>
    /// <param name="stackPanel">需要显示的 Panel 名。</param>
    private void SinglePanelUpdate(StackPanel? stackPanel = null)
    {
        PreviewPanel.Visibility = Visibility.Collapsed;
        ProcessingPanel.Visibility = Visibility.Collapsed;
        FinishPanel.Visibility = Visibility.Collapsed;
        OutDirButton.Visibility = Visibility.Collapsed;

        if (stackPanel == null) return;
        stackPanel.Visibility = Visibility.Visible;
        if (stackPanel == FinishPanel) OutDirButton.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// 快速替换选择单个文件。
    /// </summary>
    private void SingleFileOpenButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // 先将快速制作模式任务置空，然后打开个性化字体文件
            App.SingleReplaceTask = null;
            var singleFile = new OpenFileDialog { Filter = "字体文件 (*.ttf,*.otf)|*.ttf;*.otf" };

            // 尝试创建字体文件实例
            if (singleFile.ShowDialog() == false) return;
            var singleFilePath = singleFile.FileName;
            SinglePanelUpdate(PreviewPanel);
            var font = new Font(singleFilePath);

            App.SingleReplaceTask = new SingleReplace(font, SHint);
            
            if (SingleFilePreview(font))
            {
                Run.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            FontExtension.HandleFontException(ex, SHint);
            Previewer.Visibility = Visibility.Collapsed;
            PreviewFontSizeController.IsEnabled = false;
        }
    }

    /// <summary>
    /// 用于在快速替换模式下构建预览，同时检验文件并判断是否能将快速替换处理进程切换为就绪状态。
    /// </summary>
    /// <param name="font">个性化字体文件</param>
    /// <returns>检验认为快速替换处理进程具备就绪条件时返回 true，否则返回 false</returns>
    private bool SingleFilePreview(Font font)
    {
        var stopwatch = Stopwatch.StartNew();
        var task = App.SingleReplaceTask;

        // 若快速替换任务为空或未能临时注册字体，返回 false
        if (task == null || MainWindow.AddFontResource(font.FontPath) != 0) 
            return false;

        // 验证 CJK 字符集数量
        task.MainThread.VerifyCjkCharacterCount();

        // 正常情况下，使用该字体构建预览，然后返回 true
        var uri = new Uri($"file:///{font.FontPath}");
        var fontFamily = new FontFamily(uri + $"#{font.FontFamily()}");
        PreviewA.FontFamily = fontFamily;
        Previewer.Visibility = Visibility.Visible;
        PreviewFontSizeController.IsEnabled = true;

        stopwatch.Stop();
        Console.WriteLine("字体解析完成。");
        Console.WriteLine($"总耗时: {stopwatch.Elapsed.TotalMilliseconds:F4} 毫秒");
        return true;
    }

    /// <summary>
    /// 快速替换模式更改预览窗格文字显示大小。
    /// </summary>
    private void SingleChangeFontSize(object sender, RoutedEventArgs e)
    {
        switch (((Button)sender).Name)
        {
            case "Fm":
                PreviewA.FontSize -= 1;
                break;
            case "Fp":
                PreviewA.FontSize += 1;
                break;
        }
    }

    /// <summary>
    /// 快速替换模式开始制作。
    /// </summary>
    private async void SingleFileRunButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SinglePanelUpdate(ProcessingPanel);
            await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

            if (App.SingleReplaceTask == null) return;
            await App.SingleReplaceTask.TaskStartPropRep();
            await App.SingleReplaceTask.TaskMergeFont();
            App.SingleReplaceTask.TaskFinishing();
        }
        catch (Exception ex)
        {
            SinglePanelUpdate();
            MessageBox.Show(ex.Message, "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Run.IsEnabled = false;
            return;
        }

        Run.IsEnabled = false;
        App.SingleOutputDirectory = App.SingleReplaceTask.OutputDirPath;
        App.SingleReplaceTask = null;
        SinglePanelUpdate(FinishPanel);
    }
}