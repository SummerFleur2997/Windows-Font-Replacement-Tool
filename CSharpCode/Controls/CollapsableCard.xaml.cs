using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Windows_Font_Replacement_Tool.Controls;

public partial class CollapsableCard
{
    private const int BasicHeight = 40;

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(string), typeof(CollapsableCard), 
            new PropertyMetadata("", OnIconChanged));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(CollapsableCard), 
            new PropertyMetadata("", OnTitleChanged));

    public static readonly DependencyProperty ExtraHeightProperty =
        DependencyProperty.Register(nameof(ExtraHeight), typeof(double), typeof(CollapsableCard), 
            new PropertyMetadata(0.0, OnExtraHeightChanged));

    public static readonly DependencyProperty IsCollapsedProperty =
        DependencyProperty.Register(nameof(IsCollapsed), typeof(bool), typeof(CollapsableCard), 
            new PropertyMetadata(true, OnIsCollapsedChanged));

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public double ExtraHeight
    {
        get => (double)GetValue(ExtraHeightProperty);
        set => SetValue(ExtraHeightProperty, value);
    }

    public bool IsCollapsed
    {
        get => (bool)GetValue(IsCollapsedProperty);
        set => SetValue(IsCollapsedProperty, value);
    }

    public CollapsableCard()
    {
        InitializeComponent();
    }

    private void OnCollapseButtonClick(object sender, RoutedEventArgs e)
    {
        IsCollapsed = !IsCollapsed;
    }

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

    private static void OnExtraHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CollapsableCard card && e.NewValue is double newHeight)
            card.ExtraHeight = newHeight;
    }

    private static void OnIsCollapsedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CollapsableCard card && e.NewValue is bool state)
            card.UpdateSwapState(state);
    }

    private void UpdateSwapState(bool collapsed)
    {
        AnimateHeight(Border, collapsed ? BasicHeight : BasicHeight + ExtraHeight); 
        AnimateRotate(ExpandIcon, collapsed ? 180 : 0);
    }

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