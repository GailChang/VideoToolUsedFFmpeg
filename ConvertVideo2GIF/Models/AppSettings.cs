namespace ConvertVideo2GIF.Models
{
    public class AppSettings
    {
        public string WorkingDirectory { get; set; } = string.Empty;
    }

    public class Compress
    {
        public int Bitrate { get; set; }
    }

    public class AppSettingsRoot
    {
        public AppSettings AppSettings { get; set; } = new AppSettings();
        public Compress Compress { get; set; } = new Compress();
    }
}