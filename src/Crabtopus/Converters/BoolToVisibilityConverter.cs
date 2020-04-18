using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Crabtopus.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public static readonly BoolToVisibilityConverter Default = new BoolToVisibilityConverter();
        public static readonly BoolToVisibilityConverter Inverse = new BoolToVisibilityConverter(true);

        public BoolToVisibilityConverter(bool invert)
        {
            Invert = invert;
        }

        private BoolToVisibilityConverter() : this(false)
        {
        }

        public bool Invert { get; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!Invert)
            {
                return (bool)value ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return !(bool)value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
