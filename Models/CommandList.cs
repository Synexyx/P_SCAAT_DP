using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace P_SCAAT.Models
{
    internal class CommandList
    {
        #region Properties
        public string OscilloscopeSeriesID { get; set; }
        public int NumberOfAnalogChannels { get; set; }

        private List<string> _trueFalseOptions;
        public List<string> TrueFalseOptions { get => _trueFalseOptions ?? new List<string>(); set => _trueFalseOptions = value; }
        public string OscilloscopeSingleAcquisitionCommand { get; set; }
        public string OscilloscopeStopCommand { get; set; }
        public string OsclloscopeRunCommand { get; set; }
        public string OscilloscopeOperationCompleteCommand { get; set; }
        public string OscilloscopeTriggerEventCommand { get; set; }

        public string ChannelDisplayCommand { get; set; }
        public string ChannelLabelCommand { get; set; }
        public string ChannelScaleCommand { get; set; }

        public string ChannelOffsetCommand { get; set; }
        public string ChannelCouplingCommand { get; set; }
        private List<string> _channelCouplingModes;
        public List<string> ChannelCouplingModes { get => _channelCouplingModes ?? new List<string>(); set => _channelCouplingModes = value; }

        public string TimebasePositionCommand { get; set; }
        public string TimebaseScaleCommand { get; set; }

        public string TriggerEdgeSourceCommand { get; set; }
        private List<string> _triggerEdgeSourceOptions;
        public List<string> TriggerEdgeSourceOptions { get => _triggerEdgeSourceOptions ?? new List<string>(); set => _triggerEdgeSourceOptions = value; }
        public string TriggerEdgeSlopeCommand { get; set; }
        private List<string> _triggerEdgeSlopeOptions;
        public List<string> TriggerEdgeSlopeOptions { get => _triggerEdgeSlopeOptions ?? new List<string>(); set => _triggerEdgeSlopeOptions = value; }
        public string TriggerLevelCommand { get; set; }

        public string WaveformDataCommand { get; set; }
        public string WaveformSourceCommand { get; set; }
        private List<string> _waveformSourceOptions;
        public List<string> WaveformSourceOptions { get => _waveformSourceOptions ?? new List<string>(); set => _waveformSourceOptions = value; }
        public string WaveformFormatCommand { get; set; }
        private List<string> _waveformFormatOptions;
        public List<string> WaveformFormatOptions { get => _waveformFormatOptions ?? new List<string>(); set => _waveformFormatOptions = value; }
        public string WaveformStreamingCommand { get; set; }
        #endregion

        /// <summary>
        /// If <paramref name="command"/> exists, creates properly formated string to be send to the device as command with parameter.
        /// <br/>
        /// If <paramref name="command"/> does not exist, throws <see cref="FormatException"/>.
        /// <br/>
        /// <example>Example:
        /// <code>
        ///     //<paramref name="command"/> =  ":TIMebase:SCALe {0}";
        ///     //<paramref name="parameter1"/> = "20E-03";
        ///     //=> <see cref="Tuple{T1, T2}.Item1"/> = ":TIMebase:SCALe"
        ///     //=> <see cref="Tuple{T1, T2}.Item2"/> = ":TIMebase:SCALe 20E-03"
        /// </code>
        /// </example>
        /// </summary>
        /// <exception cref="FormatException"/>
        public static (string, string) UniversalCommandString(string command, string parameter1)
        {
            if (!string.IsNullOrEmpty(command))
            {
                return ForgeCommandToString(command, parameter1);
            }
            throw new FormatException($"The command could not be created because it was not found in the command list file.{Environment.NewLine}{command}");
        }
        /// <summary>
        /// Overloaded method for <see cref="UniversalCommandString(string, string)"/> with additional <paramref name="parameter2"/>
        /// </summary>
        /// <exception cref="FormatException"/>
        public static (string, string) UniversalCommandString(string command, string parameter1, string parameter2)
        {
            if (!string.IsNullOrEmpty(command))
            {
                return ForgeCommandToString(command, parameter1, parameter2);
            }
            throw new FormatException("The command could not be created because it was not found in the command list file.");
        }
        /// <summary>
        /// If <paramref name="command"/> exists, creates properly formated string to be send to the device as command query.
        /// <br/>
        /// <example>Example:
        /// <code>
        ///     //<paramref name="command"/> = ":TIMebase:SCALe {0}";
        ///     //=> ":TIMebase:SCALe?"
        /// </code>
        /// </example>
        /// </summary>
        /// <exception cref="FormatException"/>
        public static string UniversalAskCommandString(string command)
        {
            if (!string.IsNullOrEmpty(command))
            {
                StringBuilder stringBuilder = new StringBuilder();
                string askCommand = stringBuilder.AppendFormat(CultureInfo.InvariantCulture, command, "?").ToString();
                return Regex.Replace(askCommand, @"\s+", string.Empty);
            }
            throw new FormatException("The command could not be created because it was not found in the command list file.");
        }
        /// <summary>
        /// Overloaded method for <see cref="UniversalAskCommandString(string)"/> with additional <paramref name="parameter1"/>
        /// </summary>
        /// <exception cref="FormatException"/>
        public static string UniversalAskCommandString(string command, string parameter1)
        {
            if (!string.IsNullOrEmpty(command))
            {
                StringBuilder stringBuilder = new StringBuilder();
                string askCommand = stringBuilder.AppendFormat(CultureInfo.InvariantCulture, command, parameter1, "?").ToString();
                return Regex.Replace(askCommand, @"\s+", string.Empty);

            }
            throw new FormatException("The command could not be created because it was not found in the command list file.");
        }

        /// <summary>
        /// If <paramref name="command"/> exists, creates properly formated string to be send to the device as command query with <paramref name="parameter1"/> in source dependant commands.
        /// <br/>
        /// <example>Example:
        /// <code>
        ///     //<paramref name="command"/> = ":TRIGger:LEVel {0} {1}";
        ///     //<paramref name="parameter1"/> = "CHANnel1";
        ///     //=> ":TRIGger:LEVel? CHANnel1"
        /// </code>
        /// </example>
        /// </summary>
        public static string SourceDependantAskCommandString(string command, string parameter1)
        {
            if (!string.IsNullOrEmpty(command))
            {
                StringBuilder stringBuilder = new StringBuilder();
                string tempCommand = stringBuilder.AppendFormat(CultureInfo.InvariantCulture, command, "?", string.Empty).ToString();
                tempCommand = Regex.Replace(tempCommand, @"\s+", string.Empty);
                _ = stringBuilder.Clear();
                string askCommand = stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0} {1}", tempCommand, parameter1).ToString();
                return askCommand;
            }
            throw new FormatException("The command could not be created because it was not found in the command list file.");
        }

        /// <summary>
        /// Using <see cref="StringBuilder"/> creates <see cref="Tuple{T1, T2}"/>. <see cref="Tuple{T1, T2}.Item1"/> is pure <paramref name="selectedCommand"/> body. 
        /// <see cref="Tuple{T1, T2}.Item2"/> is <paramref name="selectedCommand"/> body with <paramref name="commandParameter1"/> inserted to proper place using string interpolation.
        /// <example>
        /// <code>
        ///     //<paramref name="selectedCommand"/> =  ":TIMebase:SCALe {0}";
        ///     //<paramref name="commandParameter1"/> = "20E-03";
        ///     //=> <see cref="Tuple{T1, T2}.Item1"/> = ":TIMebase:SCALe"
        ///     //=> <see cref="Tuple{T1, T2}.Item2"/> = ":TIMebase:SCALe 20E-03"
        /// </code>
        /// </example>
        /// </summary>
        private static (string, string) ForgeCommandToString(string selectedCommand, string commandParameter1)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string partCommand = stringBuilder.AppendFormat(CultureInfo.InvariantCulture,
                selectedCommand, string.Empty).ToString();
            _ = stringBuilder.Clear();
            string resultCommand = stringBuilder.AppendFormat(CultureInfo.InvariantCulture,
                selectedCommand, commandParameter1).ToString();
            return (partCommand, resultCommand);
        }
        /// <summary>
        /// Overloaded method for <see cref="ForgeCommandToString(string, string)"/> with additional <paramref name="commandParameter2"/>
        /// </summary>
        private static (string, string) ForgeCommandToString(string selectedCommand, string commandParameter1, string commandParameter2)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string partCommand = stringBuilder.AppendFormat(CultureInfo.InvariantCulture,
                selectedCommand, commandParameter1, string.Empty).ToString();
            _ = stringBuilder.Clear();
            string resultCommand = stringBuilder.AppendFormat(CultureInfo.InvariantCulture,
                selectedCommand, commandParameter1, commandParameter2).ToString();
            return (partCommand, resultCommand);
        }
    }
}

