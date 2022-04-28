using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace P_SCAAT.Views.ValueConverters
{
    //[ValueConversion(typeof(decimal), typeof(string))] === NO NEED FOR IMPLICIT ATTRIBUTE
    public class OscilloscopeValuesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // decimal => string
            try
            {
                string unit = "";
                _ = decimal.TryParse(value.ToString(), out decimal numberResult);
                //decimal numberResult = System.Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                decimal absValue = Math.Abs(numberResult);
                if (absValue < 1M && absValue >= 1E-03M)
                {
                    numberResult *= 1E03M;
                    unit = "m";
                }
                else if (absValue < 1E-03M && absValue >= 1E-06M)
                {
                    numberResult *= 1E06M;
                    unit = "\u03BC";
                }
                else if (absValue < 1E-06M && absValue >= 1E-09M)
                {
                    numberResult *= 1E09M;
                    unit = "n";
                }
                else if (absValue < 1E-09M && absValue >= 1E-12M)
                {
                    numberResult *= 1E12M;
                    unit = "p";
                }

                //switch (numberResult)
                //{
                //    case < 1M and >= 1E-03M: numberResult *= 1E03M; unit = "m"; break;
                //    case < 1E-03M and >= 1E-06M: numberResult *= 1E06M; unit = "\u03BC"; break;
                //    case < 1E-06M and >= 1E-09M: numberResult *= 1E09M; unit = "n"; break;
                //    case < 1E-09M and >= 1E-12M: numberResult *= 1E12M; unit = "p"; break;
                //    default: break;
                //}
                if (parameter.ToString().Equals("VOLTS", StringComparison.Ordinal))
                {
                    unit += "V";
                }
                else if (parameter.ToString().Equals("SECONDS", StringComparison.Ordinal))
                {
                    unit += "s";
                }
                return numberResult.ToString($"0.### {unit}", CultureInfo.InvariantCulture);
            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp.Message);
            }
            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // string => decimal
            try
            {
                string userInput = value.ToString();
                userInput = Regex.Replace(userInput, @"\s", "");
                userInput = Regex.Replace(userInput, @",", ".");
                Match regexMatch = new Regex(@"([-]*\d*[.]*\d+)([mu\u03BC\u00B5np]*)([sV]*)").Match(userInput);
                _ = decimal.TryParse(regexMatch.Groups[1].Value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal numberResult);
                string unit = regexMatch.Groups[2].Value;
                if (unit.Equals("m", StringComparison.Ordinal))
                {
                    numberResult *= 1E-03M;
                }
                else if (unit.Equals("u", StringComparison.Ordinal) || unit.Equals("\u03BC", StringComparison.Ordinal) || unit.Equals("\u00B5", StringComparison.Ordinal))
                {
                    numberResult *= 1E-06M;
                }
                else if (unit.Equals("n", StringComparison.Ordinal))
                {
                    numberResult *= 1E-09M;
                }
                else if (unit.Equals("p", StringComparison.Ordinal))
                {
                    numberResult *= 1E-12M;
                }

                //switch (regexMatch.Groups[2].Value)
                //{
                //    case "m": numberResult *= 1E-03M; break;
                //    case "u" or "\u03BC" or "\u00B5": numberResult *= 1E-06M; break;
                //    case "n": numberResult *= 1E-09M; break;
                //    case "p": numberResult *= 1E-12M; break;
                //    default: break;
                //}
                return numberResult;
            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp.Message);
            }
            return 0;
        }
    }
}
