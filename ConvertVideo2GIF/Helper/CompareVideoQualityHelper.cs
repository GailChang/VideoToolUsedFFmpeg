using ConvertVideo2GIF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertVideo2GIF.Helper
{
    public class CompareVideoQualityHelper
    {
        /// <summary>
        /// 使用 PSNR 比較影片畫質
        /// </summary>
        public static void ComparePSNR(DirPathObj originalObj, DirPathObj compressedObj)
        {
            string logFile = Path.Combine(originalObj.workingDir, "psnr_log.txt");

            // 刪除舊的日誌檔案
            if (File.Exists(logFile))
            {
                File.Delete(logFile);
            }

            // 將路徑中的反斜線轉換為正斜線,並處理特殊字元
            string escapedLogFile = logFile.Replace("\\", "/").Replace(":", "\\:");

            // FFmpeg PSNR 命令 - 使用正確的濾鏡格式
            string command = $"-i \"{compressedObj.inputPath}\" -i \"{originalObj.inputPath}\" -lavfi \"psnr=stats_file='{escapedLogFile}'\" -f null -";
            ExecHelper.FFmpegDebugCommandExec(originalObj, command);

            Console.WriteLine($"PSNR 日誌已保存至: {logFile}");
        }

        /// <summary>
        /// 使用 SSIM 比較影片畫質
        /// </summary>
        public static void CompareSSIM(DirPathObj originalObj, DirPathObj compressedObj)
        {
            string logFile = Path.Combine(originalObj.workingDir, "ssim_log.txt");

            // 刪除舊的日誌檔案
            if (File.Exists(logFile))
            {
                File.Delete(logFile);
            }

            // 將路徑中的反斜線轉換為正斜線,並處理特殊字元
            string escapedLogFile = logFile.Replace("\\", "/").Replace(":", "\\:");

            // FFmpeg SSIM 命令 - 使用正確的濾鏡格式
            string command = $"-i \"{compressedObj.inputPath}\" -i \"{originalObj.inputPath}\" -lavfi \"ssim=stats_file='{escapedLogFile}'\" -f null -";
            ExecHelper.FFmpegDebugCommandExec(originalObj, command);
            Console.WriteLine($"SSIM 日誌已保存至: {logFile}");
        }

        /// <summary>
        /// 使用 VMAF 比較影片畫質 (需要 FFmpeg 支援 libvmaf)
        /// </summary>
        public static void CompareVMAF(DirPathObj originalObj, DirPathObj compressedObj)
        {
            string logFile = Path.Combine(originalObj.workingDir, "vmaf_log.json");

            // 刪除舊的日誌檔案
            if (File.Exists(logFile))
            {
                File.Delete(logFile);
            }

            // 將路徑中的反斜線轉換為正斜線,並處理特殊字元
            string escapedLogFile = logFile.Replace("\\", "/").Replace(":", "\\:");

            // FFmpeg VMAF 命令 - 使用正確的濾鏡格式
            string command = $"-i \"{compressedObj.inputPath}\" -i \"{originalObj.inputPath}\" -lavfi \"libvmaf\" -f null -";
            ExecHelper.FFmpegDebugCommandExec(originalObj, command);
        }
    }
}