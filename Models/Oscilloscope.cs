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
                    //MessageBasedSession = (MessageBasedSession)rmSession.Open(SessionName);
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
        internal string Measure(CryptoDeviceMessage cryptoDeviceMessage, int messageLenght, CancellationToken token)
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
                string messageForge = ReplaceEscapeSeq(dataString);
                MessageBasedSession.RawIO.Write(messageForge);
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
                string receivedMessage = MessageBasedSession.RawIO.ReadString(10000000);
                //var receivedMessage2 = MessageBasedSession.RawIO.BeginRead(0);
                return InsertEscapeSeq(receivedMessage);
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
            return message.Replace("\n", "").Replace("\r", "");

        }

        public override void ClearAllData()
        {
            SessionName = null;
            IsSessionOpen = false;
            base.ClearAllData();
        }

        public void UpdateAllResources()
        {
            //Thread.Sleep(5000);

            List<string> tempConfigString = new List<string>(OscilloscopeConfigString);
            //OscilloscopeConfigString.Clear(); 
            //ToDo not sure if will work properly

            //TEST
            //WaveformFormatIndex = WaveformFormatOptions.IndexOf("WORD");
            //Trigger.TriggerEdgeSourceIndex = 2;
            //Trigger.TriggerEdgeSlopeSelectedIndex = 1;

            string oscilloscopeResponse = string.Empty;
            //ToDo UPDATE CONFIG STRING!!

            //ToDo prvně načíst config string a z něj pak nastavení tříd??
            foreach (ChannelSettings channel in Channels)
            {

                ////ChannelLabel
                //oscilloscopeResponse = TESTQUERY(Commands.ChannelLabelAskCommandString(channel.ChannelNumber));
                //channel.ChannelLabel = oscilloscopeResponse;

                ////ChannelDisplay
                ////Debug.WriteLine(channel.ChannelDisplay);//
                //oscilloscopeResponse = TESTQUERY(Commands.ChannelDisplayAskCommandString(channel.ChannelNumber));
                ////Debug.WriteLine(Commands.TrueFalseOptions.IndexOf(oscilloscopeResponse));//
                //if (Commands.TrueFalseOptions.IndexOf(oscilloscopeResponse) % 2 != 0)
                //{
                //    channel.ChannelDisplay = true;
                //    //tempConfigString.Add(Commands.ChannelDisplayCommandString(channel.ChannelNumber, channel.ChannelDisplay).Item2);
                //}
                //string displayToString = Commands.TrueFalseOptions != null
                //    ? channel.ChannelDisplay ? Commands.TrueFalseOptions.ElementAtOrDefault(0) ?? "1" : Commands.TrueFalseOptions.ElementAtOrDefault(1) ?? "0"
                //    : channel.ChannelDisplay ? "ON" : "OFF";
                //tempConfigString.Add(CommandList.UniversalCommandString(Commands.ChannelDisplayCommand, channel.ChannelNumber.ToString(CultureInfo.InvariantCulture), displayToString).Item2);
                ////channel.ChannelDisplay = Convert.ToBoolean(oscilloscopeResponse.Replace("\\n", ""), CultureInfo.InvariantCulture);

                ////Debug.WriteLine(channel.ChannelDisplay);//

                //ChannelScale
                //string forgeCommand = CommandList.UniversalCommandString(Commands.ChannelScaleCommand, channel.ChannelNumber.ToString(), "?").Item2.Replace(" ", "");
                //string forgeCommand = CommandList.UniversalAskCommandString(Commands.ChannelScaleCommand); //ToDo need chNumber
                //oscilloscopeResponse = TESTQUERY2(CommandList.UniversalCommandString(Commands.ChannelScaleCommand, channel.ChannelNumber.ToString(CultureInfo.InvariantCulture), "?").Item2);

                //oscilloscopeResponse = QueryData(forgeCommand).Replace("\\n", "");
                _ = decimal.TryParse(oscilloscopeResponse, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal numberResult);
                channel.ChannelScale = numberResult;

                //ChannelPosition
                //ChannelOffset
                //ChannelCouplingModes
                //ChannelCouplingSelected

                OscilloscopeConfigString = tempConfigString.Distinct().ToList();
            }
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
    }
}
