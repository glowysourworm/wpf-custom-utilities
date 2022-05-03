using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfCustomUtilities.UI.Converter
{
    public class ParameterMultiplyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (ReferenceEquals(value, null) ||
                ReferenceEquals(value, DependencyProperty.UnsetValue) ||
                ReferenceEquals(parameter, null))
                return Binding.DoNothing;

            var valueDouble = System.Convert.ToDouble(value);
            var multiplier = System.Convert.ToDouble(parameter);

            return System.Convert.ChangeType(valueDouble * multiplier, value.GetType());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
