using System;
using System.Windows;

namespace Windows_Font_Replacement_Tool.Sections;

public partial class WelcomeTab
{
    public event EventHandler<RoutedEventArgs>? ButtonClickRequested;

    public WelcomeTab() => InitializeComponent();

    /// <summary>
    /// 其他界面下，模拟标签页按钮点击。
    /// </summary>
    private void AltTabButton_Click(object sender, RoutedEventArgs e) => ButtonClickRequested?.Invoke(sender, e);
}