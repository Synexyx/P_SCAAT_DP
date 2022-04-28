using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace P_SCAAT.Views.ValueConverters
{
    public class EdgeSlopeOptionsToStringConverter : IValueConverter
    {

        //CURRENTLY NOT USING
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<string> resultList = value as List<string>;
            int indefOfOption = System.Convert.ToInt32(parameter, CultureInfo.InvariantCulture);
            return string.IsNullOrEmpty(resultList[indefOfOption])
                ? string.Empty
                : resultList[indefOfOption];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack is not supported.");
        }
    }
}
