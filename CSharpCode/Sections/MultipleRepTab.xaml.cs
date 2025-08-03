using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
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
        Run.IsEnabled = App.MultipleReplaceTask.AddReplaceThread(multipleFilePath, button, textBlock);
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