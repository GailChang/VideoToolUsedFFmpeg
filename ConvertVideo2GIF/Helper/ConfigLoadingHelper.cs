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
    }
}