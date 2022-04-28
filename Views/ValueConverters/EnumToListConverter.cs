using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace P_SCAAT.Views.ValueConverters
{
    /// <summary>
    /// Converts Enum to List<Enum> for binding to ComboBox.ItemsSource
    /// </summary>
    //[ValueConversion(typeof(List<Enum>), typeof(IEnumerable))] === NO NEED FOR IMPLICIT ATTRIBUTE
    public class EnumToListConverter : IValueConverter
    {
        //CURRENTLY NOT USING
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Enum.GetValues(value.GetType()).Cast<Enum>().ToList();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack is not supported.");
            //return "";
        }
    }
}
