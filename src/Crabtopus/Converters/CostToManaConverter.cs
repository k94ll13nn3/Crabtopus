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
                        "W" => Application.Current.FindResource("W") as Viewbox,
                        "U" => Application.Current.FindResource("U") as Viewbox,
                        "B" => Application.Current.FindResource("B") as Viewbox,
                        "R" => Application.Current.FindResource("R") as Viewbox,
                        "G" => Application.Current.FindResource("G") as Viewbox,
                        "(U/R)" => Application.Current.FindResource("UR") as Viewbox,
                        "(U/B)" => Application.Current.FindResource("UB") as Viewbox,
                        "(B/G)" => Application.Current.FindResource("BG") as Viewbox,
                        "(B/R)" => Application.Current.FindResource("BR") as Viewbox,
                        "(G/U)" => Application.Current.FindResource("GU") as Viewbox,
                        "(G/W)" => Application.Current.FindResource("GW") as Viewbox,
                        "(R/G)" => Application.Current.FindResource("RG") as Viewbox,
                        "(R/W)" => Application.Current.FindResource("RW") as Viewbox,
                        "(W/B)" => Application.Current.FindResource("WB") as Viewbox,
                        "(W/U)" => Application.Current.FindResource("WU") as Viewbox,
                        "1" => Application.Current.FindResource("1") as Viewbox,
                        "2" => Application.Current.FindResource("2") as Viewbox,
                        "3" => Application.Current.FindResource("3") as Viewbox,
                        "4" => Application.Current.FindResource("4") as Viewbox,
                        "5" => Application.Current.FindResource("5") as Viewbox,
                        "6" => Application.Current.FindResource("6") as Viewbox,
                        "7" => Application.Current.FindResource("7") as Viewbox,
                        "8" => Application.Current.FindResource("8") as Viewbox,
                        "9" => Application.Current.FindResource("9") as Viewbox,
                        "X" => Application.Current.FindResource("X") as Viewbox,
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
