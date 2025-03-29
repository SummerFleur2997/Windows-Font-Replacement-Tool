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
        private static readonly string LibsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
        public static readonly string XmlsPath = Path.Combine(LibsPath, "xmls");
        
        private const string LibSha = "2f7349a1ec9dc23c1385d91505b67df010505ce4";

        public static void Initialize()
        {
            try
            {
                if (!Directory.Exists(XmlsPath))
                {
                    ValidateLibFiles();
                }
                ValidateXmlFiles();
            }
            catch (Exception ex)
            {
                HandleError("文件校验失败，请尝试重新下载并安装本工具", ex);
            }
        }

        private static void ValidateLibFiles()
        {
            var dataLibsPath = Path.Combine(LibsPath, "xmlLibs");
            
            if (!File.Exists(dataLibsPath) || ComputeSha1(dataLibsPath) != LibSha)
                throw new Exception("xmlLibs 文件损坏");
            
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
            var warning = new StringBuilder();
            warning.AppendLine(message);
            warning.AppendLine("错误信息：" + ex.Message);

            MessageBox.Show(warning.ToString(), "错误", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);

            Application.Current.Shutdown();
        }
    }
}