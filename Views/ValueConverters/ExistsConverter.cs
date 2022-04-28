using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace P_SCAAT.Views.ValueConverters
{
    public class ExistsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //CURRENTLY NOT USING
            bool result = value != null || !string.IsNullOrEmpty((string)value);
            if (parameter != null && parameter.ToString().Equals("INVERSE", StringComparison.Ordinal))
            {
                bool test = parameter.ToString().Equals("INVERSE", StringComparison.Ordinal);
                return test ? !result : result;
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack is not supported.");
        }
    }
}
