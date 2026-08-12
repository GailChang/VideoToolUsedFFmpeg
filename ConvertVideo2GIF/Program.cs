using ConvertVideo2GIF.Enums;
using ConvertVideo2GIF.Extensions;
using ConvertVideo2GIF.Helper;
using ConvertVideo2GIF.Models;

namespace ConvertVideo2GIF
{
    internal class Program
    {
        /// <summary>
        /// 主程式入口點
        /// </summary>
        /// <param name="args"></param>
        private static async Task Main(string[] args)
        {
            // 設置控制台編碼為 UTF-8，支援日文、中文等 Unicode 字符
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            // Check the path of ffmpeg.exe exists
            DirPathObj dirObj = new DirPathObj("ffmpeg", ".exe", ".exe");
            if (!File.Exists(dirObj.ffmpegPath))
            {
                Console.WriteLine("ffmpeg.exe not found in the Resources folder. Please check the folder and make sure you unzip the ffmpeg.zip.");
                return;
            }
            // Check the working directory exists
            Console.WriteLine($"當前 Working directory {dirObj.workingDir} {Environment.NewLine}");

            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") + "程式執行開始!");

            var isContinue = true;
            while (isContinue)
            {
                isContinue = await OptionPage1();
            }
        }

        private static async Task<Boolean> OptionPage1()
        {
            Console.WriteLine("=== 影片處理工具 ===");
            Console.WriteLine("1. 剪輯影片 (CutVideo)");
            Console.WriteLine("2. 合併兩段影片 (MergeVideo)");
            Console.WriteLine("3. 提取字幕 (ExtractSubtitles)");
            Console.WriteLine("4. 去除浮水印 (RemoveWatermark)");
            Console.WriteLine("5. 壓縮轉檔影片 (ConvertVideo, CompressVideo)");
            Console.WriteLine("6. 更多功能(下一頁)");
            Console.WriteLine("0. 離開");
            Console.WriteLine("==================");
            Console.Write("請選擇功能 (輸入數字): ");

            string choice = Console.ReadLine() ?? "";
            Console.WriteLine();

            return await SwitchPageOption(1, choice);
        }

        private static async Task<Boolean> OptionPage2()
        {
            Console.WriteLine("=== 影片處理工具 ===");
            Console.WriteLine("1. 合併音訊和影片 (CombineAudioAndVideo)");
            Console.WriteLine("6. (上一頁)");
            Console.WriteLine("0. 離開");
            Console.WriteLine("==================");
            Console.Write("請選擇功能 (輸入數字): ");

            string choice = Console.ReadLine() ?? "";
            Console.WriteLine();

            return await SwitchPageOption(2, choice);
        }

        private static async Task<Boolean> SwitchPageOption(int page, string optionNumber)
        {
            bool isContinue = true;
            switch ((page, optionNumber))
            {
                case (1, "1"):
                    await HandleCutVideo();
                    break;

                case (1, "2"):
                    await HandleMergeVideo();
                    break;

                case (1, "3"):
                    HandleGetSubtitles();
                    break;

                case (1, "4"):
                    HandleRemoveWatermark();
                    break;

                case (1, "5"):
                    HandleCompressOrConvertVideo();
                    break;

                case (1, "6"):
                    await OptionPage2();
                    break;

                case (2, "6"):
                    await OptionPage1();
                    break;

                case (2, "1"):
                    HandleCombineAudioAndVideo();
                    break;

                case (1, "0"):
                case (2, "0"):
                    Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") + " 程式執行結束!");
                    isContinue = false;
                    break;

                default:
                    Console.WriteLine("無效的選項，請重新選擇！");
                    break;
            }
            ;
            return isContinue;
        }

        private static async Task HandleCutVideo()
        {
            Console.Write("請輸入影片檔名(包含副檔名): ");
            string inputFile = Console.ReadLine() ?? "";

            Console.Write("請輸入輸出檔名(可留空): ");
            string outputFile = Console.ReadLine() ?? "";

            Console.Write("請輸入開始時間(格式: HH:MM:SS，預設00:00:00): ");
            string? startTime = Console.ReadLine();

            Console.Write("請輸入結束時間(格式: HH:MM:SS): ");
            string? endTime = Console.ReadLine();

            await SplitMergeHelper.CutVideo(inputFile, outputFile, string.IsNullOrEmpty(startTime) ? "00:00:00" : startTime, string.IsNullOrEmpty(endTime) ? "00:00:00" : endTime);
        }

        private static async Task HandleMergeVideo()
        {
            Console.Write("請輸入輸出檔名: ");
            string outputFile = Console.ReadLine() ?? "";

            List<string> files = new List<string>();
            Console.WriteLine("請依序輸入要合併的影片檔名(不含副檔名)，輸入空白行結束:");

            while (true)
            {
                Console.Write($"影片 {files.Count + 1}: ");
                string fileName = Console.ReadLine() ?? "";

                if (string.IsNullOrWhiteSpace(fileName))
                    break;

                files.Add(fileName);
            }

            if (files.Count > 0)
            {
                await SplitMergeHelper.MergeVideo(files, outputFile);
            }
            else
            {
                Console.WriteLine("未輸入任何檔案！");
            }
        }

        private static async Task HandleAdjustVolume()
        {
            Console.Write("請輸入影片檔名(不含副檔名): ");
            string inputFile = Console.ReadLine() ?? "";

            Console.Write("請輸入音量百分比(例如: 150): ");
            string volumeInput = Console.ReadLine() ?? "";

            Console.Write("請輸入輸出檔名(可留空): ");
            string outputFile = Console.ReadLine() ?? "";

            if (int.TryParse(volumeInput, out int volumePercent))
            {
                await AdjustVolumePercentage(inputFile, volumePercent, outputFile);
            }
            else
            {
                Console.WriteLine("音量百分比必須是數字！");
            }
        }

        /// <summary>
        /// 處理合併音訊和影片所需參數
        /// </summary>
        private static void HandleCombineAudioAndVideo()
        {
            Console.Write("請輸入音訊檔案名稱(含副檔名): ");
            string audioFile = Console.ReadLine() ?? "";

            Console.Write("請輸入影片檔名(含副檔名): ");
            string videoFile = Console.ReadLine() ?? "";

            Console.Write("請輸入輸出檔名(可留空): ");
            string outputFile = Console.ReadLine() ?? "";

            SplitMergeHelper.CombineAudioAndVideo(audioFile, videoFile, outputFile);
        }

        /// <summary>
        /// 處理壓縮檔案或轉檔
        /// </summary>
        private static void HandleCompressOrConvertVideo()
        {
            Console.Write("請輸入影片檔名(含副檔名): ");
            string inputFile = Console.ReadLine() ?? "";
            Console.WriteLine("請選擇壓縮方法，依跨平臺支援度排名 ↓");
            Console.Write("(1: H264 | 2: NVENC_H264 | 3: AV1 | 4: H265 | 5: VP9 | 0: 不壓縮只轉檔): ");
            string methodInput = Console.ReadLine() ?? "";

            // 如果使用者輸入 0，則只進行轉檔，不壓縮
            if (int.TryParse(methodInput, out int methodNumber) && methodNumber == 0)
            {
                ConvertFileHelper.ConvertVCodeH264(inputFile);
                return;
            }

            if (Enum.TryParse(methodInput, out CompressMethod method))
            {
                SpaceSaverHelper spaceSaver = new SpaceSaverHelper();
                string outputFile = spaceSaver.CompressVideo(inputFile, method);

                // 提取字幕
                Console.Write("是否提取字幕 (y/n): ");
                string extractSubtitlesInput = Console.ReadLine() ?? "";
                if (extractSubtitlesInput.Equals("y", StringComparison.OrdinalIgnoreCase))
                {
                    ConvertFileHelper.ExtractSubtitles(inputFile, "7", false, outputFile);
                }
            }
            else
            {
                Console.WriteLine("無效的壓縮方法！");
            }
        }

        /// <summary>
        /// 取得 mkv 檔案中的字幕檔
        /// </summary>
        private static void HandleGetSubtitles()
        {
            Console.Write("請輸入影片檔名(含副檔名): ");
            string inputFile = Console.ReadLine() ?? "";

            Console.Write("請輸入字幕 index (例如: 0, 1, 2，留空表示用預設第1軌字幕): ");
            string methodInput = Console.ReadLine() ?? "";

            Console.Write("是否使用中文標籤找尋字幕軌 (y/n，預設 n): ");
            string extractSubtitlesInput = Console.ReadLine() ?? "";

            ConvertFileHelper.ExtractSubtitles(inputFile, methodInput, extractSubtitlesInput.Equals("y", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 去除浮水印(1. 先截圖 2.根據位置去除浮水印)
        /// </summary>
        private static void HandleRemoveWatermark()
        {
            Console.Write("請輸入影片檔名(不含副檔名): ");
            string inputFile = Console.ReadLine() ?? "";

            WatermarkHelper.GetScreenShot(inputFile);

            string watermarkPosition = "";
            string watermarkSize = "";
            int retryTime = 0;

            if (string.IsNullOrWhiteSpace(watermarkPosition) || string.IsNullOrWhiteSpace(watermarkSize))
            {
                if (retryTime > 0)
                {
                    Console.WriteLine("浮水印位置或大小輸入不正確，請輸入正確的值！");
                }

                Console.Write("請輸入浮水印位置 (x,y) 例如: 10,20: ");
                watermarkPosition = Console.ReadLine() ?? "";
                Console.Write("請輸入浮水印寬度 (w)、高度(h) 例如: 100,200: ");
                watermarkSize = Console.ReadLine() ?? "";
                retryTime++;

                if (retryTime > 10)
                {
                    Console.WriteLine("浮水印位置或大小輸入不正確，已達最大重試次數，程式將結束！");
                    return;
                }
            }

            if (watermarkPosition.Split(',').Length != 2 || watermarkSize.Split(',').Length != 2)
            {
                Console.WriteLine("浮水印位置或大小格式不正確，程式將結束！");
                return;
            }

            WatermarkInfoModel watermarkInfo = new()
            {
                PositionX = int.Parse(watermarkPosition.Split(',')[0]),
                PositionY = int.Parse(watermarkPosition.Split(',')[1]),
                Width = int.Parse(watermarkSize.Split(',')[0]),
                Height = int.Parse(watermarkSize.Split(',')[1])
            };

            WatermarkHelper.RemoveWatermark(inputFile, watermarkInfo);
        }

        /// <summary>
        /// 將影片音量調整到正常水平
        /// </summary>
        /// <param name="iFileName"></param>
        /// <returns></returns>
        private static void NormalVolumn(string iFileName)
        {
            string oFileName = iFileName + " - adjusted";
            DirPathObj dirObj = new DirPathObj(iFileName, oFileName, ".mp4", ".mp4");

            // 確保輸出資料夾存在
            if (!Directory.Exists(dirObj.workingDir))
            {
                Directory.CreateDirectory(dirObj.workingDir);
            }

            // 使用 FileNameHelper 避免檔名衝突
            dirObj.outFileName = FileNameHelper.ResolveFileNameConflict(dirObj);

            if (!File.Exists(dirObj.inputPath))
            {
                Console.WriteLine("影片不存在!");
                return;
            }

            // 使用 volume 濾鏡調整音量
            // -i input.mp4 -af loudnorm output.mp4
            string command = $"-i \"{dirObj.inputPath}\" -af loudnorm \"{dirObj.outputPath}\"";

            ExecHelper.FFmpegDebugCommandExec(dirObj, command);

            if (File.Exists(dirObj.outputPath))
                Console.WriteLine($"音量調整完成！");
            else
                Console.WriteLine("音量調整失敗!");
        }

        /// <summary>
        /// 將影片音量調整到指定百分比
        /// </summary>
        /// <param name="iFileName">輸入檔名(不含副檔名)</param>
        /// <param name="volumePercent">音量百分比 (例如: 100 表示 100%, 150 表示 150%, 50 表示 50%)</param>
        /// <param name="oFileName">輸出檔名(可選，預設為輸入檔名 + " - volume{百分比}")</param>
        /// <returns></returns>
        private static async Task AdjustVolumePercentage(string iFileName, int volumePercent, string oFileName = "")
        {
            if (string.IsNullOrEmpty(oFileName))
                oFileName = iFileName + $" - volume{volumePercent}";

            DirPathObj dirObj = new DirPathObj(iFileName, oFileName, ".mp4", ".mp4");

            // 確保輸出資料夾存在
            if (!Directory.Exists(dirObj.workingDir))
            {
                Directory.CreateDirectory(dirObj.workingDir);
            }

            // 使用 FileNameHelper 避免檔名衝突
            dirObj.outFileName = FileNameHelper.ResolveFileNameConflict(dirObj);

            if (!File.Exists(dirObj.inputPath))
            {
                Console.WriteLine("影片不存在!");
                return;
            }

            // 將百分比轉換為 FFmpeg 的 volume 值 (例如: 100% = 1.0, 150% = 1.5, 50% = 0.5)
            double volumeValue = volumePercent / 100.0;

            // 使用 volume 濾鏡設定音量
            // volume=1.0 表示 100% 原始音量, volume=1.5 表示 150%, volume=0.5 表示 50%
            string command = $"-i \"{dirObj.inputPath}\" -af \"volume={volumeValue.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)}\" -c:v copy \"{dirObj.outputPath}\"";

            Console.WriteLine($"正在調整音量至 {volumePercent}% (volume={volumeValue})...");

            await ExecHelper.FFmpegCommandExec(dirObj, command);

            if (File.Exists(dirObj.outputPath))
                Console.WriteLine($"音量調整為 {volumePercent}% 完成！");
            else
                Console.WriteLine("音量調整失敗!");
        }

        /// <summary>
        /// 比較兩部影片的畫質差距 - 總入口方法
        /// </summary>
        /// <param name="originalFileName">原始影片檔名(不含副檔名)</param>
        /// <param name="compressedFileName">壓縮後影片檔名(不含副檔名)</param>
        /// <param name="compareMethod">比較方法</param>
        /// <returns></returns>
        private static void CompareVideoQuality(string originalFileName, string compressedFileName, QualityCompareMethod compareMethod = QualityCompareMethod.All)
        {
            DirPathObj originalObj = new DirPathObj(originalFileName, ".mp4", ".mp4");
            DirPathObj compressedObj = new DirPathObj(compressedFileName, ".mp4", ".mp4");

            // 確認兩個影片檔案都存在
            if (!File.Exists(originalObj.inputPath))
            {
                Console.WriteLine($"原始影片不存在: {originalObj.inputPath}");
                return;
            }

            if (!File.Exists(compressedObj.inputPath))
            {
                Console.WriteLine($"壓縮後影片不存在: {compressedObj.inputPath}");
                return;
            }

            Console.WriteLine("=== 開始比較影片畫質 ===");
            Console.WriteLine($"原始影片: {originalFileName}");
            Console.WriteLine($"比較影片: {compressedFileName}");
            Console.WriteLine($"比較方法: {compareMethod.GetDescription()}");
            Console.WriteLine();

            // 根據選擇的方法進行比較
            string logFile = string.Empty;
            switch (compareMethod)
            {
                case QualityCompareMethod.PSNR:
                    Console.WriteLine("正在進行 PSNR 分析...");
                    CompareVideoQualityHelper.ComparePSNR(originalObj, compressedObj);
                    logFile = Path.Combine(originalObj.workingDir, "psnr_log.txt");
                    ExecHelper.AnalyzePSNRLog(logFile);
                    break;

                case QualityCompareMethod.SSIM:
                    Console.WriteLine("正在進行 SSIM 分析...");
                    CompareVideoQualityHelper.CompareSSIM(originalObj, compressedObj);
                    logFile = Path.Combine(originalObj.workingDir, "ssim_log.txt");
                    ExecHelper.AnalyzeSSIMLog(logFile);
                    break;

                case QualityCompareMethod.VMAF:
                    Console.WriteLine("正在進行 VMAF 分析...");
                    CompareVideoQualityHelper.CompareVMAF(originalObj, compressedObj);
                    break;

                case QualityCompareMethod.All:
                    Console.WriteLine("正在進行 PSNR 分析...");
                    CompareVideoQualityHelper.ComparePSNR(originalObj, compressedObj);
                    logFile = Path.Combine(originalObj.workingDir, "psnr_log.txt");
                    ExecHelper.AnalyzePSNRLog(logFile);

                    Console.WriteLine("正在進行 SSIM 分析...");
                    CompareVideoQualityHelper.CompareSSIM(originalObj, compressedObj);
                    logFile = Path.Combine(originalObj.workingDir, "ssim_log.txt");
                    ExecHelper.AnalyzeSSIMLog(logFile);
                    break;

                default:
                    Console.WriteLine("不支援的比較方法！");
                    return;
            }

            Console.WriteLine();
            Console.WriteLine("=== 畫質比較完成 ===");
        }
    }
}