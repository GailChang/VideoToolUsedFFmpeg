using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertVideo2GIF.Models
{
    public class WatermarkInfoModel
    {
        /// <summary>
        /// 水印的 X 位置
        /// </summary>
        public int PositionX { get; set; }

        /// <summary>
        /// 水印的 Y 位置
        /// </summary>
        public int PositionY { get; set; }

        /// <summary>
        /// 水印的寬度
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 水印的高度
        /// </summary>
        public int Height { get; set; }
    }
}