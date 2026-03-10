using ConvertVideo2GIF.Models;
using System.Diagnostics;

namespace ConvertVideo2GIF.Helper
{
    public static class ConvertFileHelper
    {
        /// <summary>
        /// 轉成 MP4 檔案格式
        /// </summary>
        /// <param name="oFileName">原始檔名(不含副檔名)</param>
        /// <returns></returns>
        public static async Task ConvertMOV2MP4(string oFileName)
        {
            DirPathObj dirObj = new DirPathObj(oFileName, ".mov", ".mp4");
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
        /// <param name="oFileName">原始檔名(不含副檔名)</param>
        public static async Task ConvertGIF(string oFileName)
        {
            DirPathObj dirObj = new DirPathObj(oFileName, ".mp4", ".gif");

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
    }
}