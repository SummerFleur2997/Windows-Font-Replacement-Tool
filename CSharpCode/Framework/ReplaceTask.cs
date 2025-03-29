using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace Windows_Font_Replacement_Tool.Framework;

public class ReplaceTask
{
    protected readonly Dictionary<string, string> Sha2File = new()
    {
        {"d46344fc3841184ac741685d53f0b01cd11865e7", "msyh01.ttf"},
        {"791e18622ff1011b9a6c68bfb8796258a3a1cf85", "msyh02.ttf"},
        {"ea87b3ddadc073d04b25039f9e41bfbefd8b0eba", "msyhbd01.ttf"},
        {"e49a970410fe1b22ac10bdc7c656117c74f22b39", "msyhbd02.ttf"},
        {"7d70dd165648425f19346947a268a8ee58262eb2", "msyhl01.ttf"},
        {"83eb72dd5f5285488ed2ca4d72810e266bc2fd4e", "msyhl02.ttf"},
        {"5213a5c8e131255d461ebc4e8b5ed71eaedf01dc", "segoeui.ttf"},
        {"c6adb891613704c322580732b92b6566a7e80684", "segoeuib.ttf"},
        {"f8808c6fcf9ce4a74a6682fa5d886fe25a60f11b", "segoeuii.ttf"},
        {"12144c399e57c776e7b3ed8f689cce09df5c1a53", "segoeuil.ttf"},
        {"dde72eaa7ba41cac65e21350b613b95f0c04bd4d", "segoeuisl.ttf"},
        {"8f4a51f221db950112948a897deacf6d45c10b1a", "segoeuiz.ttf"},
        {"b6379d63dd25e0c01c09dec61e56b4e2a4fc2456", "seguibl.ttf"},
        {"bb6f5d72997a1cd118f67dfc5335943ea600cd1b", "seguibli.ttf"},
        {"b6ba87aa742964e7103ff654516a017ddf6031e0", "seguili.ttf"},
        {"f80b4059b145408bc7948ff57aa2d326a08afa31", "seguisb.ttf"},
        {"27fa3fd2cf0ad25dc9d87e4892898e6813916719", "seguisbi.ttf"},
        {"cd0bed1303626bf6cfc7412206744c5bd794be1b", "seguisli.ttf"},
        {"4ebb195ad99add8fa11308749363a1599e29e0c7", "SegUIVar.ttf"}
    };
    private string TaskName { get; set; }
    private string TaskOutput { get; set; }
    private string TaskResource { get; set; }
    

    private static string CreateOutputDir(string taskName)
    {
        var outputDir = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + taskName;
        var outputDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output", outputDir);
        Directory.CreateDirectory(outputDirPath);
        return outputDirPath;
    }

    private static string OutputPath(string fileName, string taskName)
    {
        var outputDirPath = CreateOutputDir(taskName);
        return Path.Combine(outputDirPath, fileName);
    }

    protected int RunPropertyRep(string font, string xml)
    {
        try
        {
            // 1. 配置进程启动参数
            var startInfo = new ProcessStartInfo
            {
                FileName = "functions.exe",  // 如果未添加到环境变量，需指定完整路径
                Arguments = $"propertyRep \"{font}\" \"{xml}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            // 2. 启动进程并等待结束
            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();
                
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                
                process.WaitForExit(); // 等待程序退出
                
                Console.WriteLine("标准输出:\n" + output);
                if (!string.IsNullOrEmpty(error))
                    Console.WriteLine("错误输出:\n" + error);
                
                return process.ExitCode;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"执行失败: {ex.Message}");
            return -1;
        }
    }

    protected ReplaceTask(string sha1, string taskName)
    {
        TaskName = taskName;
        TaskOutput = OutputPath(TaskName, Sha2File[sha1]);
        TaskResource = Path.Combine(HashTab.XmlsPath, sha1);
    }
}