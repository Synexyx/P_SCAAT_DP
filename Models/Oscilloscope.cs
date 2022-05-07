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
        public string SessionName { get; private set; }
        public MessageBasedSession MessageBasedSession { get; private set; }
        public bool IsSessionOpen { get; private set; }
        #endregion


        #region StaticGetOscilloscopeList
        /// <summary>
        /// Static method for getting name of resources (oscilloscopes) available for session.
        /// </summary>
        /// <returns><see cref="List{T}"/> of available oscilloscopes</returns>
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

        /// <summary>
        /// Open and initialize oscilloscope session with <paramref name="sessionName"/>
        /// </summary>
        /// <exception cref="SessionControlException"/>
        public void OpenSession(string sessionName)
        {
            SessionName = sessionName;
            Debug.WriteLine("Openning session: " + SessionName);
            using (ResourceManager rmSession = new ResourceManager())
            {
                try
                {
                    MessageBasedSession = (MessageBasedSession)rmSession.Open(SessionName);
                    MessageBasedSession.TimeoutMilliseconds = 5000;
                    IsSessionOpen = true;
                    InitializeSessionSettings();
                    Debug.WriteLine("Session: " + SessionName + " succesfully opened.");
                }
                catch (Exception ex)
                {
                    CloseSession();
                    throw new SessionControlException($"Oscilloscope session cannot be estabilished!{Environment.NewLine}REASON :{ex.GetType()}{Environment.NewLine}{ex.Message}", ex);
                }
            }
        }
        /// <summary>
        /// Gets <paramref name="oscilloscopeID"/> as ID string from the oscilloscope. Which will be used to identify correct <see cref="CommandList"/> file.<br/>
        /// Initialization WILL NOT synchronize this settings with real device to prevent unnecessary communication with the device in case of not wanting to change configuration of the real device.<br/>
        /// For synchronization see <see cref="SynchronizeConfig"/>
        /// </summary>
        private void InitializeSessionSettings()
        {
            //string oscilloscopeID = QueryData("*IDN?");
            string oscilloscopeID = "MSO9104A";
            InitializeSettings(oscilloscopeID);
        }

        /// <summary>
        /// Close and clear data used in the oscilloscope session
        /// </summary>
        public void CloseSession()
        {
            if (MessageBasedSession != null)
            {
                MessageBasedSession.Dispose();
            }
            ClearAllData();
            Debug.WriteLine("Session succesfully closed.");
        }

        /// <summary>
        /// Set oscilloscope to standby mode using <paramref name="OscilloscopeSingleAcquisitionCommand"/>
        /// </summary>
        public void MeasurePrep()
        {
            bool isReady = false;
            SendData(Commands.OscilloscopeSingleAcquisitionCommand);
            while (!isReady)
            {
                Thread.Sleep(100);
                isReady = QueryData(Commands.OscilloscopeOperationCompleteCommand).Contains("1");
            }
        }
        public string GetCurrentWaveformFormat()
        {
            string currentWaveformFormat = AskCommand(Commands.WaveformFormatCommand);
            int waveformFormatIndex = Commands.WaveformFormatOptions
                .Select(x => x = Regex.Replace(x, @"[a-z]+", string.Empty))
                .Select((item, index) => (item, index))
                .Where(x => x.item.Contains(currentWaveformFormat))
                .Select(x => x.index)
                .FirstOrDefault();
            string waveformFormatOption = Commands.WaveformFormatOptions.ElementAtOrDefault(waveformFormatIndex);

            return waveformFormatOption;
        }
        public void ChangeWaveformSource(string selectedSource)
        {
            string switchCorrectSource = CommandList.UniversalCommandString(Commands.WaveformSourceCommand, selectedSource).Item2;
            SendData(switchCorrectSource);
        }

        /// <summary>
        /// Get all waveform data from the oscilloscope. Way of getting data is selected by selected waveform format. If acquisition via that format fails, retry with universal read byte 
        /// (<see cref="GetWaveformDataUniversal"/>)<br/>
        /// Order of formats in <see cref="CommandList.WaveformFormatOptions"/> as follows <c>ASCii</c>, <c>BINary</c>, <c>BYTE</c>, <c>WORD</c><br/>
        /// Reference <see href="https://www.testworld.com/wp-content/uploads/keysight-agilent-infiniium-oscilloscopes-programmers-guide.pdf">Programmers guide</see> pg. 1167
        /// </summary>
        public byte[] GetWaveformData(string selectedFormat)
        {
            ///===================================== OPTIONAL TO WAIT FOR TRIGGER EVENT
            //bool triggerEvent = false;
            //while (!triggerEvent)
            //{
            //    Thread.Sleep(10);
            //    triggerEvent = QueryData(Commands.OscilloscopeTriggerEventCommand).Contains("1");
            //}
            ///=====================================
            //var watch = new Stopwatch();
            //watch.Start();

            SendData(Commands.WaveformDataCommand);
            Thread.Sleep(10);

            //watch.Stop();
            //Debug.WriteLine($"Request to get data {watch.ElapsedMilliseconds}");
            //watch.Restart();

            byte[] response = new byte[0];


            ///====== Wanted to do switch statement, but current version of C# is not supporting dynamic comparison. Only constants.
            if (selectedFormat.Equals(Commands.WaveformFormatOptions.ElementAtOrDefault(0), StringComparison.OrdinalIgnoreCase)) //ASCII
            {
                try
                {
                    string stringResponse = MessageBasedSession.FormattedIO.ReadUntilEnd();
                    response = Encoding.ASCII.GetBytes(stringResponse);
                }
                catch { response = GetWaveformDataUniversal(); }
            }
            else if (selectedFormat.Equals(Commands.WaveformFormatOptions.ElementAtOrDefault(1), StringComparison.OrdinalIgnoreCase)) //BINARY
            {
                try
                {
                    long[] binaryResponse = MessageBasedSession.FormattedIO.ReadLineBinaryBlockOfInt64();
                    response = new byte[binaryResponse.Length * 8];
                    Buffer.BlockCopy(binaryResponse, 0, response, 0, binaryResponse.Length * 8);
                }
                catch { response = GetWaveformDataUniversal(); }
            }
            else if (selectedFormat.Equals(Commands.WaveformFormatOptions.ElementAtOrDefault(2), StringComparison.OrdinalIgnoreCase)) //BYTE
            {
                try
                {
                    response = MessageBasedSession.FormattedIO.ReadLineBinaryBlockOfByte();
                }
                catch { response = GetWaveformDataUniversal(); }
            }
            else if (selectedFormat.Equals(Commands.WaveformFormatOptions.ElementAtOrDefault(3), StringComparison.OrdinalIgnoreCase)) //WORD
            {
                try
                {
                    short[] wordResponse = MessageBasedSession.FormattedIO.ReadLineBinaryBlockOfInt16();
                    response = new byte[wordResponse.Length * 2];
                    Buffer.BlockCopy(wordResponse, 0, response, 0, wordResponse.Length * 2);
                }
                catch { response = GetWaveformDataUniversal(); }
            }
            else
            {
                response = GetWaveformDataUniversal();
            }


            if (response.Length == 0)
            {
                throw new SessionCommunicationException($"NO DATA ACQUIRED FROM THE DEVICE{Environment.NewLine}");
            }

            //watch.Stop();
            //Debug.WriteLine($"Data acq. complete {watch.ElapsedMilliseconds}");

            int lastIndex = Array.FindLastIndex(response, byteValue => byteValue != 0);
            Array.Resize(ref response, lastIndex + 1);
            return response;
        }

        private byte[] GetWaveformDataUniversal()
        {
            byte[] response = new byte[0];
            MemoryStream memoryStream = new MemoryStream();
            while (true)
            {
                try
                {
                    response = MessageBasedSession.RawIO.Read();
                    memoryStream.Write(response, 0, response.Length);
                }
                catch (Exception)
                {
                    Debug.WriteLine($"Reading finished");
                    break;
                }
            }
            return response;
        }

        /// <summary>
        /// Send all commands in <see cref="OscilloscopeConfig"/> into the oscilloscope.
        /// </summary>
        public void ApplyAllSettingsToDevice()
        {
            foreach (string setting in OscilloscopeConfigString)
            {
                SendData(setting);
            }
        }

        /// <summary>
        /// Send data defined in <paramref name="dataString"/> to the oscilloscope.
        /// </summary>
        /// <param name="dataString"></param>
        /// <exception cref="SessionCommunicationException"/>
        public void SendData(string dataString)
        {
            Thread.Sleep(10);
            try
            {
                MessageBasedSession.RawIO.Write(dataString);
            }
            catch (Exception ex)
            {
                throw new SessionCommunicationException($"Sending data to the oscilloscope failed!{Environment.NewLine}REASON :{ex.GetType()}{Environment.NewLine}{ex.StackTrace}");
            }
        }
        /// <summary>
        /// Send data defined in <paramref name="dataString"/> to the oscilloscope and gets response.
        /// </summary>
        /// <param name="dataString"></param>
        /// <exception cref="SessionCommunicationException"/>
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
        /// <summary>
        /// Read all string data sent by oscilloscope.
        /// </summary>
        /// <exception cref="SessionCommunicationException"/>
        public string ReadStringData()
        {
            try
            {
                string response = MessageBasedSession.FormattedIO.ReadUntilEnd();
                return response.Replace("\n", string.Empty);
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


        /// <summary>
        /// Clear all currently used data. Used when session is closed.
        /// </summary>
        protected override void ClearAllData()
        {
            SessionName = null;
            IsSessionOpen = false;
            base.ClearAllData();
        }

        /// <summary>
        /// Synchronize all settings that can be changed via application with real device.
        /// </summary>
        public void SynchronizeConfig()
        {
            SynchronizeChannels();
            SynchronizeTrigger();
            SynchronizeTimebase();
            SynchronizeWaveform();
        }


        //Methods below should be self-explanatory.
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

        #region Individual synchronization methods
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
            _ = decimal.TryParse(oscilloscopeResponse, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal numberResult);
            channel.ChannelScale = numberResult;
            AddResponseToConfig(commandPart, channelNumberString, oscilloscopeResponse);
        }
        private void SynchronizeChannelOffset(ChannelSettings channel)
        {
            string channelNumberString = channel.ChannelNumber.ToString(CultureInfo.InvariantCulture);
            string commandPart = Commands.ChannelOffsetCommand;
            string oscilloscopeResponse = AskCommand(commandPart, channelNumberString);
            _ = decimal.TryParse(oscilloscopeResponse, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal numberResult);
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
            foreach (string source in Commands.TriggerEdgeSourceOptions)
            {
                string oscilloscopeResponse = SourceDependantAskCommand(commandPart, source);
                _ = decimal.TryParse(oscilloscopeResponse, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal numberResult);
                if (trigger.TriggerLevel.ContainsKey(source))
                {
                    trigger.TriggerLevel[source] = numberResult;
                }
                else
                {
                    trigger.TriggerLevel.Add(source, numberResult);
                }
                AddResponseToConfig(commandPart, source, oscilloscopeResponse);
            }
        }

        private void SynchronizeTimebaseScale()
        {
            string commandPart = Commands.TimebaseScaleCommand;
            string oscilloscopeResponse = AskCommand(commandPart);
            _ = decimal.TryParse(oscilloscopeResponse, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal numberResult);
            TimebaseScale = numberResult;
            AddResponseToConfig(commandPart, oscilloscopeResponse);
        }
        private void SynchronizeTimebasePosition()
        {
            string commandPart = Commands.TimebasePositionCommand;
            string oscilloscopeResponse = AskCommand(commandPart);
            _ = decimal.TryParse(oscilloscopeResponse, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal numberResult);
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
        private string AskCommand(string commandPart, string commandParameter1)
        {
            string askCommand = CommandList.UniversalAskCommandString(commandPart, commandParameter1);
            return QueryData(askCommand);
        }
        private string SourceDependantAskCommand(string commandPart, string source)
        {
            string askCommand = CommandList.SourceDependantAskCommandString(commandPart, source);
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
        private void AddResponseToConfig(string commandPart, string commandParameter1, string oscilloscopeResponse)
        {
            string configPart = CommandList.UniversalCommandString(commandPart, commandParameter1, oscilloscopeResponse).Item2;
            List<string> tempConfigString = new List<string>();
            tempConfigString.AddRange(OscilloscopeConfigString);
            tempConfigString.Add(configPart);
            OscilloscopeConfigString = tempConfigString.Distinct().ToList();
            OscilloscopeConfigString.Sort();
        }
    }
}
