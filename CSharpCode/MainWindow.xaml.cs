using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using Windows_Font_Replacement_Tool.Framework;

namespace Windows_Font_Replacement_Tool;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 关闭程序。
    /// </summary>
    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    /// <summary>
    /// 最小化窗口。
    /// </summary>
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        SystemCommands.MinimizeWindow(this);
    }

    /// <summary>
    /// 拖动标题栏更改窗口位置。
    /// </summary>
    private void Border_MouseDown(object sender, RoutedEventArgs e)
    {
        DragMove();
    }

    /// <summary>
    /// 点按左侧标签按钮切换标签页。
    /// </summary>
    private void TabButton_Checked(object sender, RoutedEventArgs e)
    {
        WelcomeContent.Visibility = Visibility.Collapsed;
        Tab1Content.Visibility = Visibility.Collapsed;
        Tab2Content.Visibility = Visibility.Collapsed;
        Tab3Content.Visibility = Visibility.Collapsed;

        switch (((RadioButton)sender).Name)
        {
            case "WelcomeTab":
                WelcomeContent.Visibility = Visibility.Visible;
                break;
            case "Tab1":
                Tab1Content.Visibility = Visibility.Visible;
                break;
            case "Tab2":
                Tab2Content.Visibility = Visibility.Visible;
                break;
            case "Tab3":
                Tab3Content.Visibility = Visibility.Visible;
                break;
        }
    }

    /// <summary>
    /// 其他界面下，模拟标签页按钮点击。
    /// </summary>
    private void AltTabButton_Click(object sender, RoutedEventArgs e)
    {
        switch (((Button)sender).Name)
        {
            case "Back1" or "Back2" or "Back3":
                WelcomeTab.IsChecked = true;
                break;
            case "Button1":
                Tab1.IsChecked = true;
                break;
            case "Button2":
                Tab2.IsChecked = true;
                break;
        }
    }

    /// <summary>
    /// 打开导出目录。
    /// </summary>
    private void OutputDirectoryButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var outputDirectory = ((Button)sender).Tag switch
            {
                "Single" => App.SingleOutputDirectory,
                "Multiple" => App.MultipleOutputDirectory,
                _ => null
            };

            if (!Directory.Exists(outputDirectory))
                outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");

            Process.Start(new ProcessStartInfo
            {
                FileName = outputDirectory,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"未能打开导出目录：{ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 打开帮助文档。
    /// </summary>
    private void DocumentButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var pdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Help.pdf");

            if (!File.Exists(pdfPath))
            {
                MessageBox.Show("你把帮助文档弄哪儿去了？", "嗯哼？",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = pdfPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"未能打开文档：{ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 控制切换“快速制作”标签页右下角的控件显示内容，增强交互性。
    /// </summary>
    /// <param name="stackPanel">需要显示的 Panel 名。</param>
    private void SinglePanelUpdate(StackPanel? stackPanel = null)
    {
        PreviewPanel1.Visibility = Visibility.Collapsed;
        ProcessingPanel1.Visibility = Visibility.Collapsed;
        FinishPanel1.Visibility = Visibility.Collapsed;
        OutDirButton1.Visibility = Visibility.Collapsed;

        if (stackPanel == null) return;
        stackPanel.Visibility = Visibility.Visible;
        if (stackPanel == FinishPanel1) OutDirButton1.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// 控制切换“精细制作”标签页右下角的控件显示内容，增强交互性。
    /// </summary>
    /// <param name="dockPanel">需要显示的 Panel 名。</param>
    private void MultiplePanelUpdate(DockPanel? dockPanel = null)
    {
        ProcessingPanel2.Visibility = Visibility.Collapsed;
        FinishPanel2.Visibility = Visibility.Collapsed;

        if (dockPanel == null) return;
        dockPanel.Visibility = Visibility.Visible;
        if (dockPanel == FinishPanel2) OutDirButton2.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// 快速替换选择单个文件。
    /// </summary>
    private void SingleFileOpenButton_Click(object sender, RoutedEventArgs e)
    {
        // 先将快速制作模式任务置空，然后打开个性化字体文件
        App.SingleReplaceTask = null;
        var singleFile = new OpenFileDialog { Filter = "字体文件 (*.ttf,*.otf)|*.ttf;*.otf" };

        if (singleFile.ShowDialog() == false) return;
        var singleFilePath = singleFile.FileName;

        App.SingleReplaceTask = new SingleReplace(singleFilePath, SHint);
        SinglePanelUpdate(PreviewPanel1);
        Run1.IsEnabled = SingleFilePreview(singleFilePath);
    }

    /// <summary>
    /// 临时注册给定的字体文件
    /// </summary>
    /// <param name="fontPath">字体文件绝对路径</param>
    /// <returns>返回 0 时表示字体注册成功。</returns>
    [DllImport("gdi32.dll", EntryPoint = "AddFontResourceW", SetLastError = true)]
    private static extern int AddFontResource(string fontPath);

    /// <summary>
    /// 用于在快速替换模式下构建预览，同时检验文件并判断是否能将快速替换处理进程切换为就绪状态。
    /// </summary>
    /// <param name="fontPath">字体文件绝对路径</param>
    /// <returns>检验认为快速替换处理进程具备就绪条件时返回 true，否则返回 false</returns>
    private bool SingleFilePreview(string fontPath)
    {
        var fontFamilyName = FontValidation.GetFontFamily(fontPath);
        // 若快速替换任务为空，或字体检查结果不合法，返回 false，禁用部分控件
        if (App.SingleReplaceTask == null || !App.SingleReplaceTask.SingleFontCheck(fontFamilyName))
        {
            Previewer1.Visibility = Visibility.Collapsed;
            PreviewFontSizeController.IsEnabled = false;
            return false;
        }

        // 若未能临时注册字体，返回 false（字体可能存在潜在问题导致无法注册）
        if (AddFontResource(fontPath) != 0) return false;
        // 正常情况下，使用该字体构建预览，然后返回 true
        var uri = new Uri($"file:///{fontPath}");
        FontFamily fontFamily = new FontFamily(uri + $"#{fontFamilyName}");
        PreviewA.FontFamily = fontFamily;
        Previewer1.Visibility = Visibility.Visible;
        PreviewFontSizeController.IsEnabled = true;
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
        SinglePanelUpdate(ProcessingPanel1);
        await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

        if (App.SingleReplaceTask == null) return;

        try
        {
            await App.SingleReplaceTask.TaskStartPropRep();
            await App.SingleReplaceTask.TaskMergeFont();
            App.SingleReplaceTask.TaskFinishing();
        }
        catch (Exception ex)
        {
            SinglePanelUpdate();
            MessageBox.Show(ex.Message, "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Run1.IsEnabled = false;
            return;
        }

        Run1.IsEnabled = false;
        App.SingleOutputDirectory = App.SingleReplaceTask.OutputDirPath;
        App.SingleReplaceTask = null;
        SinglePanelUpdate(FinishPanel1);
    }

    /// <summary>
    /// 精细替换选择单个文件。
    /// </summary>
    private void MultipleFileOpenButton_Click(object sender, RoutedEventArgs e)
    {
        if (App.MultipleReplaceTask == null) MultiplePanelUpdate();

        var multipleFile = new OpenFileDialog { Filter = "字体文件 (*.ttf,*.otf)|*.ttf;*.otf" };
        multipleFile.Filter = "字体文件 (*.ttf,*.otf)|*.ttf;*.otf";

        if (multipleFile.ShowDialog() == false) return;
        var multipleFilePath = multipleFile.FileName;

        App.MultipleReplaceTask ??= new MultipleReplace();
        var button = (Button)sender;
        var tbName = button.Name + "S";
        var textBlock = FindName(tbName) as TextBlock;

        if (textBlock == null) return;
        Run2.IsEnabled = App.MultipleReplaceTask.AddReplaceThread(multipleFilePath, button, textBlock);
    }

    /// <summary>
    /// 精细替换模式开始制作。
    /// </summary>
    private async void MultipleFileRunButton_Click(object sender, RoutedEventArgs e)
    {
        MultiplePanelUpdate(ProcessingPanel2);
        await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

        if (App.MultipleReplaceTask == null) return;
        if (!App.MultipleReplaceTask.MultipleFontCheck())
        {
            Run2.IsEnabled = false;
            return;
        }

        try
        {
            await App.MultipleReplaceTask.TaskStartPropRep();
            await App.MultipleReplaceTask.TaskMergeFont();
        }
        catch (Exception ex)
        {
            MultiplePanelUpdate();
            MessageBox.Show(ex.Message, "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Run2.IsEnabled = false;
            return;
        }

        App.MultipleReplaceTask.TaskFinishing();
        App.MultipleReplaceTask.InitInterface();
        Run2.IsEnabled = false;
        App.MultipleOutputDirectory = App.MultipleReplaceTask.OutputDirPath;
        App.MultipleReplaceTask = null;
        MultiplePanelUpdate(FinishPanel2);
    }
}