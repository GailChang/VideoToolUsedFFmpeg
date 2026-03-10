using System.ComponentModel;

namespace ConvertVideo2GIF.Extensions
{
    /// <summary>
    /// 列舉擴充
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// 取得Description
        /// </summary>
        /// <param name="value">列舉數值</param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            if (field == null)
            {
                return value.ToString();
            }

            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                return attribute.Description;
            }

            return value.ToString();
        }
    }
}