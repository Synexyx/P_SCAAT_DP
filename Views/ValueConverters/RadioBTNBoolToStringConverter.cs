using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace P_SCAAT.Views.ValueConverters
{
    //[ValueConversion(typeof(bool), typeof(string))] === NO NEED FOR IMPLICIT ATTRIBUTE
    public class RadioBTNBoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
            {
                return false;
            }
            string selected = value.ToString();
            return selected.Equals(parameter.ToString(), StringComparison.Ordinal);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }
}
