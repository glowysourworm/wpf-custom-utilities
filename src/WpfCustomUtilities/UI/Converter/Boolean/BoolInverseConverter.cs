using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfCustomUtilities.UI.Converter
{
    public class BoolInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null &&
                value == DependencyProperty.UnsetValue)
                return Binding.DoNothing;

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null &&
                value == DependencyProperty.UnsetValue)
                return Binding.DoNothing;

            return !(bool)value;
        }
    }
}
