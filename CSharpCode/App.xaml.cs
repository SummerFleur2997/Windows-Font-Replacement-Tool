using System;
using System.IO;
using System.Windows;
using Newtonsoft.Json;
using Windows_Font_Replacement_Tool.Framework;

namespace Windows_Font_Replacement_Tool;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    /// <inheritdoc cref="Framework.Config"/>
    public static Config Config { get; private set; } = new();

    /// <summary>
    /// 快速制作模式任务
    /// </summary>
    internal static SingleReplace? SingleReplaceTask { get; set; }

    /// <summary>
    /// 精细制作模式任务
    /// </summary>
    internal static MultipleReplace? MultipleReplaceTask { get; set; }

    internal static string? SingleOutputDirectory;
    internal static string? MultipleOutputDirectory;

    internal static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
    internal static readonly string ResourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
    internal static readonly string XmlsPath = Path.Combine(ResourcePath, "xmls");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        HashTab.Initialize();
        LoadConfig();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        SingleReplaceTask = null;
        MultipleReplaceTask = null;
    }

    /// <summary>
    /// 加载程序配置文件。
    /// </summary>
    private static void LoadConfig()
    {
        var configExists = File.Exists(ConfigPath);
        if (configExists)
        {
            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
            Config = config ?? new Config();
        }

        Config.Save();
    }
}