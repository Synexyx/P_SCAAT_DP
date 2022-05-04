using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NationalInstruments.Visa;
using Ivi.Visa;
using System.Threading;
using System.Globalization;
using ResourceManager = NationalInstruments.Visa.ResourceManager;
using P_SCAAT.Exceptions;
using System.Text.RegularExpressions;
using System.IO;

namespace P_SCAAT.Models
{
    internal class Oscilloscope : OscilloscopeConfig, ISessionDevice
    {
        #region Properties
        public string SessionName
        {
            get;
            private set;
        }
        public MessageBasedSession MessageBasedSession
        {
            get;
            private set;
        }

        public bool IsSessionOpen
        {
            get;
            private set;
        }
        #endregion


        #region StaticGetOscilloscopeList
        /// <summary>
        /// Static method for getting <see cref="List{T}"/> of resources (osciloscopes) available for session.
        /// </summary>
        /// <returns>List of available oscilloscopes</returns>
        public static List<string> GetOscilloscopeList()
        {
            List<string> rsList = new List<string>();
            using (ResourceManager rmSession = new ResourceManager())
            {
                IEnumerable<string> resources = rmSession.Find("(ASRL|USB)?*");
                foreach (string rsName in resources)
                {
                    rsList.Add(rsName);
                }
                return rsList;
            }
        }
        #endregion

        public void OpenSession(string sessionName)
        {
            SessionName = sessionName;
            Debug.WriteLine("Openning session: " + SessionName);
            using (ResourceManager rmSession = new ResourceManager())
            {
                try
                {
                    MessageBasedSession = (MessageBasedSession)rmSession.Open(SessionName);
                    IsSessionOpen = true;
                    InitializeSessionSettings();
                    Debug.WriteLine("Session: " + SessionName + " succesfully opened.");
                }
                catch (Exception ex)
                {
                    throw new SessionControlException($"Oscilloscope session cannot be estabilished!{Environment.NewLine}REASON :{ex.GetType()}{Environment.NewLine}{ex.Message}", ex);
                }
            }
        }

        public void InitializeSessionSettings()
        {
            //ToDo prostě do commandListu dát celý ID
            //string oscilloscopeID = QueryData("*IDN?");
            string oscilloscopeID = "Agilent,MSO9104A,...,...";
            InitializeSettings(oscilloscopeID);
        }

        public void CloseSession()
        {
            if (MessageBasedSession != null)
            {
                MessageBasedSession.Dispose();
            }
            ClearAllData();



            Debug.WriteLine("Session succesfully closed.");
        }

        internal void MeasurePrep()
        {
            Thread.Sleep(5);
            return;
            bool isReady = false;
            SendData(Commands.OscilloscopeSingleAcquisitionCommand);
            while (!isReady)
            {
                Thread.Sleep(100);
                isReady = QueryData(Commands.OscilloscopeOperationCompleteCommand).Contains("1");
            }
        }
        internal void ChangeWaveformSource(string selectedSource)
        {
            string switchCorrectSource = CommandList.UniversalCommandString(Commands.WaveformSourceCommand, selectedSource).Item2;
            SendData(switchCorrectSource);
        }

        internal byte[] GetMeasuredData()
        {
            Thread.Sleep(5);
            byte[] test = new byte[] { 1, 2, 3 };
            //return test;

            Thread.Sleep(10);
            return test;
            SendData(Commands.WaveformDataCommand);
            Thread.Sleep(10);
            MemoryStream memoryStream = new MemoryStream();
            while (true)
            {
                try
                {
                    byte[] response = MessageBasedSession.RawIO.Read();
                    memoryStream.Write(response, 0, response.Length);
                }
                catch (Exception)
                {
                    Debug.WriteLine("Reading finished");
                    break;
                }
            }
            //var bitconv = BitConverter.ToString(memoryStream.GetBuffer());
            //var utfString = Encoding.UTF8.GetString(memoryStream.GetBuffer());
            //var asciiString = Encoding.ASCII.GetString(memoryStream.ToArray());
            //var base64String = Convert.ToBase64String(memoryStream.ToArray());
            byte[] result = memoryStream.GetBuffer();
            int lastIndex = Array.FindLastIndex(result, b => b != 0);
            Array.Resize(ref result, lastIndex + 1);
            return result;
        }

        internal void ListCurrentCommands()
        {
            foreach (string item in OscilloscopeConfigString)
            {
                Debug.WriteLine("FINAL " + item);
            }
        }

        internal void ApplyAllSettingsToDevice()
        {
            foreach (string setting in OscilloscopeConfigString)
            {
                Thread.Sleep(20);
                SendData(setting);
            }
        }

        public void SendData(string dataString)
        {
            //*IDN ?\n
            try
            {
                MessageBasedSession.RawIO.Write(dataString);
            }
            catch (Exception ex)
            {
                throw new SessionCommunicationException($"Sending data to the oscilloscope failed!{Environment.NewLine}REASON :{ex.GetType()}{Environment.NewLine}{ex.StackTrace}");
            }
        }
        public string QueryData(string dataString)
        {
            string resultData;
            try
            {
                SendData(dataString);
                resultData = ReadStringData();
            }
            catch (SessionCommunicationException ex)
            {
                throw new SessionCommunicationException($"Data query to the oscilloscope failed!{Environment.NewLine}REASON :{ex.GetType()}{Environment.NewLine}{ex.StackTrace}");
            }
            return resultData;
        }

        public string ReadStringData()
        {
            try
            {
                //MessageBasedSession.TimeoutMilliseconds = 5000;
                string response = MessageBasedSession.FormattedIO.ReadUntilEnd();
                return response;
            }
            catch (Exception ex)
            {
                throw new SessionCommunicationException($"Reading data from the oscilloscope failed!{Environment.NewLine}REASON :{ex.GetType()}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        //private static string ReplaceEscapeSeq(string message)
        //{
        //    return message.Replace("\\n", "\n").Replace("\\r", "\r");
        //}

        //private static string InsertEscapeSeq(string message)
        //{
        //    //return message.Replace("\n", "\\n").Replace("\r", "\\r");
        //    return message.Replace("\n", string.Empty).Replace("\r", string.Empty);
        //}

        public override void ClearAllData()
        {
            SessionName = null;
            IsSessionOpen = false;
            base.ClearAllData();
        }

        public void SynchronizeConfig()
        {
            SynchronizeChannels();
            SynchronizeTrigger();
            SynchronizeTimebase();
            SynchronizeWaveform();
        }
        private void SynchronizeChannels()
        {
            foreach (ChannelSettings channel in Channels)
            {
                SynchronizeChannelLabel(channel);
                SynchronizeChannelDisplay(channel);
                SynchronizeChannelScale(channel);
                SynchronizeChannelOffset(channel);
                SynchronizeChannelCoupling(channel);
            }
        }
        private void SynchronizeTrigger()
        {
            SynchronizeTriggerEdgeSource(Trigger);
            SynchronizeTriggerEdgeSlope(Trigger);
            SynchronizeTriggerLevel(Trigger);
        }

        private void SynchronizeTimebase()
        {
            SynchronizeTimebaseScale();
            SynchronizeTimebasePosition();
        }

        private void SynchronizeWaveform()
        {
            SynchronizeWaveformFormat();
            SynchronizeWaveformStreaming();
        }

        #region Partial synchronization methods
        private void SynchronizeChannelLabel(ChannelSettings channel)
        {
            string channelNumberString = channel.ChannelNumber.ToString(CultureInfo.InvariantCulture);
            string commandPart = Commands.ChannelLabelCommand;
            string oscilloscopeResponse = AskCommand(commandPart, channelNumberString);
            string removedQuotes = Regex.Replace(oscilloscopeResponse, $"\"+", string.Empty);
            channel.ChannelLabel = removedQuotes;
            AddResponseToConfig(commandPart, channelNumberString, oscilloscopeResponse);
        }
        private void SynchronizeChannelDisplay(ChannelSettings channel)
        {
            string channelNumberString = channel.ChannelNumber.ToString(CultureInfo.InvariantCulture);
            string commandPart = Commands.ChannelDisplayCommand;
            string oscilloscopeResponse = AskCommand(commandPart, channelNumberString);
            if (Commands.TrueFalseOptions.IndexOf(oscilloscopeResponse) % 2 == 0)
            {
                channel.ChannelDisplay = true;
            }
            AddResponseToConfig(commandPart, channelNumberString, oscilloscopeResponse);
        }
        private void SynchronizeChannelScale(ChannelSettings channel)
        {
            string channelNumberString = channel.ChannelNumber.ToString(CultureInfo.InvariantCulture);
            string commandPart = Commands.ChannelScaleCommand;
            string oscilloscopeResponse = AskCommand(commandPart, channelNumberString);
            _ = decimal.TryParse(oscilloscopeResponse, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal numberResult);
            channel.ChannelScale = numberResult;
            AddResponseToConfig(commandPart, channelNumberString, oscilloscopeResponse);
        }
        private void SynchronizeChannelOffset(ChannelSettings channel)
        {
            string channelNumberString = channel.ChannelNumber.ToString(CultureInfo.InvariantCulture);
            string commandPart = Commands.ChannelOffsetCommand;
            string oscilloscopeResponse = AskCommand(commandPart, channelNumberString);
            _ = decimal.TryParse(oscilloscopeResponse, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal numberResult);
            channel.ChannelOffset = numberResult;
            AddResponseToConfig(commandPart, channelNumberString, oscilloscopeResponse);
        }
        private void SynchronizeChannelCoupling(ChannelSettings channel)
        {
            string channelNumberString = channel.ChannelNumber.ToString(CultureInfo.InvariantCulture);
            string commandPart = Commands.ChannelCouplingCommand;
            string oscilloscopeResponse = AskCommand(commandPart, channelNumberString);
            channel.ChannelCouplingIndex = Commands.ChannelCouplingModes.IndexOf(oscilloscopeResponse);
            AddResponseToConfig(commandPart, channelNumberString, oscilloscopeResponse);
        }
        private void SynchronizeTriggerEdgeSource(TriggerSettings trigger)
        {

            string commandPart = Commands.TriggerEdgeSourceCommand;
            string oscilloscopeResponse = AskCommand(commandPart);
            trigger.TriggerEdgeSourceIndex = Commands.TriggerEdgeSourceOptions
                .Select(x => x = Regex.Replace(x, @"[a-z]+", string.Empty))
                .Select((item, index) => (item, index))
                .Where(x => x.item.Contains(oscilloscopeResponse))
                .Select(x => x.index)
                .FirstOrDefault();
            AddResponseToConfig(commandPart, Commands.TriggerEdgeSourceOptions.ElementAt(trigger.TriggerEdgeSourceIndex));
        }
        private void SynchronizeTriggerEdgeSlope(TriggerSettings trigger)
        {
            string commandPart = Commands.TriggerEdgeSlopeCommand;
            string oscilloscopeResponse = AskCommand(commandPart);
            trigger.TriggerEdgeSlopeIndex = Commands.TriggerEdgeSlopeOptions
                .Select(x => x = Regex.Replace(x, @"[a-z]+", string.Empty))
                .Select((item, index) => (item, index))
                .Where(x => x.item.Contains(oscilloscopeResponse))
                .Select(x => x.index)
                .FirstOrDefault();
            AddResponseToConfig(commandPart, Commands.TriggerEdgeSlopeOptions.ElementAt(trigger.TriggerEdgeSlopeIndex));
        }
        private void SynchronizeTriggerLevel(TriggerSettings trigger)
        {
            string commandPart = Commands.TriggerLevelCommand;
            //ToDo trigger:level? channel<x> !!!!!! FUCK ME!!
            string oscilloscopeResponse = AskCommand(commandPart);
            _ = decimal.TryParse(oscilloscopeResponse, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal numberResult);
            trigger.TriggerLevel = numberResult;
            AddResponseToConfig(commandPart, oscilloscopeResponse);
        }

        private void SynchronizeTimebaseScale()
        {
            string commandPart = Commands.TimebaseScaleCommand;
            string oscilloscopeResponse = AskCommand(commandPart);
            _ = decimal.TryParse(oscilloscopeResponse, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal numberResult);
            TimebaseScale = numberResult;
            AddResponseToConfig(commandPart, oscilloscopeResponse);
        }
        private void SynchronizeTimebasePosition()
        {
            string commandPart = Commands.TimebasePositionCommand;
            string oscilloscopeResponse = AskCommand(commandPart);
            _ = decimal.TryParse(oscilloscopeResponse, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal numberResult);
            TimebasePosition = numberResult;
            AddResponseToConfig(commandPart, oscilloscopeResponse);
        }
        private void SynchronizeWaveformFormat()
        {
            string commandPart = Commands.WaveformFormatCommand;
            string oscilloscopeResponse = AskCommand(commandPart);
            WaveformFormatIndex = Commands.WaveformFormatOptions
                .Select(x => x = Regex.Replace(x, @"[a-z]+", string.Empty))
                .Select((item, index) => (item, index))
                .Where(x => x.item.Contains(oscilloscopeResponse))
                .Select(x => x.index)
                .FirstOrDefault();
            AddResponseToConfig(commandPart, Commands.WaveformFormatOptions.ElementAt(WaveformFormatIndex));
        }
        private void SynchronizeWaveformStreaming()
        {
            string commandPart = Commands.WaveformStreamingCommand;
            string oscilloscopeResponse = AskCommand(commandPart);
            if (Commands.TrueFalseOptions.IndexOf(oscilloscopeResponse) % 2 == 0)
            {
                WaveformStreaming = true;
            }
            AddResponseToConfig(commandPart, oscilloscopeResponse);
        }
        #endregion

        private string AskCommand(string commandPart)
        {
            string askCommand = CommandList.UniversalAskCommandString(commandPart);
            return QueryData(askCommand);
        }
        private string AskCommand(string commandPart, string channelNumberString)
        {
            string askCommand = CommandList.UniversalAskCommandString(commandPart, channelNumberString);
            return QueryData(askCommand);
        }
        private void AddResponseToConfig(string commandPart, string oscilloscopeResponse)
        {
            string configPart = CommandList.UniversalCommandString(commandPart, oscilloscopeResponse).Item2;
            List<string> tempConfigString = new List<string>();
            tempConfigString.AddRange(OscilloscopeConfigString);
            tempConfigString.Add(configPart);
            OscilloscopeConfigString = tempConfigString.Distinct().ToList();
            OscilloscopeConfigString.Sort();
        }
        private void AddResponseToConfig(string commandPart, string channelNumberString, string oscilloscopeResponse)
        {
            string configPart = CommandList.UniversalCommandString(commandPart, channelNumberString, oscilloscopeResponse).Item2;
            List<string> tempConfigString = new List<string>();
            tempConfigString.AddRange(OscilloscopeConfigString);
            tempConfigString.Add(configPart);
            OscilloscopeConfigString = tempConfigString.Distinct().ToList();
            OscilloscopeConfigString.Sort();
        }
    }
}
