namespace ConvertVideo2GIF.Models
{
    public class AppSettings
    {
        public string WorkingDirectory { get; set; } = string.Empty;
    }

    public class AppSettingsRoot
    {
        public AppSettings AppSettings { get; set; } = new AppSettings();
    }
}
