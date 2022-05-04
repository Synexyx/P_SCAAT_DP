using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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
        /// If <paramref name="command"/> exists, creates properly formated string to be send to the device with <paramref name="command"/> body and <paramref name="parameter1"/>.
        /// </summary>
        /// <returns>(<typeparamref name="string"/>, <typeparamref name="string"/>) where <c>Item1</c> is commands body and <c>Item2</c> is whole command string with parameter</returns>
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
        /// If <paramref name="command"/> exists, creates properly formated string to be send to the device with <paramref name="command"/> body <paramref name="parameter1"/> and <paramref name="parameter2"/>.
        /// </summary>
        /// <returns>(<typeparamref name="string"/>, <typeparamref name="string"/>) where <c>Item1</c> is commands body and <c>Item2</c> is whole command string with parameters</returns>
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
        /// If <paramref name="command"/> exists, creates properly formated string to be send to the device as command query expecting value as response from the device.
        /// </summary>
        /// <returns><typeparamref name="string"/> as properly formated query command.</returns>
        /// <exception cref="FormatException"/>
        public static string UniversalAskCommandString(string commad)
        {
            if (!string.IsNullOrEmpty(commad))
            {
                StringBuilder stringBuilder = new StringBuilder();
                string askCommand = stringBuilder.AppendFormat(CultureInfo.InvariantCulture, commad, "?").ToString();
                return Regex.Replace(askCommand, @"\s+", string.Empty);
            }
            throw new FormatException("The command could not be created because it was not found in the command list file.");
        }
        /// <summary>
        /// If <paramref name="command"/> exists, creates properly formated string to be send to the device as command query with <paramref name="parameter1"/> expecting value as response from the device.
        /// </summary>
        /// <returns><typeparamref name="string"/> as properly formated query command.</returns>
        /// <exception cref="FormatException"/>
        public static string UniversalAskCommandString(string commad, string parameter1)
        {
            if (!string.IsNullOrEmpty(commad))
            {
                StringBuilder stringBuilder = new StringBuilder();
                string askCommand = stringBuilder.AppendFormat(CultureInfo.InvariantCulture, commad, parameter1, "?").ToString();
                return Regex.Replace(askCommand, @"\s+", string.Empty);

            }
            throw new FormatException("The command could not be created because it was not found in the command list file.");
        }

        /// <summary>
        /// Using <see cref="StringBuilder"/> creates <typeparamref name="Tuple"/>(<typeparamref name="string"/>, <typeparamref name="string"/>). <c>Item1</c> is pure <paramref name="selectedCommand"/>
        /// without any dynamic parts such as <c>{0}, {1}</c> etc. used for string interpolation. 
        /// <c>Item2</c> is same string with <paramref name="commandParameter1"/> inserted to proper place using string interpolation.
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
        /// Using <see cref="StringBuilder"/> creates <typeparamref name="Tuple"/>(<typeparamref name="string"/>, <typeparamref name="string"/>). <c>Item1</c> is <paramref name="selectedCommand"/>
        /// with <paramref name="commandParameter1"/> inserted to first string interpolation place. 
        /// <c>Item2</c> is same string with <paramref name="commandParameter2"/> inserted to proper place using string interpolation.
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

