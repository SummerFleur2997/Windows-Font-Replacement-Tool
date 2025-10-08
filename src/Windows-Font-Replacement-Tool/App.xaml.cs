using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows;
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

    private static readonly string BaseDir = AppDomain.CurrentDomain.BaseDirectory;
    internal static readonly string ConfigPath = Path.Combine(BaseDir, "config.json");
    internal static readonly string ResourcePath = Path.Combine(BaseDir, "Assets");
    internal static readonly string XmlsPath = Path.Combine(ResourcePath, "xmls");

    public static readonly JsonSerializerOptions DefaultOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
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
            var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(ConfigPath), DefaultOptions);
            Config = config ?? new Config();
        }

        Config.Save();
    }
}