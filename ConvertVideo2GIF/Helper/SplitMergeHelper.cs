using ConvertVideo2GIF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertVideo2GIF.Helper
{
    /// <summary>
    /// 剪輯、切割、合併影片 helper
    /// </summary>
    public static class SplitMergeHelper
    {
        /// <summary>
        /// 剪輯影片
        /// </summary>
        /// <param name="iFileName">原始檔名(包含副檔名)</param>
        /// <param name="oFileName">輸出檔名</param>
        /// <param name="startTime">開始時間</param>
        /// <param name="endTime">結束時間</param>
        public static async Task CutVideo(string iFileName, string oFileName, string startTime, string endTime)
        {
            DirPathObj dirObj = new DirPathObj(iFileName, oFileName + Path.GetExtension(iFileName));
            if (string.IsNullOrEmpty(oFileName)) dirObj.outFileName = $"{dirObj.inFileName} - output";

            if (!File.Exists(dirObj.inputPath))
            {
                Console.WriteLine("找不到來源影片，影片剪輯失敗!");
                return;
            }

            // 避免檔名衝突
            dirObj.ResolveFileNameConflict();

            // 現在的語法，結尾可以使用精確到毫秒的時間點，但開頭不行
            string command = $"-ss {startTime} -to {endTime} -i \"{dirObj.inputPath}\" -c copy -avoid_negative_ts 1 \"{dirObj.outputPath}\"";

            await ExecHelper.FFmpegCommandExec(dirObj, command);

            if (File.Exists(dirObj.outputPath))
                Console.WriteLine("影片剪輯完成！");
            else
                Console.WriteLine("影片剪輯失敗!");
        }

        /// <summary>
        /// 合併多個影片檔案(需為 h264 編碼的影片)
        /// </summary>
        /// <param name="files">想合併的檔案清單</param>
        /// <param name="oFileName">輸出檔名</param>
        public static async Task MergeVideo(List<string> files, string oFileName)
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
        /// 合併音訊和影片
        /// </summary>
        /// <param name="audioFileIncludeExt">音訊檔案名稱</param>
        /// <param name="videoFileIncludeExt">影片檔案名稱</param>
        /// <param name="oFileName">輸出檔案名稱(不含副檔名)</param>
        public static void CombineAudioAndVideo(string audioFileIncludeExt, string videoFileIncludeExt, string oFileName = "")
        {
            // 設定輸出檔案名稱
            if (string.IsNullOrEmpty(oFileName))
            {
                oFileName = $"{videoFileIncludeExt} - combine";
            }
            var outputDir = new DirPathObj(videoFileIncludeExt, oFileName + Path.GetExtension(videoFileIncludeExt));

            // 檢查輸入檔案是否存在
            if (!File.Exists(outputDir.inputPath))
            {
                Console.WriteLine($"影片檔案不存在: {outputDir.inputPath}");
                return;
            }

            var audioPath = Path.Combine(outputDir.workingDir, audioFileIncludeExt);
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