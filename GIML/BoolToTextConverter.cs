using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace GIML
{
    public class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            System.Diagnostics.Debug.WriteLine($"BoolToTextConverter: value={value}, type={value?.GetType()}");
            if (value is bool b)
            {
                string[] texts = (parameter as string)?.Split('|');
                if (texts != null && texts.Length == 2)
                    return b ? texts[0] : texts[1];
                else
                    return b ? "True" : "False";
            }
            return "False";
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}