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
            if (!File.Exists(dirObj.inputPath))
            {
                Console.WriteLine("輸入影片不存在，請確認檔案路徑是否正確！");
                return;
            }

            // 避免檔名衝突
            dirObj.ResolveFileNameConflict();

            try
            {
                string command = $"-i \"{dirObj.inputPath}\" -c:v libx264 -preset medium -crf 23 -c:a aac -b:a 192k \"{dirObj.outputPath}\"";

                ExecHelper.FFmpegDebugCommandExec(dirObj, command);
                Console.WriteLine("影片轉換成 MP4 完成！");
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

            if (!File.Exists(dirObj.inputPath))
            {
                Console.WriteLine("輸入影片不存在，請確認檔案路徑是否正確！");
                return;
            }

            // 避免檔名衝突
            dirObj.ResolveFileNameConflict();

            //-vf scale 重新定義尺寸；-r 幀率
            string command = $"-i \"{dirObj.inputPath}\" -r 10 \"{dirObj.outputPath}\"";

            await ExecHelper.FFmpegCommandExec(dirObj, command);

            Console.WriteLine("影片轉換成 GIF 完成！");
        }

        /// <summary>
        /// 轉換編碼格式為 H264。
        /// 同樣是 MP4，但轉成 H264 編碼格式，此格式對於大部分播放器的相容性較好。
        /// </summary>
        /// <param name="inputFileName"></param>
        /// <returns></returns>
        public static void ConvertVCodeH264(string inputFileName)
        {
            DirPathObj dirObj = new DirPathObj(inputFileName, $"{Path.GetFileNameWithoutExtension(inputFileName)} - output.mp4");

            if (!File.Exists(dirObj.inputPath))
            {
                Console.WriteLine("輸入影片不存在，請確認檔案路徑是否正確！");
                return;
            }

            // 使用 FileNameHelper 避免檔名衝突
            dirObj.ResolveFileNameConflict();

            try
            {
                string command = $"-i \"{dirObj.inputPath}\" -c:v libx264 -crf 18 -maxrate 3.75M -bufsize 7.5M -c:a copy \"{dirObj.outputPath}\"";
                ExecHelper.FFmpegDebugCommandExec(dirObj, command);
                Console.WriteLine($"影片轉換成 H264 編碼 {DateTime.Now.ToString("yyyyMMdd HH:mm:ss")} 完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine("轉換 MP4 編碼 H264 失敗: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// webm 轉成 mp4 檔案格式
        /// </summary>
        /// <param name="inputFileName"></param>
        /// <returns></returns>
        public static async Task ConvertWEBM2MP4(string inputFileName)
        {
            DirPathObj dirObj = new DirPathObj(inputFileName, ".webm", ".mp4");

            if (!File.Exists(dirObj.inputPath))
            {
                Console.WriteLine("輸入影片不存在，請確認檔案路徑是否正確！");
                return;
            }

            // 使用 FileNameHelper 避免檔名衝突
            dirObj.ResolveFileNameConflict();

            //使用 h264_nvenc 編碼，品質 -cq 23
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

            if (!File.Exists(dirObj.inputPath))
            {
                Console.WriteLine("輸入影片不存在，請確認檔案路徑是否正確！");
                return;
            }

            // 使用 FileNameHelper 避免檔名衝突
            dirObj.ResolveFileNameConflict();

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