using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using ICSharpCode.SharpZipLib.Zip;

namespace Windows_Font_Replacement_Tool.Framework;

/// <summary>
/// 用于进行文件解压与哈希校验的类。
/// </summary>
internal static class ResourceHelper
{
    /// <summary>
    /// 在主程序运行前进行初始化校验。
    /// </summary>
    public static void Initialize()
    {
        try
        {
            // xmls 目录不存在的情况下验证 Libs 文件完整性
            if (!Directory.Exists(App.XmlsPath)) DecodeLibs();
            // xmls 目录存在的情况下验证每个 xml 文件完整性
            ValidateXmlFiles();
        }
        catch (Exception ex)
        {
            HandleError("文件校验失败，请尝试重新下载并安装本工具", ex);
        }
    }

    /// <summary>
    /// 从嵌入至程序集的数据中读取 xmlData
    /// </summary>
    private static void DecodeLibs()
    {
        var uri = new Uri("pack://application:,,,/Resources/xmlData");
        var stream = Application.GetResourceStream(uri);
        if (stream == null) throw new Exception("未能解析 xmlLibs 文件");

        var reader = new StreamReader(stream.Stream);
        var base64 = reader.ReadToEnd();
        var bytes = Convert.FromBase64String(base64);

        var archivePath = Path.Combine(App.ResourcePath, "archive");
        File.WriteAllBytes(archivePath, bytes);

        using (var zipStream = new ZipInputStream(File.OpenRead(archivePath)))
        {
            ZipEntry entry;
            while ((entry = zipStream.GetNextEntry()) != null)
            {
                var entryPath = Path.Combine(App.ResourcePath, entry.Name);
                var directoryPath = Path.GetDirectoryName(entryPath);

                if (!Directory.Exists(directoryPath) && directoryPath != null)
                    Directory.CreateDirectory(directoryPath);

                if (entry.IsDirectory) continue;
                using var fileStream = File.Create(entryPath);
                zipStream.CopyTo(fileStream);
            }
        }

        File.Delete(archivePath);
    }

    /// <summary>
    /// 验证 xml文件完整性。
    /// </summary>
    private static void ValidateXmlFiles()
    {
        var xmlFiles = Directory.GetFiles(App.XmlsPath);
        // 若 xml 目录内的文件数量为 19，则依次校验文件
        if (xmlFiles.Length == 19)
        {
            foreach (var xmlFile in xmlFiles)
                if (ComputeSha1(xmlFile) != Path.GetFileName(xmlFile))
                    break;
            return;
        }

        // 若 xml 目录内的文件数量不为 19，则可能存在异常，删除目录并重新解包
        Directory.Delete(App.XmlsPath, true);
        DecodeLibs();
    }

    /// <summary>
    /// 功能函数，用于返回文件 Sha1值。
    /// </summary>
    /// <param name="filePath">需要进行校验的文件绝对路径。</param>
    /// <returns>给定文件的 Sha1值，字母均为小写字母。</returns>
    private static string ComputeSha1(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var sha1 = SHA1.Create();
        var hashBytes = sha1.ComputeHash(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// 用于提示报错信息弹窗。
    /// </summary>
    /// <param name="message">错误提示</param>
    /// <param name="ex">错误实例</param>
    private static void HandleError(string message, Exception ex)
    {
        var warning = new StringBuilder();
        warning.AppendLine(message);
        warning.AppendLine("错误信息：" + ex.Message);

        MessageBox.Show(warning.ToString(), "错误",
            MessageBoxButton.OK, MessageBoxImage.Error);

        Application.Current.Shutdown();
    }
}