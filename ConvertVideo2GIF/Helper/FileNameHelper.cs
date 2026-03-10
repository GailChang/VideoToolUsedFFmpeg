using ConvertVideo2GIF.Models;

namespace ConvertVideo2GIF.Helper
{
    internal static class FileNameHelper
    {
        /// <summary>
        /// 避免檔名衝突，自動在檔名後面加上 (1)、(2) 等等的後綴
        /// </summary>
        /// <param name="dirObj">DirPathObj 物件，包含檔案路徑資訊</param>
        public static string ResolveFileNameConflict(DirPathObj dirObj)
        {
            bool noConflict = true;
            string currentName = dirObj.outFileName;
            string currentFullPath = dirObj.outputPath;

            // 避免檔名衝突
            for (int i = 1; noConflict; i++)
            {
                if (!File.Exists(currentFullPath))
                {
                    noConflict = false;
                    break;
                }
                string suffix = "(" + i.ToString() + ")";
                currentName = dirObj.outFileName + suffix;
                currentFullPath = dirObj.workingDir + currentName + dirObj.outputFormat;
            }
            return currentName;
        }
    }
}