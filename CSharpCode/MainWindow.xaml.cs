using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using Windows_Font_Replacement_Tool.Framework;

namespace Windows_Font_Replacement_Tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            HashTab.Initialize();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void Border_MouseDown(object sender, RoutedEventArgs e)
        {
            DragMove();
        }
        
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
    }
}