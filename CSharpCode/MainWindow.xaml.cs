using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using Microsoft.Win32;
using Windows_Font_Replacement_Tool.Framework;

namespace Windows_Font_Replacement_Tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SingleReplace? SingleReplaceTask { get; set; }
        
        public MainWindow()
        {
            InitializeComponent();
            HashTab.Initialize();
        }

        /// <summary>
        /// 关闭程序
        /// </summary>
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// 最小化窗口
        /// </summary>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        /// <summary>
        /// 拖动标题栏更改窗口位置
        /// </summary>
        private void Border_MouseDown(object sender, RoutedEventArgs e)
        {
            DragMove();
        }
        
        /// <summary>
        /// 点按左侧标签按钮切换标签页
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
        /// 其他界面下，模拟标签页按钮点击
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
        /// 打开帮助文档
        /// </summary>
        private void DocumentButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string pdfPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Help.pdf");
        
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
        /// 控制切换“快速替换”标签页右下角的控件显示内容，增强交互性
        /// </summary>
        /// <param name="stackPanel">需要显示的 Panel名。</param>
        private void SinglePanelUpdate(StackPanel? stackPanel)
        {
            PreviewPanel1.Visibility = Visibility.Collapsed;
            ProcessingPanel1.Visibility = Visibility.Collapsed;
            FinishPanel1.Visibility = Visibility.Collapsed;
            
            if (stackPanel != null)
                stackPanel.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 快速替换选择单个文件
        /// </summary>
        private void SingleFileOpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog singleFile = new OpenFileDialog();
            singleFile.Filter = "字体文件 (*.ttf,*.otf)|*.ttf;*.otf";
            if (singleFile.ShowDialog() == false) return;
            string singleFilePath = singleFile.FileName;
            SingleReplaceTask = new SingleReplace(singleFilePath);
            Run1.IsEnabled = true;
        }

        /// <summary>
        /// 快速替换开始制作事件
        /// </summary>
        private async void SingleFileRunButton_Click(object sender, RoutedEventArgs e)
        {
            SinglePanelUpdate(ProcessingPanel1);
            await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);
            
            if (SingleReplaceTask != null)
                await SingleReplaceTask.SingleStartPropRep();
            SinglePanelUpdate(FinishPanel1);
        }
    }
}