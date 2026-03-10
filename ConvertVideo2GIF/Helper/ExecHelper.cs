using ConvertVideo2GIF.Models;
using System.Diagnostics;

namespace ConvertVideo2GIF.Helper
{
    public static class ExecHelper
    {
        public static async Task FFmpegCommandExec(DirPathObj dirObj, string command)
        {
            if (string.IsNullOrEmpty(command)) return;

            if (command.IndexOf("ffmpeg") > -1)
            {
                command = command.Replace("ffmpeg", "").Trim();
            }

            using (Process ffmpegProcess = new Process())
            {
                ffmpegProcess.StartInfo.FileName = dirObj.ffmpegPath;
                ffmpegProcess.StartInfo.WorkingDirectory = dirObj.workingDir; //工作資料夾位址
                ffmpegProcess.StartInfo.Arguments = command; //執行命令

                ffmpegProcess.StartInfo.UseShellExecute = false;
                ffmpegProcess.StartInfo.CreateNoWindow = true;

                ffmpegProcess.Start();

                await ffmpegProcess.WaitForExitAsync();

                Console.WriteLine("ffmpeg command 完成！");
            }
        }

        public static void FFmpegDebugCommandExec(DirPathObj dirObj, string command)
        {
            if (string.IsNullOrEmpty(command)) return;

            if (command.IndexOf("ffmpeg") > -1)
            {
                command = command.Replace("ffmpeg", "").Trim();
            }

            using (Process ffmpegProcess = new Process())
            {
                ffmpegProcess.StartInfo.FileName = dirObj.ffmpegPath;
                ffmpegProcess.StartInfo.WorkingDirectory = dirObj.workingDir; //工作資料夾位址
                ffmpegProcess.StartInfo.Arguments = command; //執行命令

                ffmpegProcess.StartInfo.UseShellExecute = false;
                ffmpegProcess.StartInfo.RedirectStandardOutput = true;
                ffmpegProcess.StartInfo.RedirectStandardError = true;
                ffmpegProcess.StartInfo.CreateNoWindow = true;

                //擷取進度訊息
                ffmpegProcess.OutputDataReceived += (sender, e) =>
                {
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        // 處理進度訊息
                        Console.WriteLine($"FFmpeg Output: {e.Data}");
                    }
                };
                //擷取錯誤訊息
                ffmpegProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (!String.IsNullOrEmpty(e.Data))
                    {
                        // 處理錯誤訊息
                        Console.WriteLine($"FFmpeg debug: {e.Data}");
                    }
                };
                // 設置 Exited 事件處理程序
                ffmpegProcess.EnableRaisingEvents = true;
                ffmpegProcess.Exited += (sender, e) =>
                {
                    Console.WriteLine("ffmpeg debug 完成！");
                };

                //不知道為什麼，沒有錯誤訊息時，處理序就不會自己結束
                //所以 debug 模式下，如果沒有訊息就代表沒有錯誤，可以使用非 debug 模式執行了

                ffmpegProcess.Start();
                ffmpegProcess.BeginOutputReadLine(); // 非同步讀取輸出
                ffmpegProcess.BeginErrorReadLine();  // 非同步讀取錯誤輸出

                // 等待 ffmpeg 完成
                ffmpegProcess.WaitForExit();

                Console.WriteLine("ffmpeg 完成！");
            }
        }

        /// <summary>
        /// 驗證影片檔案的完整性
        /// </summary>
        /// <param name="dirObj">檔案路徑物件</param>
        /// <returns>是否為有效的影片檔案</returns>
        public static async Task<bool> ValidateVideoFile(DirPathObj dirObj)
        {
            try
            {
                // 使用 ffprobe 驗證影片檔案
                string probeCommand = $"ffprobe -v quiet -print_format json -show_format -show_streams \"{dirObj.inputPath}\"";

                using (Process ffprobeProcess = new Process())
                {
                    ffprobeProcess.StartInfo.FileName = dirObj.ffmpegPath.Replace("ffmpeg.exe", "ffprobe.exe");
                    ffprobeProcess.StartInfo.Arguments = probeCommand.Replace("ffprobe", "").Trim();
                    ffprobeProcess.StartInfo.UseShellExecute = false;
                    ffprobeProcess.StartInfo.RedirectStandardOutput = true;
                    ffprobeProcess.StartInfo.RedirectStandardError = true;
                    ffprobeProcess.StartInfo.CreateNoWindow = true;

                    ffprobeProcess.Start();

                    string output = await ffprobeProcess.StandardOutput.ReadToEndAsync();
                    string error = await ffprobeProcess.StandardError.ReadToEndAsync();

                    await ffprobeProcess.WaitForExitAsync();

                    // 如果 ffprobe 成功執行且有輸出，表示檔案有效
                    return ffprobeProcess.ExitCode == 0 && !string.IsNullOrWhiteSpace(output);
                }
            }
            catch (Exception)
            {
                // 如果 ffprobe 不存在，退回到基本檔案檢查
                return File.Exists(dirObj.inputPath) && new FileInfo(dirObj.inputPath).Length > 0;
            }
        }

        /// <summary>
        /// 分析 PSNR 日誌檔案並輸出統計資訊
        /// </summary>
        public static void AnalyzePSNRLog(string logFilePath)
        {
            if (!File.Exists(logFilePath))
            {
                Console.WriteLine("找不到 PSNR 日誌檔案!");
                return;
            }

            var lines = File.ReadAllLines(logFilePath);
            var psnrAvgValues = new List<double>();
            var psnrYValues = new List<double>();

            foreach (var line in lines)
            {
                // 解析 psnr_avg 值
                var avgMatch = System.Text.RegularExpressions.Regex.Match(line, @"psnr_avg:([\d.]+)");
                var yMatch = System.Text.RegularExpressions.Regex.Match(line, @"psnr_y:([\d.]+)");

                if (avgMatch.Success && yMatch.Success)
                {
                    psnrAvgValues.Add(double.Parse(avgMatch.Groups[1].Value));
                    psnrYValues.Add(double.Parse(yMatch.Groups[1].Value));
                }
            }

            if (psnrAvgValues.Count > 0)
            {
                Console.WriteLine("\n=== PSNR 分析結果 ===");
                Console.WriteLine($"總幀數: {psnrAvgValues.Count}");
                Console.WriteLine($"平均 PSNR: {psnrAvgValues.Average():F2} dB");
                Console.WriteLine($"最小 PSNR: {psnrAvgValues.Min():F2} dB");
                Console.WriteLine($"最大 PSNR: {psnrAvgValues.Max():F2} dB");
                Console.WriteLine($"亮度平均 PSNR: {psnrYValues.Average():F2} dB");

                // 品質評估
                double avgPsnr = psnrAvgValues.Average();
                string quality = avgPsnr > 40 ? "優秀" : avgPsnr > 30 ? "良好" : "可接受";
                Console.WriteLine($"\n整體品質評價: {quality}");
            }
        }

        /// <summary>
        /// 分析 SSIM 日誌檔案並輸出統計資訊
        /// </summary>
        public static void AnalyzeSSIMLog(string logFilePath)
        {
            if (!File.Exists(logFilePath))
            {
                Console.WriteLine("找不到 SSIM 日誌檔案!");
                return;
            }

            var lines = File.ReadAllLines(logFilePath);
            var ssimAllValues = new List<double>();
            var ssimYValues = new List<double>();
            var ssimUValues = new List<double>();
            var ssimVValues = new List<double>();

            foreach (var line in lines)
            {
                // 解析 SSIM 值
                // 格式: n:1 Y:0.95 U:0.97 V:0.96 All:0.96 (15.23)
                var yMatch = System.Text.RegularExpressions.Regex.Match(line, @"Y:([\d.]+)");
                var uMatch = System.Text.RegularExpressions.Regex.Match(line, @"U:([\d.]+)");
                var vMatch = System.Text.RegularExpressions.Regex.Match(line, @"V:([\d.]+)");
                var allMatch = System.Text.RegularExpressions.Regex.Match(line, @"All:([\d.]+)");

                if (allMatch.Success && yMatch.Success)
                {
                    ssimAllValues.Add(double.Parse(allMatch.Groups[1].Value));
                    ssimYValues.Add(double.Parse(yMatch.Groups[1].Value));

                    if (uMatch.Success)
                        ssimUValues.Add(double.Parse(uMatch.Groups[1].Value));
                    if (vMatch.Success)
                        ssimVValues.Add(double.Parse(vMatch.Groups[1].Value));
                }
            }

            if (ssimAllValues.Count > 0)
            {
                double avgSSIM = ssimAllValues.Average();
                double minSSIM = ssimAllValues.Min();
                double maxSSIM = ssimAllValues.Max();

                Console.WriteLine("\n=== SSIM 分析結果 ===");
                Console.WriteLine($"總幀數: {ssimAllValues.Count}");
                Console.WriteLine($"平均 SSIM: {avgSSIM:F4}");
                Console.WriteLine($"最小 SSIM: {minSSIM:F4}");
                Console.WriteLine($"最大 SSIM: {maxSSIM:F4}");
                Console.WriteLine($"亮度平均 SSIM (Y): {ssimYValues.Average():F4}");

                if (ssimUValues.Count > 0)
                    Console.WriteLine($"色度 U 平均 SSIM: {ssimUValues.Average():F4}");
                if (ssimVValues.Count > 0)
                    Console.WriteLine($"色度 V 平均 SSIM: {ssimVValues.Average():F4}");

                // 品質評估
                string quality = avgSSIM >= 0.98 ? "優秀" :
                                avgSSIM >= 0.95 ? "良好" :
                                avgSSIM >= 0.90 ? "可接受" : "較差";

                Console.WriteLine($"\n整體品質評價: {quality}");
                Console.WriteLine($"結構相似度: {avgSSIM * 100:F2}%");

                // 品質穩定性分析
                double variance = CalculateVariance(ssimAllValues);
                Console.WriteLine($"品質穩定性 (方差): {variance:F6}");
                if (variance < 0.0001)
                    Console.WriteLine("  - 品質非常穩定");
                else if (variance < 0.001)
                    Console.WriteLine("  - 品質穩定");
                else
                    Console.WriteLine("  - 品質波動較大");
            }
        }

        /// <summary>
        /// 計算方差
        /// </summary>
        private static double CalculateVariance(List<double> values)
        {
            if (values.Count == 0) return 0;

            double avg = values.Average();
            double sumOfSquares = values.Sum(val => Math.Pow(val - avg, 2));
            return sumOfSquares / values.Count;
        }
    }
}