using System;
using System.Globalization;
using System.Windows.Data;

namespace P_SCAAT.Views.ValueConverters
{
    /// <summary>
    /// Negates value of boolean. Used for binding IsEnabled for Controls.
    /// </summary>
    //[ValueConversion(typeof(bool), typeof(bool))] === NO NEED FOR IMPLICIT ATTRIBUTE
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool resultBool = (bool)value;
            return !resultBool;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool resultBool = (bool)value;
            return !resultBool;
        }
    }
}
