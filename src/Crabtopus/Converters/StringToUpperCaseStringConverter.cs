using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Crabtopus.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class StringToUpperCaseStringConverter : IValueConverter
    {
        public static readonly StringToUpperCaseStringConverter Default = new StringToUpperCaseStringConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as string)?.ToUpperInvariant() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
