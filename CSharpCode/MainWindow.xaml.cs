using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Diagnostics;
using Microsoft.Win32;
using Windows_Font_Replacement_Tool.Framework;
using System.Runtime.InteropServices;

namespace Windows_Font_Replacement_Tool;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private SingleReplace?   SingleReplaceTask   { get; set; }
    private MultipleReplace? MultipleReplaceTask { get; set; }
    private string? OutputDirectory { get; set; }
    
    public MainWindow()
    {
        InitializeComponent();
        HashTab.Initialize();
    }

    /// <summary>
    /// 关闭程序。
    /// </summary>
    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        SingleReplaceTask   = null;
        MultipleReplaceTask = null;
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
            var outputDirectory = Directory.Exists(OutputDirectory)
                ? OutputDirectory
                : AppDomain.CurrentDomain.BaseDirectory;
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
            string pdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Help.pdf");
    
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
    private void SinglePanelUpdate(StackPanel? stackPanel=null)
    {
        PreviewPanel1.Visibility = Visibility.Collapsed;
        ProcessingPanel1.Visibility = Visibility.Collapsed;
        FinishPanel1.Visibility = Visibility.Collapsed;
        OutDirButton1.Visibility = Visibility.Collapsed;
        
        if (stackPanel == null) return;
        stackPanel.Visibility = Visibility.Visible;
        if (stackPanel == FinishPanel1)
            OutDirButton1.Visibility = Visibility.Visible;
    }
    
    /// <summary>
    /// 控制切换“精细制作”标签页右下角的控件显示内容，增强交互性。
    /// </summary>
    /// <param name="dockPanel">需要显示的 Panel 名。</param>
    private void MultiplePanelUpdate(DockPanel? dockPanel=null)
    {
        ProcessingPanel2.Visibility = Visibility.Collapsed;
        FinishPanel2.Visibility = Visibility.Collapsed;
        
        if (dockPanel == null) return;
        dockPanel.Visibility = Visibility.Visible;
        if (dockPanel == FinishPanel2)
            OutDirButton2.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// 快速替换选择单个文件。
    /// </summary>
    private void SingleFileOpenButton_Click(object sender, RoutedEventArgs e)
    {
        SingleReplaceTask = null;
        var singleFile = new OpenFileDialog { Filter = "字体文件 (*.ttf,*.otf)|*.ttf;*.otf" };
        
        if (singleFile.ShowDialog() == false) return;
        var singleFilePath = singleFile.FileName;
        
        SingleReplaceTask = new SingleReplace(singleFilePath, SHint);
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
    /// 快速替换模式构建预览。
    /// </summary>
    /// <param name="fontPath">字体文件绝对路径</param>
    private bool SingleFilePreview(string fontPath)
    {
        if (AddFontResource(fontPath) != 0) return false;
        
        var uri = new Uri($"file:///{fontPath}");
        var fontFamilyName = FontValidation.GetFontFamily(fontPath);
        if (SingleReplaceTask == null || !SingleReplaceTask.SingleFontCheck(fontFamilyName))
        {
            Previewer1.Visibility = Visibility.Collapsed;
            PreviewFontSizeController.IsEnabled = false;
            return false;
        }
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

        if (SingleReplaceTask == null) return;
        
        try
        {
            await SingleReplaceTask.TaskStartPropRep();
            await SingleReplaceTask.TaskMergeFont();
            SingleReplaceTask.TaskFinishing();
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
        OutputDirectory = SingleReplaceTask.OutputDirPath;
        SingleReplaceTask = null;
        SinglePanelUpdate(FinishPanel1);
    }
    
    /// <summary>
    /// 精细替换选择单个文件。
    /// </summary>
    private void MultipleFileOpenButton_Click(object sender, RoutedEventArgs e)
    {
        var multipleFile = new OpenFileDialog { Filter = "字体文件 (*.ttf,*.otf)|*.ttf;*.otf" };
        multipleFile.Filter = "字体文件 (*.ttf,*.otf)|*.ttf;*.otf";
        
        if (multipleFile.ShowDialog() == false) return;
        var multipleFilePath = multipleFile.FileName;
        
        MultipleReplaceTask ??= new MultipleReplace();
        var button = (Button)sender;
        var tbName = button.Name + "S";
        var textBlock = FindName(tbName) as TextBlock;
        
        if (textBlock == null) return;
        Run2.IsEnabled = MultipleReplaceTask.AddReplaceThread(multipleFilePath, button, textBlock);
    }
    
    /// <summary>
    /// 精细替换模式开始制作。
    /// </summary>
    private async void MultipleFileRunButton_Click(object sender, RoutedEventArgs e)
    {
        MultiplePanelUpdate(ProcessingPanel2);
        await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

        if (MultipleReplaceTask == null) return;
        if (!MultipleReplaceTask.MultipleFontCheck())
        {
            Run2.IsEnabled = false;
            return;
        }
        
        try
        {
            await MultipleReplaceTask.TaskStartPropRep();
            await MultipleReplaceTask.TaskMergeFont();
        }
        catch (Exception ex)
        {
            MultiplePanelUpdate();
            MessageBox.Show(ex.Message, "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Run2.IsEnabled = false;
            return;
        }
        
        MultipleReplaceTask.TaskFinishing();
        MultipleReplaceTask.InitInterface();
        Run2.IsEnabled = false;
        OutputDirectory = MultipleReplaceTask.OutputDirPath;
        MultipleReplaceTask = null;
        MultiplePanelUpdate(FinishPanel2);
    }
}
