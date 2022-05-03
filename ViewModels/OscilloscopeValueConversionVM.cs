using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace P_SCAAT.ViewModels
{
    internal abstract class OscilloscopeValueConversionVM : CorePropChangedVM
    {
        protected decimal StringToDecimal(string stringToConvert)
        {
            stringToConvert = Regex.Replace(stringToConvert, @"\s", string.Empty);
            stringToConvert = Regex.Replace(stringToConvert, @",", ".");
            Match regexMatch = new Regex(@"([-]*\d*[.]*\d+)([mu\u03BC\u00B5np]*)([sV]*)").Match(stringToConvert);
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
            return numberResult;

        }
        protected string DecimalToString(decimal decimalToConvert, string unit)
        {
            string unitPrefix = string.Empty;
            decimal absValue = Math.Abs(decimalToConvert);
            if (absValue < 1M && absValue >= 1E-03M)
            {
                decimalToConvert *= 1E03M;
                unitPrefix = "m";
            }
            else if (absValue < 1E-03M && absValue >= 1E-06M)
            {
                decimalToConvert *= 1E06M;
                unitPrefix = "\u03BC";
            }
            else if (absValue < 1E-06M && absValue >= 1E-09M)
            {
                decimalToConvert *= 1E09M;
                unitPrefix = "n";
            }
            else if (absValue < 1E-09M && absValue >= 1E-12M)
            {
                decimalToConvert *= 1E12M;
                unitPrefix = "p";
            }
            return decimalToConvert.ToString($"0.### {unitPrefix}{unit}", CultureInfo.InvariantCulture);
        }
    }
}
