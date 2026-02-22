using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;

namespace GIML
{
    public class ItemImageConverter : IValueConverter
    {
        private const string BaseImagePath = "ms-appx:///Assets/Items/";

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string itemKey && !string.IsNullOrWhiteSpace(itemKey))
            {
                string fileName = $"item-{itemKey}.png";
                string uriString = BaseImagePath + fileName;
                System.Diagnostics.Debug.WriteLine($"图片uri: {uriString}");
                try
                {
                    return new BitmapImage(new Uri(uriString));
                }
                catch(Exception ex)
                {
                    // URI 无效时返回 null（图片不显示）
                    System.Diagnostics.Debug.WriteLine($"BitmapImage转换错误: {ex}");
                    return null;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}