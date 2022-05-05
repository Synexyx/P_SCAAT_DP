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
    internal abstract class OscilloscopeConfig
    {
        #region Properties
        public List<string> OscilloscopeConfigString { get; protected set; }
        public List<ChannelSettings> Channels { get; protected set; }
        public TriggerSettings Trigger { get; protected set; }
        public decimal TimebaseScale { get; protected set; }
        public decimal TimebasePosition { get; protected set; }
        public int WaveformFormatIndex { get; protected set; }
        private List<string> _waveformFormatOptions;
        public List<string> WaveformFormatOptions { get => _waveformFormatOptions ?? new List<string>(); protected set => _waveformFormatOptions = value; }
        private List<string> _waveformSourceOptions;
        public List<string> WaveformSourceOptions { get => _waveformSourceOptions ?? new List<string>(); protected set => _waveformSourceOptions = value; }
        public bool WaveformStreaming { get; protected set; }

        public CommandList Commands { get; protected set; }
        #endregion

        public OscilloscopeConfig()
        {
            _ = CreateDefaultCommandList();
        }
        /// <summary>
        /// Initialize all oscilloscope settings. Initializes default settings for <see cref="Trigger"/> and <see cref="Channels"/>. Calls <see cref="LoadCommandList(string)"/> 
        /// to get correct <see cref="CommandList"/> for oscilloscope using <paramref name="oscilloscopeID"/> as identifier.
        /// </summary>
        /// <param name="oscilloscopeID"></param>
        public void InitializeSettings(string oscilloscopeID)
        {
            OscilloscopeConfigString = new List<string>();
            Channels = new List<ChannelSettings>();
            Trigger = new TriggerSettings();

            Commands = LoadCommandList(oscilloscopeID);

            Trigger.TriggerEdgeSourceOptions = Commands.TriggerEdgeSourceOptions;
            Trigger.TriggerEdgeSlopeOptions = Commands.TriggerEdgeSlopeOptions;
            foreach (string source in Trigger.TriggerEdgeSourceOptions)
            {
                Trigger.TriggerLevel.Add(source, 0);
            }
            WaveformFormatOptions = Commands.WaveformFormatOptions;
            WaveformSourceOptions = Commands.WaveformSourceOptions;

            for (int i = 0; i < Commands.NumberOfAnalogChannels; i++)
            {
                ChannelSettings channel = new ChannelSettings(i + 1)
                {
                    ChannelCouplingModes = Commands.ChannelCouplingModes
                };
                Channels.Add(channel);
            }
        }

        /// <summary>
        /// Creates properly formated default empty <see cref="CommandList"/> file in <c>/OscilloscopeCommandLists/</c> folder for easy editation and customization.
        /// </summary>
        /// <returns></returns>
        private static async Task CreateDefaultCommandList()
        {
            CommandList commandList = new CommandList();

            string filePathName = "../../OscilloscopeCommandLists/default.json";
            JsonSerializerOptions options = new JsonSerializerOptions() { WriteIndented = true };
            using (FileStream createStream = File.Create(filePathName))
            {
                await JsonSerializer.SerializeAsync(createStream, commandList, options);
            }
        }

        /// <summary>
        /// Finds and loads <see cref="CommandList"/> file and creates new <see cref="CommandList"/> object.
        /// </summary>
        private static CommandList LoadCommandList(string oscilloscopeID)
        {
            string filePathName = FindCorrectCommandListFile(oscilloscopeID);
            string jsonString = File.ReadAllText(filePathName);
            CommandList commandList = JsonSerializer.Deserialize<CommandList>(jsonString);
            return commandList;
        }

        /// <summary>
        /// Get path of correct <see cref="CommandList"/> file which contain <paramref name="oscilloscopeID"/> inside it. If no such file is found, use the default file.
        /// </summary>
        private static string FindCorrectCommandListFile(string oscilloscopeID)
        {
            string defaultFileDirectoryPath = "../../OscilloscopeCommandLists/";
            string commandListFilePathName = string.Empty;
            bool commandListFileFound = false;

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
            if (!commandListFileFound)
            {
                commandListFilePathName = defaultFileDirectoryPath + "default.json";
            }
            return commandListFilePathName;
        }

        /// <summary>
        /// Replace current <see cref="OscilloscopeConfigString"/> with <paramref name="oscilloscopeConfigString"/>.
        /// </summary>
        public void InsertNewConfigString(List<string> oscilloscopeConfigString)
        {
            OscilloscopeConfigString.Clear();
            OscilloscopeConfigString.AddRange(oscilloscopeConfigString);
        }
        /// <summary>
        /// Replace current <see cref="Channels"/> with <paramref name="channels"/>.
        /// </summary>
        public void InsertNewChannelSettings(List<ChannelSettings> channels)
        {
            Channels.Clear();
            Channels.AddRange(channels);
        }
        /// <summary>
        /// Replace current <see cref="Trigger"/> with <paramref name="trigger"/>.
        /// </summary>
        public void InsertTriggerSettings(TriggerSettings trigger)
        {
            Trigger = trigger;
        }
        /// <summary>
        /// Replace all other settings not yet replaced by previous methods.
        /// </summary>
        public void InsertOtherSettings(decimal timebaseScale, decimal timebasePosition, int waveformFormatIndex, bool waveformStreaming)
        {
            TimebaseScale = timebaseScale;
            TimebasePosition = timebasePosition;
            WaveformFormatIndex = waveformFormatIndex;
            WaveformStreaming = waveformStreaming;
        }

        /// <summary>
        /// Clear all currently used data. Used when session is closed.
        /// </summary>
        public virtual void ClearAllData()
        {
            OscilloscopeConfigString = null;
            Channels = null;
            Trigger = null;
            Commands = null;
            TimebaseScale = 0;
            WaveformFormatIndex = 0;
            WaveformFormatOptions = null;
            WaveformSourceOptions = null;
            WaveformStreaming = false;
            _waveformFormatOptions = null;
            _waveformSourceOptions = null;
        }

    }


}
