using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace Windows_Font_Replacement_Tool.Controls;

/// <summary>
/// 自定义任务盒子中的一行，用于存储并展示一个自定义字体替换任务的相关信息。
/// </summary>
public partial class CustomTaskRow
{
    /// <inheritdoc cref="IndexNum"/>
    public static readonly DependencyProperty IndexProperty =
        DependencyProperty.Register(nameof(IndexNum), typeof(int), typeof(CustomTaskRow), 
            new PropertyMetadata(0, OnIndexChanged));

    /// <inheritdoc cref="FontToRep"/>
    public static readonly DependencyProperty FontToRepProperty =
        DependencyProperty.Register(nameof(FontToRep), typeof(string), typeof(CustomTaskRow), 
            new PropertyMetadata("", OnFontToRepChanged));

    /// <inheritdoc cref="FontOfCus"/>
    public static readonly DependencyProperty FontOfCusProperty =
        DependencyProperty.Register(nameof(FontOfCus), typeof(string), typeof(CustomTaskRow), 
            new PropertyMetadata("", OnFontOfCusChanged));

    public CustomTaskRow()
    {
        InitializeComponent();
    }

    public CustomTaskRow(int index) : this()
    {
        IndexNum = index;
    }

    /// <summary>
    /// 自定义任务的索引值
    /// </summary>
    public int IndexNum
    {
        get => (int)GetValue(IndexProperty);
        set => SetValue(IndexProperty, value);
    }

    /// <summary>
    /// 待替换的字体
    /// </summary>
    public string FontToRep
    {
        get => (string)GetValue(FontToRepProperty);
        set => SetValue(FontToRepProperty, value);
    }

    /// <summary>
    /// 个性化字体
    /// </summary>
    public string FontOfCus
    {
        get => (string)GetValue(FontOfCusProperty);
        set => SetValue(FontOfCusProperty, value);
    }

    private static void OnIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CustomTaskRow row && e.NewValue is int value)
            row.Index.Text = value.ToString();
    }

    private static void OnFontToRepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CustomTaskRow row && e.NewValue is string value)
            row.FontToReplace.Content = value;
    }

    private static void OnFontOfCusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CustomTaskRow row && e.NewValue is string value)
            row.FontOfCustom.Content = value;
    }

    private void OnFontSelectButtonClick(object s, RoutedEventArgs e)
    {
        if (s is not Button button) return;

        // 打开个性化字体文件，然后更改按钮标题
        var originalFont = new OpenFileDialog { Filter = "字体文件 (*.ttf,*.otf)|*.ttf;*.otf" };
        if (originalFont.ShowDialog() == false) return;

        var originalFontPath = Path.GetFileNameWithoutExtension(originalFont.FileName);
        button.Content = originalFontPath;
    }
}