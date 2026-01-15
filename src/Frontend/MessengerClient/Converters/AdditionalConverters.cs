using System;
using System.Globalization;
using System.Windows.Data;

namespace MessengerClient.Converters
{
    public class FirstCharConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && !string.IsNullOrEmpty(str))
            {
                return str[0].ToString().ToUpper();
            }
            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TimeAgoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                var now = DateTime.Now;
                var diff = now - dateTime;

                if (diff.TotalMinutes < 1)
                    return "jetzt";
                if (diff.TotalMinutes < 60)
                    return $"{(int)diff.TotalMinutes}m";
                if (diff.TotalHours < 24)
                    return $"{(int)diff.TotalHours}h";
                if (diff.TotalDays < 7)
                    return $"{(int)diff.TotalDays}d";
                
                return dateTime.ToString("dd.MM");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSent)
            {
                return isSent ? System.Windows.Media.Brushes.Black : System.Windows.Media.Brushes.Black;
            }
            return System.Windows.Media.Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
