using System;
using System.Globalization;
using System.Windows.Data;

namespace MessengerClient.Converters
{
    public class TimestampConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime timestamp)
            {
                TimeSpan diff = DateTime.Now - timestamp;

                if (diff.TotalMinutes < 1)
                    return "Gerade eben";
                if (diff.TotalMinutes < 60)
                    return $"vor {(int)diff.TotalMinutes} Min";
                if (diff.TotalHours < 24)
                    return $"vor {(int)diff.TotalHours}h";
                if (diff.TotalDays < 7)
                    return $"vor {(int)diff.TotalDays}d";

                return timestamp.ToString("dd.MM.yyyy");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
