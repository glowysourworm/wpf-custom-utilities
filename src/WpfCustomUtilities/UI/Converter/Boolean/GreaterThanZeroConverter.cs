using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfCustomUtilities.UI.Converter
{
    public class GreaterThanZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Binding.DoNothing;

            var comparable = value as IComparable;

            if (value == null)
                return Binding.DoNothing;

            return comparable.CompareTo(0) > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
