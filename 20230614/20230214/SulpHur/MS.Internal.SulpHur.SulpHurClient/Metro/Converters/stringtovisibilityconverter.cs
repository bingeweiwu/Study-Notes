using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MS.Internal.SulpHur.SulpHurClient.Metro.Converters
{
    public class StringToVisibilityConverter : MarkupConverter
    {
        protected override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        protected override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
