using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MessengerClient.Converters
{
    public class MessageBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSent)
            {
                return isSent 
                    ? new SolidColorBrush(Color.FromRgb(25, 118, 210))
                    : new SolidColorBrush(Color.FromRgb(66, 66, 66));
            }
            return new SolidColorBrush(Color.FromRgb(66, 66, 66));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
