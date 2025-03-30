using System;
using System.IO;
using System.Threading.Tasks;

namespace Windows_Font_Replacement_Tool.Framework;

public class SingleReplace: ReplaceTask
{
    private static string CreateOutputDir(string taskName)
    {
        var outputDir = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + taskName;
        var outputDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output", outputDir);
        Directory.CreateDirectory(outputDirPath);
        return outputDirPath;
    }
    
    public SingleReplace(string customFont)
    {
        TaskName = Path.GetFileNameWithoutExtension(customFont);
        OutputDirPath = CreateOutputDir(TaskName);
        var index = 0;
        foreach (var sha1 in Sha2File.Keys)
        {
            ReplaceThreads[index] = new ReplaceThread(TaskName, customFont, sha1);
            index++;
        }
    }

    public async Task SingleStartPropRep()
    {
        ReplaceThread.CreateCacheDir();
        var maxDegree = Environment.ProcessorCount;

        await Task.Run(() => 
        {
            Parallel.ForEach(
                ReplaceThreads,
                new ParallelOptions { MaxDegreeOfParallelism = maxDegree },
                task => task.RunPropertyRep()
            );
        });
    }
}