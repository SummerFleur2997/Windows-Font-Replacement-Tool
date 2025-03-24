using System;
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
            // DetectSystemIconFont();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void Border_MouseDown(object sender, RoutedEventArgs e)
        {
            DragMove();
        }
        
        private void TabButton_Click(object sender, RoutedEventArgs e)
        {
            Tab1Content.Visibility = Visibility.Collapsed;
            Tab2Content.Visibility = Visibility.Collapsed;
            Tab3Content.Visibility = Visibility.Collapsed;
            
            var btn = sender as Button;
            switch (btn?.Name)
            {
                case "Tab1Btn":
                    Tab1Content.Visibility = Visibility.Visible;
                    break;
                case "Tab2Btn":
                    Tab2Content.Visibility = Visibility.Visible;
                    break;
                case "Tab3Btn":
                    Tab3Content.Visibility = Visibility.Visible;
                    break;
            }
            
            foreach (var child in ((StackPanel)btn.Parent).Children)
            {
                if (child is Button b)
                {
                    b.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6750A4"));
                }
            }
            btn.Background = new SolidColorBrush(Colors.DarkViolet);
        }
    }
}