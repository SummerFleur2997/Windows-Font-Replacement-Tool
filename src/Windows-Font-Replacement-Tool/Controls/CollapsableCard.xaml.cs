using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Windows_Font_Replacement_Tool.Controls;

public partial class CollapsableCard
{
    /// <summary>
    /// 卡片的基础高度，即折叠时的高度
    /// </summary>
    private const int BasicHeight = 40;

    /// <inheritdoc cref="Icon"/>
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(string), typeof(CollapsableCard),
            new PropertyMetadata("", OnIconChanged));

    /// <inheritdoc cref="Title"/>
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(CollapsableCard),
            new PropertyMetadata("", OnTitleChanged));

    /// <inheritdoc cref="IsCollapsed"/>
    public static readonly DependencyProperty IsCollapsedProperty =
        DependencyProperty.Register(nameof(IsCollapsed), typeof(bool), typeof(CollapsableCard),
            new PropertyMetadata(true, OnIsCollapsedChanged));

    /// <inheritdoc cref="Content"/>
    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(object), typeof(CollapsableCard),
            new PropertyMetadata(null, OnContentChanged));

    /// <summary>
    /// 折叠卡片的图标
    /// </summary>
    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// 折叠卡片的标题
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// 折叠卡片的内容是否折叠
    /// </summary>
    public bool IsCollapsed
    {
        get => (bool)GetValue(IsCollapsedProperty);
        set => SetValue(IsCollapsedProperty, value);
    }

    /// <summary>
    /// 折叠卡片的内容
    /// </summary>
    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public CollapsableCard()
    {
        InitializeComponent();
        Dispatcher.BeginInvoke(
            new Action(() => { UpdateSwapState(IsCollapsed); }),
            DispatcherPriority.Loaded);
    }

    /// <summary>
    /// 当折叠按钮按下时调用
    /// </summary>
    private void OnCollapseButtonClick(object sender, RoutedEventArgs e) =>
        // 切换折叠状态
        IsCollapsed = !IsCollapsed;

    private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CollapsableCard card && e.NewValue is string newIcon)
            card.CardIcon.Text = newIcon;
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CollapsableCard card && e.NewValue is string newTitle)
            card.CardName.Text = newTitle;
    }

    private static void OnIsCollapsedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CollapsableCard card && e.NewValue is bool state)
            card.UpdateSwapState(state);
    }

    private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CollapsableCard card)
            card.ContentPresenter.Content = e.NewValue;
    }

    /// <summary>
    /// 更新折叠状态
    /// </summary>
    private void UpdateSwapState(bool collapsed)
    {
        // 先获取总高度
        var height = GetContentHeight() + BasicHeight;
        // 然后使用动画改变高度与折叠按钮旋转角度
        AnimateHeight(Border, height);
        AnimateRotate(ExpandIcon, collapsed ? 180 : 0);
    }

    /// <summary>
    /// 获取内容高度
    /// </summary>
    private double GetContentHeight()
    {
        ContentPresenter.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var expandedHeight = ContentPresenter.DesiredSize.Height;

        return IsCollapsed ? 0 : expandedHeight;
    }

    /// <summary>
    /// 展开折叠动画，适用于内容
    /// </summary>
    private static void AnimateHeight(FrameworkElement element, double toHeight)
    {
        var anim = new DoubleAnimation
        {
            To = toHeight,
            Duration = TimeSpan.FromSeconds(0.25),
            AccelerationRatio = 0.4,
            DecelerationRatio = 0.05
        };
        element.BeginAnimation(HeightProperty, anim);
    }

    /// <summary>
    /// 旋转动画，适用于折叠按钮
    /// </summary>
    private static void AnimateRotate(UIElement element, double toAngle)
    {
        if (element.RenderTransform is not RotateTransform rt) return;
        var anim = new DoubleAnimation
        {
            To = toAngle,
            Duration = TimeSpan.FromSeconds(0.25),
            AccelerationRatio = 0.4,
            DecelerationRatio = 0.05
        };
        rt.BeginAnimation(RotateTransform.AngleProperty, anim);
    }
}