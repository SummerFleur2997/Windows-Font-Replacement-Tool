using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using FontReader;
using Microsoft.Win32;
using Windows_Font_Replacement_Tool.Framework;

namespace Windows_Font_Replacement_Tool.Sections;

public partial class MultipleRepTab
{
    public MultipleRepTab()
    {
        InitializeComponent();
        OutDirButton.Click += MainWindow.OutputDirectoryButton_Click;
    }

    /// <summary>
    /// 控制切换“精细制作”标签页右下角的控件显示内容，增强交互性。
    /// </summary>
    /// <param name="dockPanel">需要显示的 Panel 名。</param>
    private void MultiplePanelUpdate(DockPanel? dockPanel = null)
    {
        ProcessingPanel.Visibility = Visibility.Collapsed;
        FinishPanel.Visibility = Visibility.Collapsed;

        if (dockPanel == null) return;
        dockPanel.Visibility = Visibility.Visible;
        if (dockPanel == FinishPanel) OutDirButton.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// 精细替换选择单个文件。
    /// </summary>
    private void MultipleFileOpenButton_Click(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        var tbName = string.Concat(button.Name, "S");
        if (FindName(tbName) is not TextBlock textBlock) return;
        try
        {
            if (App.MultipleReplaceTask == null)
            {
                App.MultipleReplaceTask = new MultipleReplace();
                MultiplePanelUpdate();
            }
            var multipleFile = new OpenFileDialog { Filter = "字体文件 (*.ttf,*.otf)|*.ttf;*.otf" };

            // 尝试创建字体文件实例
            if (multipleFile.ShowDialog() == false) return;
            var multipleFilePath = multipleFile.FileName;
            var font = new Font(multipleFilePath);
            App.MultipleReplaceTask.AddReplaceThread(font, button, textBlock);

            Run.IsEnabled = App.MultipleReplaceTask.MultipleFontCheck();
        }
        catch (Exception ex)
        {
            FontExtension.HandleFontException(ex, textBlock);
        }
    }

    /// <summary>
    /// 精细替换模式开始制作。
    /// </summary>
    private async void MultipleFileRunButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            MultiplePanelUpdate(ProcessingPanel);
            await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

            if (App.MultipleReplaceTask == null) return;
            if (!App.MultipleReplaceTask.MultipleFontCheck())
            {
                Run.IsEnabled = false;
                return;
            }

            await App.MultipleReplaceTask.TaskStartPropRep();
            await App.MultipleReplaceTask.TaskMergeFont();
        }
        catch (Exception ex)
        {
            MultiplePanelUpdate();
            MessageBox.Show(ex.Message, "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
            Run.IsEnabled = false;
            return;
        }

        App.MultipleReplaceTask.TaskFinishing();
        App.MultipleReplaceTask.InitInterface();
        Run.IsEnabled = false;
        App.MultipleOutputDirectory = App.MultipleReplaceTask.OutputDirPath;
        App.MultipleReplaceTask = null;
        MultiplePanelUpdate(FinishPanel);
    }
}