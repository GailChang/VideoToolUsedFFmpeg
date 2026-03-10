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

            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") + "程式執行開始!");

            while (true)
            {
                Console.WriteLine("=== 影片處理工具 ===");
                Console.WriteLine("1. 剪輯影片 (CutVideo)");
                Console.WriteLine("2. 合併影片 (MergeVideo)");
                Console.WriteLine("3. 調整音量 (AdjustVolume)");
                Console.WriteLine("4. 合併音訊和影片 (CombineAudioAndVideo)");
                Console.WriteLine("0. 離開");
                Console.WriteLine("==================");
                Console.Write("請選擇功能 (輸入數字): ");

                string choice = Console.ReadLine() ?? "";
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        await HandleCutVideo();
                        break;

                    case "2":
                        await HandleMergeVideo();
                        break;

                    case "3":
                        await HandleAdjustVolume();
                        break;

                    case "4":
                        HandleCombineAudioAndVideo();
                        break;

                    case "0":
                        Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") + "程式執行結束!");
                        return;

                    default:
                        Console.WriteLine("無效的選項，請重新選擇！");
                        break;
                }
            }
        }

        private static async Task HandleCutVideo()
        {
            Console.Write("請輸入影片檔名(不含副檔名): ");
            string inputFile = Console.ReadLine() ?? "";

            Console.Write("請輸入輸出檔名(可留空): ");
            string outputFile = Console.ReadLine() ?? "";

            Console.Write("請輸入開始時間(格式: HH:MM:SS，預設00:00:00): ");
            string startTime = Console.ReadLine() ?? "";

            Console.Write("請輸入結束時間(格式: HH:MM:SS): ");
            string endTime = Console.ReadLine() ?? "";

            await CutVideo(inputFile, outputFile, startTime, endTime);
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
                await MergeVideo(files, outputFile);
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

        private static void HandleCombineAudioAndVideo()
        {
            Console.Write("請輸入音訊檔案名稱(含副檔名): ");
            string audioFile = Console.ReadLine() ?? "";

            Console.Write("請輸入影片檔名(不含副檔名): ");
            string videoFile = Console.ReadLine() ?? "";

            Console.Write("請輸入輸出檔名(可留空): ");
            string outputFile = Console.ReadLine() ?? "";

            CombineAudioAndVideo(audioFile, videoFile, outputFile);
        }

        /// <summary>
        /// 剪輯影片
        /// </summary>
        /// <param name="iFileName">原始檔名(不含副檔名)</param>
        /// <param name="oFileName">輸出檔名</param>
        /// <param name="startTime">開始時間</param>
        /// <param name="endTime">結束時間</param>
        private static async Task CutVideo(string iFileName, string oFileName, string startTime = "00:00:00", string endTime = "00:00:00")
        {
            if (string.IsNullOrEmpty(oFileName)) oFileName = iFileName + " - output";
            DirPathObj dirObj = new DirPathObj(iFileName, oFileName, ".mp4", ".mp4");

            // 讀取影片並確保輸出 影片 儲存的資料夾存在
            if (!Directory.Exists(dirObj.workingDir))
            {
                Directory.CreateDirectory(dirObj.workingDir);
            }

            // 使用 FileNameHelper 避免檔名衝突
            dirObj.outFileName = FileNameHelper.ResolveFileNameConflict(dirObj);

            if (!File.Exists(dirObj.inputPath))
            {
                Console.WriteLine("找不到來源影片，影片剪輯失敗!");
                return;
            }

            // 現在的語法，結尾可以使用精確到毫秒的時間點，但開頭不行
            string command = $"-ss {startTime} -to {endTime} -i \"{dirObj.inputPath}\" -c copy -avoid_negative_ts 1 \"{dirObj.outputPath}\"";

            await ExecHelper.FFmpegCommandExec(dirObj, command);

            if (File.Exists(dirObj.outputPath))
                Console.WriteLine("影片剪輯完成！");
            else
                Console.WriteLine("影片剪輯失敗!");
        }

        /// <summary>
        /// 合併多個影片檔案
        /// </summary>
        /// <param name="files">想合併的檔案清單</param>
        /// <param name="oFileName">輸出檔名</param>
        private static async Task MergeVideo(List<string> files, string oFileName)
        {
            if (files == null || files.Count == 0)
            {
                Console.WriteLine("錯誤：沒有提供要合併的檔案清單");
                return;
            }

            List<DirPathObj> dirPathObjs = new List<DirPathObj>();
            List<string> mylist = new List<string>();
            string mylistPath = "mylist.txt";
            string mylistFullPath = "";

            // 先驗證每個輸入檔案並收集有效的檔案
            foreach (string file in files)
            {
                DirPathObj ndirObj = new DirPathObj(file, oFileName, ".mp4", ".mp4");

                if (!File.Exists(ndirObj.inputPath))
                {
                    Console.WriteLine($"警告：檔案不存在，跳過: {ndirObj.inputPath}");
                    continue;
                }

                // 驗證影片檔案完整性
                if (await ExecHelper.ValidateVideoFile(ndirObj))
                {
                    dirPathObjs.Add(ndirObj);
                    // 使用完整路徑避免路徑問題
                    mylist.Add($"file '{ndirObj.inputPath.Replace("\\", "/")}'");

                    if (mylistFullPath == "")
                    {
                        mylistFullPath = Path.Combine(ndirObj.workingDir, mylistPath);
                    }

                    Console.WriteLine($"已加入合併清單: {ndirObj.inFileName}{ndirObj.inputFormat}");
                }
                else
                {
                    Console.WriteLine($"警告：影片檔案驗證失敗，跳過: {ndirObj.inputPath}");
                }
            }

            if (dirPathObjs.Count == 0)
            {
                Console.WriteLine("錯誤：沒有有效的影片檔案可以合併");
                return;
            }

            if (dirPathObjs.Count == 1)
            {
                Console.WriteLine("只有一個有效檔案，將直接複製到輸出位置");
                var singleFile = dirPathObjs[0];
                try
                {
                    File.Copy(singleFile.inputPath, singleFile.outputPath, true);
                    Console.WriteLine("檔案複製完成！");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"檔案複製失敗: {ex.Message}");
                }
                return;
            }

            string listStr = string.Join($"{Environment.NewLine}", mylist.ToArray());

            try
            {
                if (File.Exists(mylistFullPath))
                {
                    File.Delete(mylistFullPath);
                }
                // 使用不帶 BOM 的 UTF-8 編碼
                var utf8WithoutBom = new System.Text.UTF8Encoding(false);
                File.WriteAllText(mylistFullPath, listStr, utf8WithoutBom);
                Console.WriteLine($"已建立檔案清單: {mylistFullPath}");
                Console.WriteLine($"清單內容:\n{listStr}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("寫入檔案清單時發生錯誤：" + ex.Message);
                return;
            }

            if (!File.Exists(mylistFullPath))
            {
                Console.WriteLine("錯誤：無法建立檔案清單");
                return;
            }

            DirPathObj dirObj = dirPathObjs[0];
            // 使用輸出檔名建立正確的輸outputPath物件
            DirPathObj outputDirObj = new DirPathObj("", oFileName, ".mp4", ".mp4");

            // 讀取影片並確保輸出 影片 儲存的資料夾存在
            if (!Directory.Exists(outputDirObj.workingDir))
            {
                Directory.CreateDirectory(outputDirObj.workingDir);
            }

            // 使用 FileNameHelper 避免檔名衝突
            string resolvedFileName = FileNameHelper.ResolveFileNameConflict(outputDirObj);
            outputDirObj.outFileName = resolvedFileName;

            int count = 0;
            List<string> tempFileList = new List<string>();
            foreach (var item in files)
            {
                count++;
                string newCommand = $"ffmpeg -i \"{item}.mp4\" -c copy -bsf:v h264_mp4toannexb -f mpegts temp{count}.ts";
                var newDir = new DirPathObj(item, $"temp{count}", "mp4", "ts");
                ExecHelper.FFmpegDebugCommandExec(newDir, newCommand);
                tempFileList.Add($"temp{count}.ts");
            }

            string eachFile = string.Join("|", tempFileList);
            string command = $"ffmpeg -i \"concat:{eachFile}\" -c copy -bsf:a aac_adtstoasc \"{outputDirObj.outFileName}{outputDirObj.outputFormat}\"";

            ExecHelper.FFmpegDebugCommandExec(outputDirObj, command);
            Console.WriteLine($"{command} 完成");

            // 刪除臨時檔案
            foreach (var tempFile in tempFileList)
            {
                if (File.Exists($"{outputDirObj.workingDir}{tempFile}"))
                {
                    File.Delete($"{outputDirObj.workingDir}{tempFile}");
                }
            }
            if (File.Exists(mylistFullPath))
            {
                File.Delete(mylistFullPath);
            }
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

        /// <summary>
        /// 合併音訊和影片
        /// </summary>
        /// <param name="audioFile">音訊檔案名稱</param>
        /// <param name="videoFile">影片檔案名稱</param>
        /// <param name="oFileName">輸出檔案名稱</param>
        private static void CombineAudioAndVideo(string audioFile, string videoFile, string oFileName = "")
        {
            // 設定輸出檔案名稱
            if (string.IsNullOrEmpty(oFileName))
            {
                oFileName = $"{videoFile}_output";
            }
            var outputDir = new DirPathObj(videoFile, oFileName, ".mp4", ".mp4");

            // 檢查輸入檔案是否存在
            if (!File.Exists(outputDir.inputPath))
            {
                Console.WriteLine($"影片檔案不存在: {outputDir.inputPath}");
                return;
            }

            var audioPath = Path.Combine(outputDir.workingDir, audioFile);
            if (!File.Exists(audioPath))
            {
                Console.WriteLine($"音訊檔案不存在: {audioPath}");
                return;
            }

            // 使用 FileNameHelper 避免檔名衝突
            outputDir.outFileName = FileNameHelper.ResolveFileNameConflict(outputDir);

            // 使用FFmpeg合併音訊和影片
            string command = $"-i \"{outputDir.inputPath}\" -i \"{audioPath}\" -c:v copy -c:a aac -strict experimental \"{outputDir.outputPath}\"";
            ExecHelper.FFmpegDebugCommandExec(outputDir, command);

            Console.WriteLine($"合併完成，輸出檔案: {oFileName}");
        }
    }
}