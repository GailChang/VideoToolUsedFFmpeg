using ConvertVideo2GIF.Helper;
using System.Text.Json;

namespace ConvertVideo2GIF.Models
{
    public class DirPathObj
    {
        private static string? _cachedWorkingDir = null;

        /// <summary>
        /// 初始化 DirPathObj，會自動從完整的檔名(包含副檔名)中解析出檔名與副檔名
        /// </summary>
        /// <param name="inFileNameIncludeExt">輸入檔名(包含副檔名)</param>
        /// <param name="outFileNameIncludeExt">輸出檔名(包含副檔名)</param>
        public DirPathObj(string inFileNameIncludeExt, string outFileNameIncludeExt)
        {
            this.inFileName = Path.GetFileNameWithoutExtension(inFileNameIncludeExt);
            this.outFileName = Path.GetFileNameWithoutExtension(outFileNameIncludeExt);
            this.inputFormat = Path.GetExtension(inFileNameIncludeExt);
            this.outputFormat = Path.GetExtension(outFileNameIncludeExt);
        }

        public DirPathObj(string fileName, string inputFormat, string outputFormat)
        {
            inFileName = fileName;
            outFileName = fileName;
            this.inputFormat = inputFormat;
            this.outputFormat = outputFormat;
        }

        public DirPathObj(string inFileName, string outFileName, string inputFormat, string outputFormat)
        {
            this.inFileName = inFileName;
            this.outFileName = outFileName;
            this.inputFormat = inputFormat;
            this.outputFormat = outputFormat;
        }

        /// <summary>
        /// 解決檔名衝突，調用並更新輸出檔名
        /// </summary>
        public void ResolveFileNameConflict()
        {
            this.outFileName = FileNameHelper.ResolveFileNameConflict(this);
        }

        private static string LoadWorkingDirectoryFromConfig()
        {
            if (_cachedWorkingDir != null)
                return _cachedWorkingDir;

            var loading = new ConfigLoadingHelper();
            var appSettings = loading.LoadAppSettings();
            _cachedWorkingDir = appSettings.WorkingDirectory;

            return _cachedWorkingDir;
        }

        private string workingDirIn = LoadWorkingDirectoryFromConfig();

        // get the ffmpeg.exe path in the current directory
        private string ffmpegPathIn = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "ffmpeg.exe");

        public string workingDir { get => workingDirIn; }
        public string ffmpegPath { get => ffmpegPathIn; }

        /// <summary>
        /// 輸入檔名(不含副檔名)
        /// </summary>
        public string inFileName { get; set; }

        /// <summary>
        /// 輸出檔名(不含副檔名)
        /// </summary>
        public string outFileName { get; set; }

        /// <summary>
        /// 輸入檔案格式(副檔名)，例如 .mp4、.avi、.mov
        /// </summary>
        public string inputFormat { get; set; }

        /// <summary>
        /// 輸出檔案格式(副檔名)，例如 .gif、.mp4
        /// </summary>
        public string outputFormat { get; set; }

        /// <summary>
        /// 完整的輸入檔案路徑，包含工作目錄、檔名與副檔名
        /// </summary>
        public string inputPath { get => workingDir + inFileName + inputFormat; }

        /// <summary>
        /// 完整的輸出檔案路徑，包含工作目錄、檔名與副檔名
        /// </summary>
        public string outputPath { get => workingDir + outFileName + outputFormat; }
    }
}