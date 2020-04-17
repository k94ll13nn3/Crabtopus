using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

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
                    Viewbox? viewBox = manaCost switch
                    {
                        "W" => Application.Current.FindResource("MonoWhite") as Viewbox,
                        "U" => Application.Current.FindResource("MonoBlue") as Viewbox,
                        "B" => Application.Current.FindResource("MonoBlack") as Viewbox,
                        "R" => Application.Current.FindResource("MonoRed") as Viewbox,
                        "G" => Application.Current.FindResource("MonoGreen") as Viewbox,
                        "(U/R)" => Application.Current.FindResource("UR") as Viewbox,
                        "(U/B)" => Application.Current.FindResource("UB") as Viewbox,
                        "1" => Application.Current.FindResource("OneGeneric") as Viewbox,
                        "2" => Application.Current.FindResource("TwoGeneric") as Viewbox,
                        "3" => Application.Current.FindResource("ThreeGeneric") as Viewbox,
                        "X" => Application.Current.FindResource("XGeneric") as Viewbox,
                        "//" => new Viewbox { Height = 15, Width = 15, Child = new TextBlock { Text = "//" } },
                        _ => new Viewbox { Height = 15, Width = 15, Child = new Ellipse { Width = 600, Height = 600, Fill = Brushes.Black } },
                    };

                    if (viewBox != null)
                    {
                        boxes.Add(viewBox);
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
