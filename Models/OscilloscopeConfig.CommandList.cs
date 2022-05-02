using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace P_SCAAT.Models
{
    internal abstract partial class OscilloscopeConfig
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
            public string WaveformFormatCommand { get; set; }
            private List<string> _waveformFormatOptions;
            public List<string> WaveformFormatOptions { get => _waveformFormatOptions ?? new List<string>(); set => _waveformFormatOptions = value; }
            public string WaveformStreamingCommand { get; set; }
            #endregion

            /// <summary>
            /// 
            /// </summary>
            /// <param name="command"></param>
            /// <param name="parameter1"></param>
            /// <returns></returns>
            /// <exception cref="FormatException"></exception>
            public static (string, string) UniversalCommandString(string command, string parameter1)
            {
                if (!string.IsNullOrEmpty(command))
                {
                    return ForgeCommandToString(command, parameter1);
                }
                throw new FormatException($"The command could not be created because it was not found in the command list file.{Environment.NewLine}{command}");
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="command"></param>
            /// <param name="parameter1"></param>
            /// <param name="parameter2"></param>
            /// <returns></returns>
            /// <exception cref="FormatException"></exception>
            public static (string, string) UniversalCommandString(string command, string parameter1, string parameter2)
            {
                if (!string.IsNullOrEmpty(command))
                {
                    return ForgeCommandToString(command, parameter1, parameter2);
                }
                throw new FormatException("The command could not be created because it was not found in the command list file.");
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="commad"></param>
            /// <returns></returns>
            /// <exception cref="FormatException"></exception>
            public static string UniversalAskCommandString(string commad)
            {
                if (!string.IsNullOrEmpty(commad))
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    return stringBuilder.AppendFormat(CultureInfo.InvariantCulture, $"{commad}?").ToString();
                }
                throw new FormatException("The command could not be created because it was not found in the command list file.");
            }


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

            //ToDo smazat
            #region NOT USING
            //public (string, string) WaveformStreamingCommandString(bool waveformStreaming)
            //{
            //    if (string.IsNullOrEmpty(WaveformStreamingCommand))
            //    {
            //        return (string.Empty, string.Empty);
            //    }
            //    string waveformStreamingString = TrueFalseOptions != null
            //        ? waveformStreaming ? TrueFalseOptions.ElementAtOrDefault(0) ?? "1" : TrueFalseOptions.ElementAtOrDefault(1) ?? "0"
            //        : waveformStreaming ? "ON" : "OFF";
            //    return ForgeCommandToString(WaveformStreamingCommand, waveformStreamingString);
            //}
            //public (string, string) WaveformSourceCommandString()
            //{
            //    return (string.Empty, string.Empty);
            //}
            //public (string, string) WaveformFormatCommandString(int waveformFormatIndex)
            //{
            //    if (string.IsNullOrEmpty(WaveformFormatCommand) || WaveformFormatOptions == null)
            //    {
            //        return (string.Empty, string.Empty);
            //    }
            //    string selectedOptionByIndex = WaveformFormatOptions.ElementAtOrDefault(waveformFormatIndex) ?? string.Empty;
            //    return ForgeCommandToString(WaveformFormatCommand, selectedOptionByIndex);
            //}
            //public (string, string) WaveformDataCommandString()
            //{
            //    return string.IsNullOrEmpty(WaveformDataCommand)
            //        ? (string.Empty, string.Empty)
            //        : ForgeCommandToString(WaveformDataCommand, string.Empty);
            //}
            //public int NumberOfDigitalChannels { get; set; } //Maybe used later

            //public string ChannelPositionCommand { get; set; }
            //#region ====== CHANNEL ======

            //public (string, string) ChannelDisplayCommandString(int channelNumber, bool channelDisplay)
            //{
            //    if (string.IsNullOrEmpty(ChannelDisplayCommand))
            //    {
            //        return (string.Empty, string.Empty);
            //    }
            //    string channelDisplayString = TrueFalseOptions != null
            //        ? channelDisplay ? TrueFalseOptions.ElementAtOrDefault(0) ?? "1" : TrueFalseOptions.ElementAtOrDefault(1) ?? "0"
            //        : channelDisplay ? "ON" : "OFF";
            //    //StringBuilder stringBuilder = new();
            //    //string partCommand = stringBuilder.AppendFormat(CultureInfo.InvariantCulture,
            //    //    ChannelDisplayCommand, channelNumber, string.Empty).ToString();
            //    //_ = stringBuilder.Clear();
            //    //string resultCommand = stringBuilder.AppendFormat(CultureInfo.InvariantCulture,
            //    //    ChannelDisplayCommand, channelNumber, channelDisplayString).ToString();
            //    //return (partCommand, resultCommand);
            //    return ForgeCommandToString(ChannelDisplayCommand, channelNumber.ToString(CultureInfo.InvariantCulture),
            //        channelDisplayString);
            //    //return ForgeCommandToString(ChannelDisplayCommand, channelNumber.ToString(CultureInfo.InvariantCulture),
            //    //    channelDisplay);
            //}
            //public string ChannelDisplayAskCommandString(int channelNumber)
            //{

            //    return string.IsNullOrEmpty(ChannelDisplayCommand)
            //        ? string.Empty
            //        : ForgeCommandToString(ChannelDisplayCommand, channelNumber.ToString(CultureInfo.InvariantCulture), "?").Item2;
            //}
            ////public (string, string) ChannelLabelCommandString(int channelNumber, string channelLabel)
            ////{
            ////    
            ////    return string.IsNullOrEmpty(ChannelLabelCommand)
            ////        ? (string.Empty, string.Empty)
            ////        : ForgeCommandToString(ChannelLabelCommand, channelNumber.ToString(CultureInfo.InvariantCulture),
            ////        channelLabel);
            ////}
            //public string ChannelLabelAskCommandString(int channelNumber)
            //{
            //    return string.IsNullOrEmpty(ChannelLabelCommand)
            //        ? string.Empty
            //        : ForgeCommandToString(ChannelLabelCommand, channelNumber.ToString(CultureInfo.InvariantCulture), "?").Item2.Trim().Replace(" ", string.Empty);
            //}
            ////public (string, string) ChannelScaleCommandString(int channelNumber, decimal channelScale)
            ////{
            ////    return string.IsNullOrEmpty(ChannelScaleCommand)
            ////        ? (string.Empty, string.Empty)
            ////        : ForgeCommandToString(ChannelScaleCommand, channelNumber.ToString(CultureInfo.InvariantCulture),
            ////        channelScale.ToString("##0E00", CultureInfo.InvariantCulture));
            ////}
            ////public string ChannelScaleAskCommandString(int channelNumber)
            ////{
            ////    return string.IsNullOrEmpty(ChannelScaleCommand)
            ////        ? string.Empty
            ////        : ForgeCommandToString(ChannelScaleCommand, channelNumber.ToString(CultureInfo.InvariantCulture), "?").Item2;
            ////}
            ////public (string, string) ChannelOffsetCommandString(int channelNumber, decimal channelOffset)
            ////{
            ////    return string.IsNullOrEmpty(ChannelOffsetCommand)
            ////        ? (string.Empty, string.Empty)
            ////        : ForgeCommandToString(ChannelOffsetCommand, channelNumber.ToString(CultureInfo.InvariantCulture),
            ////        channelOffset.ToString("##0E00", CultureInfo.InvariantCulture));
            ////}
            //#endregion
            //#region ====== TIMEBASE =====

            //public (string, string) TimebasePositionCommandString(decimal timebasePosition)
            //{
            //    return string.IsNullOrEmpty(TimebasePositionCommand)
            //        ? (string.Empty, string.Empty)
            //        : ForgeCommandToString(TimebasePositionCommand, timebasePosition.ToString("##0E00", CultureInfo.InvariantCulture));
            //}
            //public (string, string) TimebaseScaleCommandString(decimal timebaseScale)
            //{
            //    return string.IsNullOrEmpty(TimebaseScaleCommand)
            //        ? (string.Empty, string.Empty)
            //        : ForgeCommandToString(TimebaseScaleCommand, timebaseScale.ToString("##0E00", CultureInfo.InvariantCulture));
            //}
            //#endregion
            //internal void UpdateDynamicOptions()
            //{
            //    //if (TriggerEdgeSourceOptions != null)
            //    //{
            //    //    List<string> partList = new();
            //    //    List<string> analogChannelPart = TriggerEdgeSourceOptions
            //    //        .Where(x => x.ToLower(CultureInfo.InvariantCulture).Contains("ch")).ToList();


            //    //    StringBuilder stringBuilder = new();
            //    //    //for (int i = 0; i < NumberOfAnalogChannels; i++)
            //    //    for (int i = 0; i < NumberOfAnalogChannels; i++)
            //    //    {
            //    //        string test = stringBuilder.AppendFormat(CultureInfo.InvariantCulture, analogChannelPart.First(), i + 1).ToString();
            //    //        Debug.WriteLine(test);
            //    //    }
            //    //    for(int i = 0; i < NumberOfDigitalChannels; i++)
            //    //    {

            //    //    }
            //    //}
            ////}
            //#region ====== TRIGGER ======

            //public (string, string) TriggerEdgeSourceCommandString(int triggerEdgeSourceIndex)
            //{
            //    if (string.IsNullOrEmpty(TriggerEdgeSourceCommand) || TriggerEdgeSourceOptions == null)
            //    {
            //        return (string.Empty, string.Empty);
            //    }
            //    string selectedOptionByIndex = TriggerEdgeSourceOptions.ElementAtOrDefault(triggerEdgeSourceIndex) ?? string.Empty;
            //    return ForgeCommandToString(TriggerEdgeSourceCommand, selectedOptionByIndex);
            //}
            //public (string, string) TriggerEdgeSlopeCommandString(int triggerEdgeSlopeIndex)
            //{
            //    if (string.IsNullOrEmpty(TriggerEdgeSlopeCommand) || TriggerEdgeSlopeOptions == null)
            //    {
            //        return (string.Empty, string.Empty);
            //    }
            //    string selectedOptionByIndex = TriggerEdgeSlopeOptions.ElementAtOrDefault(triggerEdgeSlopeIndex) ?? string.Empty;
            //    return ForgeCommandToString(TriggerEdgeSlopeCommand, selectedOptionByIndex);
            //}
            //#endregion

            //https://download.tek.com/manual/MDO4000-B-MSO-DPO4000B-and-MDO3000-Oscilloscope-Programmer-Manual-Rev-A.pdf
            //      page 240
            //      CH<x>? (Query Only)
            //      Returns the vertical parameters for channel<x>, where x is the channel number (1–4).
            //      Group Vertical
            //      Syntax CH<x>?
            //
            //      page 254
            //      page 259
            //      test page 272-3
            //      
            //      page 292 - data retrieval

            //      page 367 - timebase?
            //      page 438 - meassurment (hrany a prostě bordel??) might skip
            //      test page 764
            //      page 778 - TRIGGERS
            //      page 874 - trigger edge slope + source
            //      page 889 - trigger mode

            //      page 229 - osciloscope status
            //      page 941 - wait command ??
            //
            #endregion





        }

    }
}
