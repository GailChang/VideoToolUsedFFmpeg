using ConvertVideo2GIF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertVideo2GIF.Helper
{
    public static class WatermarkHelper
    {
        public static void GetScreenShot(string inputFileName)
        {
            var dirObj = new DirPathObj(inputFileName, $"{inputFileName}_screenshot", ".mp4", ".jpg");
            if (!File.Exists(dirObj.inputPath))
            {
                Console.WriteLine("輸入影片不存在，請確認檔案路徑是否正確！");
                return;
            }

            // 使用 FileNameHelper 避免檔名衝突
            FileNameHelper.ResolveFileNameConflict(dirObj);

            try
            {
                //截圖
                string command = $"-i \"{dirObj.inputPath}\" -ss 1 -f image2 -vframes 1 \"{dirObj.outputPath}\"";
                ExecHelper.FFmpegDebugCommandExec(dirObj, command);
                Console.WriteLine($"截圖完成 {DateTime.Now.ToString("yyyyMMdd HH:mm:ss")}");
                Console.WriteLine("請到 https://www.nuanque.com/ps/ 查找出水印的座標位置");
            }
            catch (Exception ex)
            {
                Console.WriteLine("截圖失敗: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 去除影片浮水印
        /// </summary>
        /// <param name="inputFileName"></param>
        public static void RemoveWatermark(string inputFileName, WatermarkInfoModel watermarkInfo)
        {
            var dirObj = new DirPathObj(inputFileName, $"{inputFileName}_no_watermark", ".mp4", ".mp4");

            if (!File.Exists(dirObj.inputPath))
            {
                Console.WriteLine("輸入影片不存在，請確認檔案路徑是否正確！");
                return;
            }

            // 使用 FileNameHelper 避免檔名衝突
            FileNameHelper.ResolveFileNameConflict(dirObj);

            try
            {
                string command = $"-i \"{dirObj.inputPath}\" -vf \"delogo=x={watermarkInfo.PositionX}:y={watermarkInfo.PositionY}:w={watermarkInfo.Width}:h={watermarkInfo.Height}:show=0\" -c:a copy \"{dirObj.outputPath}\"";
                ExecHelper.FFmpegDebugCommandExec(dirObj, command);
                Console.WriteLine($"去除浮水印 {DateTime.Now.ToString("yyyyMMdd HH:mm:ss")} 完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine("去除浮水印失敗: " + ex.Message);
                throw;
            }
        }
    }
}