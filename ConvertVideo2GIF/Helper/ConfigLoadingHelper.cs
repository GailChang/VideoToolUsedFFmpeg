using ConvertVideo2GIF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConvertVideo2GIF.Helper
{
    public class ConfigLoadingHelper
    {
        public AppSettingsRoot ConfigGetter { get => LoadConfig(); }

        public AppSettingsRoot LoadConfig()
        {
            string configPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

            if (!File.Exists(configPath))
            {
                Console.WriteLine("警告: appsettings.json 不存在。請確認根目錄中含有必要的設定文件");
                return new AppSettingsRoot();
            }

            string jsonString = File.ReadAllText(configPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var config = JsonSerializer.Deserialize<AppSettingsRoot>(jsonString, options);

            if (config == null)
            {
                Console.WriteLine("警告: 無法解析 appsettings.json。請確認文件格式正確。");
                return new AppSettingsRoot();
            }

            return config;
        }

        /// <summary>
        /// 讀取 AppSettings 設定
        /// </summary>
        /// <returns>AppSettings 物件</returns>
        public AppSettings LoadAppSettings()
        {
            var root = LoadConfig();
            var appSettings = root.AppSettings;
            if (string.IsNullOrWhiteSpace(appSettings.WorkingDirectory))
            {
                Console.WriteLine("警告: WorkingDirectory 未設定。程式將使用預設工作目錄: \"使用者/Downloads/\"");
                // 使用預設值
                appSettings.WorkingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads") + Path.DirectorySeparatorChar;
            }

            return appSettings;
        }
    }
}