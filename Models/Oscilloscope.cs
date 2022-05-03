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

        //public OsciloscopeConfig OsciloscopeConfig
        //{
        //    get;
        //    private set;
        //}
        #endregion

        //private OsciloscopeConfig osciloscopeConfig = new OsciloscopeConfig();    MĚLO BY BÝT AŽ PO NAVÁZÁNÍ SPOJENÍ S OSCILOSKOPEM
        //public Osciloscope() : base()
        //{
        //}
        //public Osciloscope(string sessionName) : base()
        //{
        //    SessionName = sessionName;
        //    //OsciloscopeConfig = new OsciloscopeConfig();
        //}

        #region StaticGetOscilloscopeList
        /// <summary>
        /// Static method for getting list of resources (osciloscopes) available for session
        /// </summary>
        /// <returns>List of available osciloscopes</returns>
        public static List<string> GetOscilloscopeList()
        {
            List<string> rsList = new List<string>();
            try
            {
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
            catch (Exception e)
            {
                throw new Exception(e.Message);
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
                catch (Exception exp)
                {
                    throw new SessionControlException($"Oscilloscope session cannot be estabilished!{Environment.NewLine}REASON :{exp.GetType()}{Environment.NewLine}{exp.Message}");

                    //_ = MessageBox.Show($"Session failed to open!{Environment.NewLine}{exp.Message}", "Session ERROR!", MessageBoxButton.OK, MessageBoxImage.Error);
                    //Debug.WriteLine("Session: " + SessionName + " FAILED to open.");
                }
            }
        }

        public void InitializeSessionSettings()
        {
            //ToDo 
            //string oscilloscopeID = QueryData("*IDN?");
            string oscilloscopeID = "WORD";
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

        //ToDo MEASURE LOOP - new TASK ve kterém pak cyklovat zvolené Waveform sources
        //CancellationTokenSource tokenSource = new CancellationTokenSource();

        //ToDo asi tady metoda StartMeassure
        //Předat zprávu + seznam kanálů, na kterých se bude měřit
        //internal async Task<string> Measure(CryptoDeviceMessage cryptoDeviceMessage, int messageLenght, CancellationToken token)
        internal void MeasurePrep()
        {
            bool isReady = false;
            SendData(Commands.OscilloscopeSingleAcquisitionCommand);
            while (!isReady)
            {
                Thread.Sleep(100);
                isReady = QueryData(Commands.OscilloscopeOperationCompleteCommand).Contains("1");
            }
        }

        internal byte[] GetMeasuredData()
        {
            //MessageBasedSession.FormattedIO.DiscardBuffers();
            //MessageBasedSession.RawIO.ReadString();
            //bool isReady = false;
            //while (!isReady)
            //{
            //    isReady = QueryData(Commands.OscilloscopeOperationCompleteCommand).Contains("1");
            //}
            //Thread.Sleep(100);
            SendData(Commands.WaveformDataCommand);
            MemoryStream memoryStream = new MemoryStream();
            //MessageBasedSession.RawIO.Write(Commands.WaveformDataCommand);
            while (true)
            {
                try
                {
                    //var test1 = MessageBasedSession.FormattedIO.ReadUntilEnd();
                    byte[] response = MessageBasedSession.RawIO.Read();
                    //byte[] response = MessageBasedSession.FormattedIO.ReadLineListOfByte();
                    //byte[] response = MessageBasedSession.FormattedIO.ReadListOfByte();
                    memoryStream.Write(response, 0, response.Length);
                }
                catch (Exception)
                {
                    Debug.WriteLine("Reading finished");
                    break;
                }
            }
            //var test = MessageBasedSession.RawIO.Read();
            var bitconv = BitConverter.ToString(memoryStream.GetBuffer());
            var utfString = Encoding.UTF8.GetString(memoryStream.GetBuffer());
            var asciiString = Encoding.ASCII.GetString(memoryStream.ToArray());
            var base64String = Convert.ToBase64String(memoryStream.ToArray());
            //var test2 = MessageBasedSession.RawIO.Read();
            byte[] result = memoryStream.GetBuffer();
            int lastIndex = Array.FindLastIndex(result, b => b != 0);
            Array.Resize(ref result, lastIndex + 1);
            //return memoryStream.GetBuffer();
            return result;
        }

        internal string Measure(CryptoDeviceMessage cryptoDeviceMessage, uint messageLenght, CancellationToken token)
        {
            while (true)
            {
                Debug.WriteLine("EXEC");
                Debug.WriteLine(DateTime.Now);
                Thread.Sleep(50);
                //token.ThrowIfCancellationRequested();
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                    return "H";
                }
            }
            //await Task.Run(() =>
            //{
            //    Thread.Sleep(50000);
            //    cryptoDeviceMessage.InitializeRNGMessageGenerator(messageLenght);
            //    cryptoDeviceMessage.GenerateNewMessage();
            //});
            //return "G";
        }

        internal void ListCurrentCommands()
        {
            foreach (string item in OscilloscopeConfigString)
            {
                Debug.WriteLine("FINAL " + item);
            }
        }
        //public void ApplyConfigString()
        //{
        //    Debug.WriteLine(OsciloscopeConfigString.Count);
        //}

        internal void ApplyAllSettingsToDevice()
        {
            foreach (string setting in OscilloscopeConfigString)
            {
                SendData(setting);
            }
        }

        public void SendData(string dataString)
        {
            //*IDN ?\n
            try
            {
                //    string messageForge = ReplaceEscapeSeq(dataString);
                //    MessageBasedSession.RawIO.Write(messageForge);
                MessageBasedSession.RawIO.Write(dataString);
                //MessageBasedSession.FormattedIO.WriteLine(messageForge);
                //MessageBasedSession.FormattedIO.Write(messageForge);
                //Debug.WriteLine(MessageBasedSession.RawIO.ReadString());
            }
            catch (Exception exp)
            {
                throw new SessionCommunicationException($"Sending data to the oscilloscope failed!{Environment.NewLine}REASON :{exp.GetType()}{Environment.NewLine}{exp.Message}");
            }
        }
        public string QueryData(string dataString)
        {
            string resultData;
            try
            {
                SendData(dataString);
                resultData = ReadData();
            }
            catch (SessionCommunicationException sessionComExp)
            {
                throw new SessionCommunicationException($"Data query to the oscilloscope failed!{Environment.NewLine}REASON :{sessionComExp.GetType()}{Environment.NewLine}{sessionComExp.Message}");
            }
            return resultData;
            //try
            //{
            //    string oscilloscopeReply = MessageBasedSession.RawIO.ReadString();

            //    return InsertEscapeSeq(oscilloscopeReply);
            //}
            //catch (Exception exp)
            //{
            //    _ = MessageBox.Show($"Query data to the device failed!{Environment.NewLine}{exp.Message}", "Data query ERROR!", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
            //return string.Empty;
        }

        public string ReadWaveformData()
        {
            //MessageBasedSession.RawIO.AbortAsyncOperation();
            return string.Empty;
        }

        public string ReadData()
        {
            try
            {
                //ToDo somehow fix this
                //MessageBasedSession t;
                //string receivedMessage = MessageBasedSession.RawIO.ReadString(10000000);
                //string receivedMessage = string.Empty;
                //MemoryStream memorystream = new MemoryStream();
                //for (int i = 0; i < 10; i++)
                //{

                //var test = MessageBasedSession.FormattedIO.ReadBinaryBlockOfByte();
                //var test = MessageBasedSession.FormattedIO.ReadInt64();
                //MessageBasedSession.TimeoutMilliseconds = 5000;
                //var time = MessageBasedSession.TimeoutMilliseconds;
                string response = MessageBasedSession.FormattedIO.ReadUntilEnd();
                //string response = MessageBasedSession.RawIO.ReadString();
                //memorystream.Write(response, 0, response.Length);
                //}
                //var receivedMessage2 = MessageBasedSession.RawIO.BeginRead(0);
                return response;
                //return InsertEscapeSeq(response);
            }
            catch (Exception exp)
            {
                throw new SessionCommunicationException($"Reading data from the oscilloscope failed!{Environment.NewLine}REASON :{exp.GetType()}{Environment.NewLine}{exp.Message}");
            }
        }

        private static string ReplaceEscapeSeq(string message)
        {
            return message.Replace("\\n", "\n").Replace("\\r", "\r");
        }

        private static string InsertEscapeSeq(string message)
        {
            //return message.Replace("\n", "\\n").Replace("\r", "\\r");
            return message.Replace("\n", string.Empty).Replace("\r", string.Empty);

        }

        public override void ClearAllData()
        {
            SessionName = null;
            IsSessionOpen = false;
            base.ClearAllData();
        }

        public void SynchronizeConfig()
        {
            //Thread.Sleep(5000);

            //List<string> tempConfigString = new List<string>(OscilloscopeConfigString);
            //OscilloscopeConfigString.Clear();
            //ToDo should not do that cause of custom settings

            SynchronizeChannels();
            SynchronizeTrigger();

            //SynchronizeTimebase();



        }
        private void SynchronizeChannels()
        {
            foreach (ChannelSettings channel in Channels)
            {
                //ToDo better exception handling

                ////ChannelLabel
                SynchronizeChannelLabel(channel);

                ////ChannelDisplay
                SynchronizeChannelDisplay(channel);

                ////ChannelScale
                SynchronizeChannelScale(channel);

                ////ChannelOffset
                SynchronizeChannelOffset(channel);

                ////ChannelCoupling
                SynchronizeChannelCoupling(channel);
            }
        }
        private void SynchronizeTrigger()
        {
            //ToDo better exception handling
            //ToDo use inner exception
            //ToDo mayby add ErrorMessage to OscilloscopeConfigView

            ////TriggerEdgeSource
            SynchronizeTriggerEdgeSource(Trigger);

            ////TriggerEdgeSlope
            SynchronizeTriggerEdgeSlope(Trigger);

            ////TriggerLevel
            //SynchronizeTriggerLevel(Trigger);
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
            try
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
            catch (Exception exp)
            {
                Debug.WriteLine(exp.Message);
            }
        }
        private void SynchronizeTriggerEdgeSlope(TriggerSettings trigger)
        {
            try
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
            catch (Exception exp)
            {
                Debug.WriteLine(exp.Message);
            }
        }
        private void SynchronizeTriggerLevel(TriggerSettings trigger)
        {
            try
            {
                string commandPart = Commands.TriggerLevelCommand;
                //ToDo trigger:level? channel<x> !!!!!! FUCK ME!!
                string oscilloscopeResponse = AskCommand(commandPart);
                _ = decimal.TryParse(oscilloscopeResponse, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal numberResult);
                trigger.TriggerLevel = numberResult;
                AddResponseToConfig(commandPart, oscilloscopeResponse);
            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp.Message);
            }
        }
        #endregion

        private string AskCommand(string commandPart)
        {
            try
            {
                string askCommand = CommandList.UniversalAskCommandString(commandPart);
                //ToDo just testing
                return QueryData(askCommand);
                //return TESTQUERY6(askCommand);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private string AskCommand(string commandPart, string channelNumberString)
        {
            string askCommand = CommandList.UniversalAskCommandString(commandPart, channelNumberString);
            //ToDo just testing
            return QueryData(askCommand);
            //return TESTQUERY5(askCommand);
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

        public string TESTQUERY(string dataToSent)
        {
            Debug.WriteLine("SENDING " + dataToSent);
            return "1\\n".Replace("\\n", string.Empty);
        }
        public string TESTQUERY2(string dataToSent)
        {
            Debug.WriteLine("SENDING " + dataToSent);
            return "250E-03\\n".Replace("\\n", string.Empty);
        }
        public string TESTQUERY3(string dataToSent)
        {
            Debug.WriteLine("SENDING " + dataToSent);
            return "\"channelLabel1\"";
        }
        public string TESTQUERY4(string dataToSent)
        {
            Debug.WriteLine("SENDING " + dataToSent);
            return "1";
        }
        public string TESTQUERY5(string dataToSent)
        {
            Debug.WriteLine("SENDING " + dataToSent);
            return "DCC";
        }
        public string TESTQUERY6(string dataToSent)
        {
            Debug.WriteLine("SENDING " + dataToSent);
            return "CHAN2";
        }
    }
}
