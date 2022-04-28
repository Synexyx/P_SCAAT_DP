using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace P_SCAAT.Models
{
    internal abstract partial class OscilloscopeConfig
    {
        #region Properties
        public List<string> OscilloscopeConfigString { get; protected set; }
        public List<ChannelSettings> Channels { get; protected set; }
        public TriggerSettings Trigger { get; protected set; }
        //public List<string> ChannelCouplingModes => Commands.ChannelCouplingModes ?? (new());
        public decimal TimebaseScale { get; protected set; }
        public int WaveformFormatIndex { get; protected set; }
        public List<string> WaveformFormatOptions { get; protected set; }
        //public string WaveformSource { get; private set; }
        public string WaveformStreaming { get; protected set; }


        public CommandList Commands { get; protected set; }

        #endregion
        public OscilloscopeConfig()
        {
            //Initialize();


            _ = CreateDefaultCommandList();


            //ListChannelSettings();
        }

        public void InitializeSettings(string oscilloscopeID)
        {
            OscilloscopeConfigString = new List<string>();
            Channels = new List<ChannelSettings>();
            Trigger = new TriggerSettings();

            //ToDo dynamic commandList loading


            //Commands = LoadCommandList("../../../OscilloscopeCommandLists/active.json");
            //string filePathName = FindCorrectCommandListFile(oscilloscopeID);
            Commands = LoadCommandList(oscilloscopeID);
            Trigger.TriggerEdgeSourceOptions = Commands.TriggerEdgeSourceOptions;
            Trigger.TriggerEdgeSlopeOptions = Commands.TriggerEdgeSlopeOptions;
            WaveformFormatOptions = Commands.WaveformFormatOptions;

            for (int i = 0; i < Commands.NumberOfAnalogChannels; i++)
            {
                ChannelSettings channel = new ChannelSettings(i + 1);
                Channels.Add(channel);
            }

        }

        private static async Task CreateDefaultCommandList()
        {
            CommandList commandList = new CommandList();
            //commandList.OscilloscopeSeriesID = "";
            //commandList.ChannelScaleCommand = ":CHANnel{0}:SCALe {1}";
            //commandList.ChannelDisplayCommand = ":CHANnel{0}:DISPlay {1}";
            //commandList.ChannelCouplingModes = new() { "AC", "DC" };
            //commandList.TrueFalseOptions = new() { "ON", "OFF" };

            string filePathName = "../../OscilloscopeCommandLists/default.json";
            JsonSerializerOptions options = new JsonSerializerOptions() { WriteIndented = true };

            using (FileStream createStream = File.Create(filePathName))
            {
                await JsonSerializer.SerializeAsync(createStream, commandList, options);
                createStream.Dispose();
            }
        }

        private static CommandList LoadCommandList(string oscilloscopeID)
        {
            string filePathName = FindCorrectCommandListFile(oscilloscopeID);
            try
            {
                string jsonString = File.ReadAllText(filePathName);
                CommandList commandList = JsonSerializer.Deserialize<CommandList>(jsonString);
                //commandList.UpdateDynamicOptions();
                return commandList;
            }
            catch
            {
                //ToDo dát správnou exception
                throw new NotImplementedException();
            }

        }
        private static string FindCorrectCommandListFile(string oscilloscopeID)
        {
            //string defaultFileDirectoryPath = "../../../OscilloscopeCommandLists/";
            string defaultFileDirectoryPath = "../../OscilloscopeCommandLists/";
            string commandListFilePathName = string.Empty;
            bool commandListFileFound = false;
            try
            {
                List<string> allFiles = Directory.GetFiles(defaultFileDirectoryPath).ToList();
                foreach (string filePath in allFiles)
                {
                    string fileContent = File.ReadAllText(filePath);
                    if (fileContent.Contains(oscilloscopeID))
                    {
                        commandListFilePathName = filePath;
                        commandListFileFound = true;
                        break;
                    }
                }
            }
            catch (Exception exp)
            {
                throw new FileLoadException(exp.Message);
            }
            if (!commandListFileFound)
            {
                //_ = CreateDefaultCommandList();
                commandListFilePathName = defaultFileDirectoryPath + "default.json";
            }
            return commandListFilePathName;
        }

        //CONFIGuration:ANALOg:NUMCHANnels?         VRÁTÍ POČET ANALOGOVÝCH KANÁLŮ
        //private string[] activeChannels;

        //public void LoadOscilloscopeSettings()
        //{
        //    throw new NotImplementedException();
        //}


        //PRO DVOJITÝ BINDIGN MEZI KONFIGURAČNÍM OKNEM A NASTAVENÍM SCALE KANÁLŮ

        //MOŽNÁ NAKONEC BUDU BINDOVAT PŘÍMO NA PROPERTU KANÁLU IDK YET
        //public void SetScaleForChannel(int channelNumber, float desiredValue)
        //{
        //    foreach (ChannelSettings channelSettings in Channels)
        //    {
        //        if (channelSettings.ChannelNumber.Equals(channelNumber))
        //        {
        //            Debug.WriteLine("MATCH " + channelSettings.ChannelNumber + ":" + channelSettings.ChannelScale);
        //            channelSettings.SetNewScale(desiredValue);
        //        }
        //        else
        //        {
        //            Debug.WriteLine("NO CHANNEL FOUND");
        //        }
        //    }
        //}

        public void InsertNewConfigString(List<string> oscilloscopeConfigString)
        {
            OscilloscopeConfigString.Clear();
            OscilloscopeConfigString.AddRange(oscilloscopeConfigString);
        }

        public void InsertNewChannelSettings(List<ChannelSettings> channels)
        {
            Channels.Clear();
            Channels.AddRange(channels);
            //foreach (ChannelSettings channel in channels)
            //{
            //    ChannelSettings tempChannel = new(channel);
            //    Channels.Add(tempChannel);
            //}
        }
        public void InsertTriggerSettings(TriggerSettings trigger)
        {
            Trigger = trigger;
        }
        public void InsertOtherSettings(decimal timebaseScale, int waveformFormatIndex, string waveformSource, string waveformStreaming)
        {
            TimebaseScale = timebaseScale;
            WaveformFormatIndex = waveformFormatIndex;
            //WaveformSource = waveformSource;
            WaveformStreaming = waveformStreaming;
        }

        public virtual void ClearAllData()
        {
            OscilloscopeConfigString = null;
            Channels = null;
            Trigger = null;
            Commands = null;
            TimebaseScale = 0;
            WaveformFormatIndex = 0;
            WaveformFormatOptions = null;
            //WaveformSource = null;
            WaveformStreaming = null;
        }




        //public void ListChannelSettings()
        //{
        //    //string commandTableTest = "Channel {0} number!";
        //    StringBuilder stringBuilder = new();

        //    //OsciloscopeConfigString.Clear();
        //    foreach (ChannelSettings channel in Channels)
        //    {
        //        Debug.WriteLine(Commands.ChannelScaleCommand);
        //        //_ = stringBuilder.AppendFormat(CultureInfo.InvariantCulture, commandTableTest, channel.ChannelNumber);
        //        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, Commands.ChannelScaleCommand, channel.ChannelNumber, channel.ChannelScale);
        //        Debug.WriteLine(stringBuilder);
        //        stringBuilder.Clear();
        //        //OsciloscopeConfigString.Add("Channel " + channel.ChannelNumber);
        //        //OsciloscopeConfigString.Add("Display " + channel.ChannelDisplay);
        //        //OsciloscopeConfigString.Add("Scale " + channel.ChannelScale.ToString("0.###E00", CultureInfo.InvariantCulture));
        //        //OsciloscopeConfigString.Add("Timebase " + channel.ChannelTimebase.ToString("0.###E00", CultureInfo.InvariantCulture));
        //        //OsciloscopeConfigString.Add("Coupling " + channel.ChannelCoupling);
        //    }
        //}

        //public void enableChannel(int channelNumber)
        //{
        //    //OsciloscopeConfigString.Add(":CHA");
        //}

    }


}
