using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace Windows_Font_Replacement_Tool;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        WelcomeContent.ButtonClickRequested += WelcomeTabButtonClicked;
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
            case "Tab1Button":
                Tab1Content.Visibility = Visibility.Visible;
                break;
            case "Tab2Button":
                Tab2Content.Visibility = Visibility.Visible;
                break;
            case "Tab3Button":
                Tab3Content.Visibility = Visibility.Visible;
                break;
        }
    }

    /// <summary>
    /// 其他界面下，模拟标签页按钮点击。
    /// </summary>
    public void WelcomeTabButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is null) return;
        switch (((Button)sender).Name)
        {
            case "ButtonS":
                Tab1Button.IsChecked = true;
                break;
            case "ButtonM":
                Tab2Button.IsChecked = true;
                break;
        }
    }

    /// <summary>
    /// 打开导出目录。
    /// </summary>
    public static void OutputDirectoryButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is null) throw new Exception("你是怎么触发这个 Exception 的？");
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

    // /// <summary>
    // /// 打开帮助文档。
    // /// </summary>
    // private void DocumentButton_Click()
    // {
    //     try
    //     {
    //         var pdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Help.pdf");
    //
    //         if (!File.Exists(pdfPath))
    //         {
    //             MessageBox.Show("你把帮助文档弄哪儿去了？", "嗯哼？",
    //                 MessageBoxButton.OK, MessageBoxImage.Error);
    //             return;
    //         }
    //
    //         Process.Start(new ProcessStartInfo
    //         {
    //             FileName = pdfPath,
    //             UseShellExecute = true
    //         });
    //     }
    //     catch (Exception ex)
    //     {
    //         MessageBox.Show($"未能打开文档：{ex.Message}", "错误",
    //             MessageBoxButton.OK, MessageBoxImage.Error);
    //     }
    // }

    /// <summary>
    /// 临时注册给定的字体文件
    /// </summary>
    /// <param name="fontPath">字体文件绝对路径</param>
    /// <returns>返回 0 时表示字体注册成功。</returns>
    [DllImport("gdi32.dll", EntryPoint = "AddFontResourceW", SetLastError = true)]
    internal static extern int AddFontResource(string fontPath);
}