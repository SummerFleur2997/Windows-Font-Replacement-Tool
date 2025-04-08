using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Security.Cryptography;
using ICSharpCode.SharpZipLib.Zip;

namespace Windows_Font_Replacement_Tool.Framework;

/// <summary>
/// 用于进行文件解压与校验的类。
/// </summary>
public static class HashTab
{
    public static readonly string ResourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
    public static readonly string XmlsPath = Path.Combine(ResourcePath, "xmls");
    
    private const string LibSha = "2f7349a1ec9dc23c1385d91505b67df010505ce4";

    /// <summary>
    /// 在主程序运行前进行初始化校验。
    /// </summary>
    public static void Initialize()
    {
        try
        {
            // xmls 目录不存在的情况下验证 Libs 文件完整性
            if (!Directory.Exists(XmlsPath))
            {
                ValidateLibFiles();     
            }
            // xmls 目录存在的情况下验证每个 xml 文件完整性
            ValidateXmlFiles();        
        }
        catch (Exception ex)
        {
            HandleError("文件校验失败，请尝试重新下载并安装本工具", ex);
        }
    }

    /// <summary>
    /// 验证 Resources/xmlLibs文件完整性。
    /// </summary>
    private static void ValidateLibFiles()
    {
        var dataLibsPath = Path.Combine(ResourcePath, "xmlLibs");
        // 验证失败，报错
        if (!File.Exists(dataLibsPath) || ComputeSha1(dataLibsPath) != LibSha)
            throw new Exception("xmlLibs 文件损坏");
        // 验证成功，解包
        DecodeLibs();
    }

    /// <summary>
    /// 解包 xmlLibs文件。
    /// </summary>
    private static void DecodeLibs()
    {
        var dataLibsPath = Path.Combine(ResourcePath, "xmlLibs");
        var archivePath = Path.Combine(ResourcePath, "archive");
        
        var base64Data = File.ReadAllText(dataLibsPath);
        var zipBytes = Convert.FromBase64String(base64Data);
        File.WriteAllBytes(archivePath, zipBytes);

        using (var zipStream = new ZipInputStream(File.OpenRead(archivePath)))
        {
            ZipEntry entry;
            while ((entry = zipStream.GetNextEntry()) != null)
            {
                var entryPath = Path.Combine(ResourcePath, entry.Name);
                var directoryPath = Path.GetDirectoryName(entryPath);

                if (!Directory.Exists(directoryPath) && directoryPath != null)
                {
                    Directory.CreateDirectory(directoryPath);
                }
                
                if (!entry.IsDirectory)
                {
                    using var fileStream = File.Create(entryPath);
                    zipStream.CopyTo(fileStream);
                }
            }
        }
        
        File.Delete(archivePath);
    }

    /// <summary>
    /// 验证 xml文件完整性。
    /// </summary>
    private static void ValidateXmlFiles()
    {
        var xmlFiles = Directory.GetFiles(XmlsPath);

        if (xmlFiles.Length == 19)
        {
            foreach (var xmlFile in xmlFiles)
            {
                if (ComputeSha1(xmlFile) != Path.GetFileName(xmlFile))
                    break;
            }
            return;
        }
        Directory.Delete(XmlsPath, true);
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
    /// <param name="ex">错误信息</param>
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
