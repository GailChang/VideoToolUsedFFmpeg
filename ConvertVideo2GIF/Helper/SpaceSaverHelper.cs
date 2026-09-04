using ConvertVideo2GIF.Enums;
using ConvertVideo2GIF.Models;
using FFMpegCore.Arguments;
using System.Text.Json;

namespace ConvertVideo2GIF.Helper
{
    public class SpaceSaverHelper
    {
        private AppSettingsRoot Config { get => new ConfigLoadingHelper().ConfigGetter; }
        private int Bitrate => Config.Compress.Bitrate;

        /// <summary>
        /// 使用指定的壓縮方法壓縮影片
        /// </summary>
        /// <param name="inputFileName">原始檔名(不含副檔名)</param>
        /// <param name="method">壓縮方法</param>
        public string CompressVideo(string inputFileName, CompressMethod method)
        {
            string commandLine = method switch
            {
                CompressMethod.H264 => "-i \"{0}\" -map 0 -c:v libx264 -crf 18 -sn -preset slow -tune film -c:a copy -movflags +faststart \"{1}\"",
                CompressMethod.NVENC_H264 => "-y -hwaccel cuda -i \"{0}\" -map 0 -c:v h264_nvenc -sn -preset p6 -tune hq {2} "
                    + "-spatial_aq 1 -aq-strength 10 -temporal-aq 1 "
                    + "-rc-lookahead 32 -profile:v high -pix_fmt yuv420p "
                    + "-c:a copy -movflags +faststart \"{1}\"",
                CompressMethod.H265 => "-i \"{0}\" -map 0 -c:v libx265 -crf 24 -sn -preset slow -tag:v hvc1 -x265-params aq-mode=3 -c:a copy -movflags +faststart \"{1}\"",
                CompressMethod.AV1 => "-i \"{0}\" -map 0 -c:v libsvtav1 -crf 28 -sn -preset 6 -g 240 -c:a copy -movflags +faststart \"{1}\"",
                _ => ""
            };
            // 建立新的檔名，加上 compressed 後綴
            string compressedFileName = $"{Path.GetFileNameWithoutExtension(inputFileName)} - compressed by {method}";

            CompressVideo(commandLine, inputFileName, compressedFileName);
            return compressedFileName;
        }

        /// <summary>
        /// 使用自訂 FFmpeg 命令列壓縮影片
        /// </summary>
        /// <param name="commandLine">FFmpeg 命令列參數</param>
        /// <param name="inputFileName">原始檔名(包含副檔名)</param>
        public void CompressVideo(string commandLine, string inputFileName, string outputFileName)
        {
            var dirObj = new DirPathObj(inputFileName, $"{outputFileName}.mp4");

            // 讀取影片並確保輸出影片儲存的資料夾存在
            if (!Directory.Exists(dirObj.workingDir))
            {
                Directory.CreateDirectory(dirObj.workingDir);
            }

            // 避免檔名衝突
            dirObj.outFileName = FileNameHelper.ResolveFileNameConflict(dirObj);

            if (!File.Exists(dirObj.inputPath))
            {
                Console.WriteLine("影片壓縮失敗! 輸入檔案不存在");
                return;
            }

            // ffmpeg -i input.mp4 -map 0 -c:v libx264 -crf 18 -preset slow -tune film -c:a copy -movflags +faststart output_h264.mp4
            string command = string.Format(commandLine, dirObj.inputPath, dirObj.outputPath, "");

            if (commandLine.Contains("{2}"))
            {
                // 設定目標位元率 (例如: 5M)
                string targetBitrate = $"{Bitrate}M"; // 目標位元率 (Mbps)
                string maxrate = $"{Bitrate * 1.5}M"; // TARGET x 1.5
                string bufsize = $"{Bitrate * 3}M"; // TARGET x 3
                string ncommand = $"-rc vbr_hq -b:v {targetBitrate} -maxrate {maxrate} -bufsize {bufsize}";
                command = string.Format(commandLine, dirObj.inputPath, dirObj.outputPath, ncommand);
            }

            ExecHelper.FFmpegDebugCommandExec(dirObj, command);

            if (File.Exists(dirObj.outputPath))
                Console.WriteLine("影片壓縮完成！");
            else
                Console.WriteLine("影片壓縮失敗!");
        }
    }
}