using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Crabtopus.Converters
{
    [ValueConversion(typeof(string), typeof(List<Viewbox>))]
    public class CostToManaConverter : IValueConverter
    {
        public static readonly CostToManaConverter Default = new CostToManaConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boxes = new List<Viewbox>();
            if (value is string s)
            {
                foreach (string manaCost in s.Split('o', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (manaCost == "G")
                    {
                        boxes.Add(Application.Current.FindResource("MonoGreen") as Viewbox);
                    }

                    if (manaCost == "R")
                    {
                        boxes.Add(Application.Current.FindResource("MonoRed") as Viewbox);
                    }
                }
            }

            return boxes;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
