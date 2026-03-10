using System.Text.Json;

namespace ConvertVideo2GIF.Models
{
    public class DirPathObj
    {
        private static string? _cachedWorkingDir = null;

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

        private static string LoadWorkingDirectoryFromConfig()
        {
            if (_cachedWorkingDir != null)
                return _cachedWorkingDir;

            try
            {
                string configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

                if (!File.Exists(configPath))
                {
                    Console.WriteLine("警告: appsettings.json 不存在，使用預設工作目錄");
                    _cachedWorkingDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads") + Path.DirectorySeparatorChar;
                    return _cachedWorkingDir;
                }

                string jsonString = File.ReadAllText(configPath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var config = JsonSerializer.Deserialize<AppSettingsRoot>(jsonString, options);

                if (config?.AppSettings?.WorkingDirectory != null && !string.IsNullOrWhiteSpace(config.AppSettings.WorkingDirectory))
                {
                    _cachedWorkingDir = config.AppSettings.WorkingDirectory;
                }
                else
                {
                    Console.WriteLine("警告: appsettings.json 中未設定 WorkingDirectory，使用預設工作目錄");
                    _cachedWorkingDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads") + Path.DirectorySeparatorChar;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"讀取設定檔失敗: {ex.Message}，使用預設工作目錄");
                _cachedWorkingDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads") + Path.DirectorySeparatorChar;
            }

            return _cachedWorkingDir;
        }

        private string workingDirIn = LoadWorkingDirectoryFromConfig();

        // get the ffmpeg.exe path in the current directory
        private string ffmpegPathIn = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "ffmpeg.exe");

        public string workingDir { get => workingDirIn; }
        public string ffmpegPath { get => ffmpegPathIn; }
        public string inFileName { get; set; }
        public string outFileName { get; set; }

        public string inputFormat { get; set; }
        public string outputFormat { get; set; }
        public string inputPath { get => workingDir + inFileName + inputFormat; }
        public string outputPath { get => workingDir + outFileName + outputFormat; }
    }
}