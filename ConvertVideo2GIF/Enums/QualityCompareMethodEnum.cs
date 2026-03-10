using System.ComponentModel;

namespace ConvertVideo2GIF.Enums
{
    /// <summary>
    /// 影片畫質比較方法
    /// </summary>
    public enum QualityCompareMethod
    {
        /// <summary>
        /// PSNR - 峰值信噪比
        /// </summary>
        [Description("PSNR (峰值信噪比)")]
        PSNR = 0,

        /// <summary>
        /// SSIM - 結構相似性指數
        /// </summary>
        [Description("SSIM (結構相似性指數)")]
        SSIM = 1,

        /// <summary>
        /// VMAF - Video Multimethod Assessment Fusion (需要 FFmpeg 支援 libvmaf)
        /// </summary>
        [Description("VMAF (視覺品質評估)")]
        VMAF = 2,

        /// <summary>
        /// 全部方法 (PSNR + SSIM)
        /// </summary>
        [Description("全部 (PSNR + SSIM)")]
        All = 99
    }
}