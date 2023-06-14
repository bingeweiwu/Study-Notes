using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace MS.Internal.SulpHur.SulpHurClient.Metro.Converters
{
    public class CountToVisibilityConverter : MarkupConverter
    {
        protected override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int count = (int)value;
            return count > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        protected override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ReversalConverter : MarkupConverter
    {
        protected override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((Visibility)value) { 
                case Visibility.Collapsed:
                case Visibility.Hidden:
                    return Visibility.Visible;
                case Visibility.Visible:
                    return Visibility.Collapsed;
                default:
                    return Visibility.Collapsed;
            }
        }

        protected override object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
