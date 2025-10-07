using System;
using System.Diagnostics;

namespace FontReader;

public static class Debug
{
    public static void Main()
    {
        var stopwatch = Stopwatch.StartNew();
        var font = new Font(@"F:\Fonts\Aa十九封情书.ttf");
        var name = font.FontFamily();
        var cjkCount = font.GetCjkCharacterCount();
        stopwatch.Stop();
        Console.WriteLine($"字体解析完成，字体名称为 {name}，共有 {cjkCount} 个汉字字符。");
        Console.WriteLine($"总耗时: {stopwatch.Elapsed.TotalMilliseconds:F4} 毫秒");
    }
}