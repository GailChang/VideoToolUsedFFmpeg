using ConvertVideo2GIF.Models;
using System.Diagnostics;

namespace ConvertVideo2GIF.Helper
{
    public static class ConvertFileHelper
    {
        /// <summary>
        /// 轉成 MP4 檔案格式
        /// </summary>
        /// <param name="inputFileName">原始檔名(不含副檔名)</param>
        /// <returns></returns>
        public static async Task ConvertMOV2MP4(string inputFileName)
        {
            DirPathObj dirObj = new DirPathObj(inputFileName, ".mov", ".mp4");
            // 讀取影片並確保輸出 GIF 儲存的資料夾存在
            if (!Directory.Exists(dirObj.workingDir))
            {
                Directory.CreateDirectory(dirObj.workingDir);
            }
            if (!File.Exists(dirObj.inputPath))
            {
                Console.WriteLine("輸入影片不存在，請確認檔案路徑是否正確！");
                return;
            }
            //如果有重複的檔案，則會自動加上 (1)、(2) 等等的後綴
            bool noConflict = true;
            string currentName = dirObj.outFileName;
            for (int i = 1; noConflict; i++)
            {
                if (!File.Exists(dirObj.workingDir + currentName + dirObj.outputFormat))
                {
                    noConflict = false;
                    break;
                }
                string suffix = "(" + i.ToString() + ")";
                currentName = dirObj.outFileName + suffix;
            }
            dirObj.outFileName = currentName;
            try
            {
                // 使用 FFmpeg 調用進行轉換
                using (Process ffmpegProcess = new Process())
                {
                    ffmpegProcess.StartInfo.FileName = dirObj.ffmpegPath;
                    //-vf scale 重新定義尺寸；-r 幀率
                    ffmpegProcess.StartInfo.Arguments = $"-i \"{dirObj.inputPath}\" -c:v libx264 -preset medium -crf 23 -c:a aac -b:a 192k \"{dirObj.outputPath}\"";
                    ffmpegProcess.StartInfo.UseShellExecute = false;
                    ffmpegProcess.StartInfo.RedirectStandardOutput = true;
                    ffmpegProcess.StartInfo.RedirectStandardError = true;
                    ffmpegProcess.StartInfo.CreateNoWindow = true;
                    ffmpegProcess.Start();

                    //string output = await ffmpegProcess.StandardOutput.ReadToEndAsync();
                    //string error = await ffmpegProcess.StandardError.ReadToEndAsync();
                    await ffmpegProcess.WaitForExitAsync();

                    Console.WriteLine("影片轉換成 MP4 完成！");
                    //Console.WriteLine($"標準輸出: {output}");
                    //Console.WriteLine($"標準錯誤: {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("轉換 MP4 失敗: " + ex.Message);
                return;
            }
        }

        /// <summary>
        /// 轉成 GIF 檔案格式
        /// </summary>
        /// <param name="inputFileName">原始檔名(不含副檔名)</param>
        public static async Task ConvertGIF(string inputFileName)
        {
            DirPathObj dirObj = new DirPathObj(inputFileName, ".mp4", ".gif");

            // 讀取影片並確保輸出 GIF 儲存的資料夾存在
            if (!Directory.Exists(dirObj.workingDir))
            {
                Directory.CreateDirectory(dirObj.workingDir);
            }

            if (!File.Exists(dirObj.inputPath))
            {
                Console.WriteLine("輸入影片不存在，請確認檔案路徑是否正確！");
                return;
            }

            // 使用 FileNameHelper 避免檔名衝突
            FileNameHelper.ResolveFileNameConflict(dirObj);

            //-vf scale 重新定義尺寸；-r 幀率
            string command = $"-i \"{dirObj.inputPath}\" -r 10 \"{dirObj.outputPath}\"";

            await ExecHelper.FFmpegCommandExec(dirObj, command);

            Console.WriteLine("影片轉換成 GIF 完成！");
        }

        /// <summary>
        /// webm 轉成 mp4 檔案格式
        /// </summary>
        /// <param name="inputFileName"></param>
        /// <returns></returns>
        public static async Task ConvertWEBM2MP4(string inputFileName)
        {
            DirPathObj dirObj = new DirPathObj(inputFileName, ".webm", ".mp4");

            // 讀取影片並確保輸出 MP4 儲存的資料夾存在
            if (!Directory.Exists(dirObj.workingDir))
            {
                Directory.CreateDirectory(dirObj.workingDir);
            }

            if (!File.Exists(dirObj.inputPath))
            {
                Console.WriteLine("輸入影片不存在，請確認檔案路徑是否正確！");
                return;
            }

            // 使用 FileNameHelper 避免檔名衝突
            FileNameHelper.ResolveFileNameConflict(dirObj);

            //-vf scale 重新定義尺寸；-r 幀率
            string command = $"-i \"{dirObj.inputPath}\" -c:v h264_nvenc -preset p4 -rc vbr -cq 23 -c:a aac -b:a 192k \"{dirObj.outputPath}\"";

            ExecHelper.FFmpegDebugCommandExec(dirObj, command);

            Console.WriteLine("影片轉換成 MP4 完成！");
        }

        /// <summary>
        /// 從 mkv 影片中提取字幕
        /// </summary>
        /// <param name="inputFileName"></param>
        /// <param name="subtitleIndex">字幕 index 值</param>
        /// <param name="useTagChinese">是否使用 tag>language 為中文的字幕</param>
        /// <param name="outputFileName">輸出檔名(不含副檔名)</param>
        /// <returns></returns>
        public static void ExtractSubtitles(string inputFileName, string subtitleIndex, bool? useTagChinese = false, string? outputFileName = null)
        {
            useTagChinese ??= false;
            if (Path.GetExtension(inputFileName).ToLower() != ".mkv")
            {
                Console.WriteLine("輸入檔案不是 mkv 格式，無法提取字幕！");
                return;
            }

            string prefix = Path.GetFileNameWithoutExtension(inputFileName);
            DirPathObj dirObj = new DirPathObj(
                prefix,
                outputFileName ?? (useTagChinese.Value ? prefix + "-chinese.srt" : prefix + "-sub.srt"),
                ".mkv",
                ".srt");

            // 讀取影片並確保輸出 SRT 儲存的資料夾存在
            if (!Directory.Exists(dirObj.workingDir))
            {
                Directory.CreateDirectory(dirObj.workingDir);
            }

            if (!File.Exists(dirObj.inputPath))
            {
                Console.WriteLine("輸入影片不存在，請確認檔案路徑是否正確！");
                return;
            }

            // 使用 FileNameHelper 避免檔名衝突
            FileNameHelper.ResolveFileNameConflict(dirObj);

            // 提取字幕
            // -map 0:s:0 表示選擇第一個字幕流
            // -map 0:7 表示選擇第 7 個字幕流(準確說是 index 的值)
            // -map 0:s:m:language:chi 表示選擇 tag>language 為中文的字幕，但是若有複數個中文字幕，會報錯
            string mapCommand = useTagChinese.Value ? "-map 0:s:m:language:chi" : $"-map 0:{subtitleIndex}";

            string command = $"-i \"{dirObj.inputPath}\" {mapCommand} \"{dirObj.outputPath}\"";
            ExecHelper.FFmpegDebugCommandExec(dirObj, command);
            Console.WriteLine("字幕提取完成！");
        }
    }
}