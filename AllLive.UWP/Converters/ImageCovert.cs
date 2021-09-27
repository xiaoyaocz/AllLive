using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace AllLive.UWP.Converters
{
    public class ImageSourceConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return new BitmapImage(new Uri("ms-appx:///Assets/Placeholder/Placeholde.png"));
            }
           
            var url = value.ToString();
            return new BitmapImage(new Uri(url));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
