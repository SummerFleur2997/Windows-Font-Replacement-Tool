using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using ICSharpCode.SharpZipLib.Zip;

namespace Windows_Font_Replacement_Tool.Framework
{
    public static class HashTab
    {
        private static readonly string LibsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libs");
        private static readonly string XmlsPath = Path.Combine(LibsPath, "xmls");
        
        private static readonly string LibSha = "2f7349a1ec9dc23c1385d91505b67df010505ce4";

        public static void Initialize()
        {
            try
            {
                if (!Directory.Exists(XmlsPath))
                {
                    ValidateCoreFiles();
                }
                ValidateXmlFiles();
            }
            catch (Exception ex)
            {
                HandleError("文件校验失败，请尝试重新下载并安装本工具", ex);
            }
        }

        private static void ValidateCoreFiles()
        {
            var dataLibsPath = Path.Combine(LibsPath, "xmlLibs");
            
            if (!File.Exists(dataLibsPath) || ComputeSha1(dataLibsPath) != LibSha)
                throw new InvalidOperationException("xmlLibs 文件损坏");
            
            DecodeLibs();
        }

        private static void DecodeLibs()
        {
            var dataLibsPath = Path.Combine(LibsPath, "xmlLibs");
            var archivePath = Path.Combine(LibsPath, "archive");
            
            var base64Data = File.ReadAllText(dataLibsPath);
            var zipBytes = Convert.FromBase64String(base64Data);
            File.WriteAllBytes(archivePath, zipBytes);

            using (var zipStream = new ZipInputStream(File.OpenRead(archivePath)))
            {
                ZipEntry entry;
                while ((entry = zipStream.GetNextEntry()) != null)
                {
                    var entryPath = Path.Combine(LibsPath, entry.Name);
                    var directoryPath = Path.GetDirectoryName(entryPath);

                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    
                    if (!entry.IsDirectory)
                    {
                        using (var fileStream = File.Create(entryPath))
                            zipStream.CopyTo(fileStream);
                    }
                }
            }
            
            File.Delete(archivePath);
        }

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

        private static string ComputeSha1(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            using var sha1 = SHA1.Create();
            var hashBytes = sha1.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        private static void HandleError(string message, Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine("错误详情：");
            sb.AppendLine(message);
            sb.AppendLine();
            sb.AppendLine(ex.ToString());

            MessageBox.Show(sb.ToString(), "严重错误", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);

            Application.Current.Shutdown();
        }
    }
}