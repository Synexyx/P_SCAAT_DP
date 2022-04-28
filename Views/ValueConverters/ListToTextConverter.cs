using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace P_SCAAT.Views.ValueConverters
{
    /// <summary>
    /// Converts List<string> to string and vice versa. Used for binding between TextBox.Text and List<string>.
    /// </summary>
    //[ValueConversion(typeof(List<string>), typeof(string))] === NO NEED FOR IMPLICIT ATTRIBUTE
    public class ListToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            StringBuilder stringBuilder = new StringBuilder();
            List<string> tempList = value as List<string>;
            foreach (string item in tempList)
            {
                _ = stringBuilder.AppendLine(item);
            }

            return stringBuilder.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string tempString = value.ToString();
            List<string> resultList = Regex.Split(tempString, @"\r|\n|\r\n").Where(line => line != string.Empty).ToList();
            return resultList;
        }
    }
}
