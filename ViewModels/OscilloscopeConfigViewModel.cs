using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using P_SCAAT.Models;
using P_SCAAT.ViewModels.Commands;
using P_SCAAT.ViewModels.ViewControlState;
using static P_SCAAT.Models.OscilloscopeConfig;

namespace P_SCAAT.ViewModels
{
    internal class OscilloscopeConfigViewModel : CorePropChangedVM
    {
        #region Properties
        private Oscilloscope _oscilloscope;
        private List<string> _tempOscilloscopeConfigString;
        private ObservableCollection<ChannelSettingsViewModel> _tempChannels;
        private decimal _timebaseScale;
        private decimal _timebasePosition;
        private TriggerViewModel _triggerVM;
        private List<string> _waveformFormatOptions;
        private int _waveformFormatIndex;
        private bool _waveformStreaming;
        public Oscilloscope Oscilloscope
        {
            get => _oscilloscope;
            private set { _oscilloscope = value; OnPropertyChanged(nameof(Oscilloscope)); }
        }
        public List<string> TempOscilloscopeConfigString
        {
            get => _tempOscilloscopeConfigString;
            set { _tempOscilloscopeConfigString = value; OnPropertyChanged(nameof(TempOscilloscopeConfigString)); }
        }
        public ObservableCollection<ChannelSettingsViewModel> TempChannels
        {
            get => _tempChannels;
            set { _tempChannels = value; OnPropertyChanged(nameof(TempChannels)); }
        }

        //====== TIMEBASE ======

        public decimal TimebaseScale
        {
            get => _timebaseScale;
            set
            {
                _timebaseScale = value;
                OnPropertyChanged(nameof(TimebaseScale));
                (string, string) commandParts = CommandList.UniversalCommandString(Oscilloscope.Commands.TimebaseScaleCommand,
                    TimebaseScale.ToString("0.###E00", CultureInfo.InvariantCulture));
                ApplyCommandToConfigString(commandParts);
            }
        }
        public decimal TimebasePosition
        {
            get => _timebasePosition;
            set
            {
                _timebasePosition = value;
                OnPropertyChanged(nameof(TimebasePosition));
                (string, string) commandParts = CommandList.UniversalCommandString(Oscilloscope.Commands.TimebasePositionCommand,
                    TimebasePosition.ToString("0.###E00", CultureInfo.InvariantCulture));
                ApplyCommandToConfigString(commandParts);
            }
        }

        public TriggerViewModel TriggerVM
        {
            get => _triggerVM;
            set { _triggerVM = value; OnPropertyChanged(nameof(TriggerVM)); }
        }

        public List<string> WaveformFormatOptions
        {
            get => _waveformFormatOptions;
            set { _waveformFormatOptions = value; OnPropertyChanged(nameof(WaveformFormatOptions)); }
        }
        public int WaveformFormatIndex
        {
            get => _waveformFormatIndex;
            set
            {
                _waveformFormatIndex = value;
                OnPropertyChanged(nameof(WaveformFormatIndex));
                string command = Oscilloscope.Commands.WaveformFormatCommand;
                string desiredValue = Oscilloscope.Commands.WaveformFormatOptions != null
                    ? Oscilloscope.Commands.WaveformFormatOptions.ElementAtOrDefault(WaveformFormatIndex)
                    : string.Empty;
                (string, string) commandParts = CommandList.UniversalCommandString(command, desiredValue);
                ApplyCommandToConfigString(commandParts);
            }
        }
        public bool WaveformStreaming
        {
            get => _waveformStreaming;
            set
            {
                _waveformStreaming = value;
                OnPropertyChanged(nameof(WaveformStreaming));
                string command = Oscilloscope.Commands.WaveformStreamingCommand;
                string desiredValue = Oscilloscope.Commands.TrueFalseOptions != null
                    ? WaveformStreaming ? Oscilloscope.Commands.TrueFalseOptions.ElementAtOrDefault(0) ?? "1" : Oscilloscope.Commands.TrueFalseOptions.ElementAtOrDefault(1) ?? "0"
                    : WaveformStreaming ? "ON" : "OFF";
                (string, string) commandParts = CommandList.UniversalCommandString(command, desiredValue);
                ApplyCommandToConfigString(commandParts);
            }
        }
        #endregion


        public OscilloscopeConfigViewModel(Oscilloscope oscilloscope, OscilloscopeViewControlState oscilloscopeControlState, Func<OscilloscopeViewModel> oscilloscopeVM)
        {
            Oscilloscope = oscilloscope;

            GetOscilloscopeResourcesToVM();

            CreateCommands(oscilloscopeControlState, oscilloscopeVM);
        }

        private void GetOscilloscopeResourcesToVM()
        {
            TempOscilloscopeConfigString = new List<string>(Oscilloscope.OscilloscopeConfigString);

            TempChannels = new ObservableCollection<ChannelSettingsViewModel>();
            TempChannels.CollectionChanged += TempChannels_Changed;
            foreach (ChannelSettings channel in Oscilloscope.Channels)
            {
                ChannelSettingsViewModel tempChannel = new ChannelSettingsViewModel(channel);
                TempChannels.Add(tempChannel);
            }

            TriggerVM = new TriggerViewModel(Oscilloscope.Trigger);
            TriggerVM.PropertyChanged += TriggerViewModel_PropertyChanged;

            WaveformFormatOptions = new List<string>(Oscilloscope.WaveformFormatOptions);
            _waveformFormatIndex = Oscilloscope.WaveformFormatIndex; //workaround kolem OnPropertyChange with synchronization
        }

        public List<ChannelSettings> ChannelSettingsVMtoModel()
        {
            List<ChannelSettings> channelListForModel = new List<ChannelSettings>();
            foreach (ChannelSettingsViewModel channel in TempChannels)
            {
                ChannelSettings tempChannel =
                    new ChannelSettings(channel.ChannelNumber, channel.ChannelLabel, channel.ChannelDisplay,
                    channel.ChannelScale, channel.ChannelOffset, channel.ChannelCouplingModes, channel.ChannelCouplingIndex);

                channelListForModel.Add(tempChannel);
            }
            return channelListForModel;
        }

        public TriggerSettings TriggerSettingsVMtoModel()
        {
            return new TriggerSettings(TriggerVM.TriggerEdgeSourceOptions, TriggerVM.TriggerEdgeSourceIndex,
               TriggerVM.TriggerEdgeSlopeOptions, TriggerVM.TriggerEdgeSlopeIndex, TriggerVM.TriggerLevel);
        }

        private void TempChannels_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ChannelSettingsViewModel item in e.NewItems)
                {
                    item.PropertyChanged += ChannelSettingsViewModel_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (ChannelSettingsViewModel item in e.OldItems)
                {
                    item.PropertyChanged -= ChannelSettingsViewModel_PropertyChanged;
                }
            }
        }

        private void ChannelSettingsViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ChannelSettingsViewModel channel = sender as ChannelSettingsViewModel;
            string command = string.Empty, desiredValue = string.Empty;
            switch (e.PropertyName)
            {
                case nameof(channel.ChannelDisplay):
                    command = Oscilloscope.Commands.ChannelDisplayCommand;
                    desiredValue = Oscilloscope.Commands.TrueFalseOptions != null
                        ? channel.ChannelDisplay ? Oscilloscope.Commands.TrueFalseOptions.ElementAtOrDefault(0) ?? "1" : Oscilloscope.Commands.TrueFalseOptions.ElementAtOrDefault(1) ?? "0"
                        : channel.ChannelDisplay ? "ON" : "OFF";
                    break;
                case nameof(channel.ChannelLabel):
                    command = Oscilloscope.Commands.ChannelLabelCommand;
                    desiredValue = "\"" + channel.ChannelLabel + "\"";
                    break;
                case nameof(channel.ChannelScale):
                    command = Oscilloscope.Commands.ChannelScaleCommand;
                    desiredValue = channel.ChannelScale.ToString("0.###E00", CultureInfo.InvariantCulture);
                    break;
                case nameof(channel.ChannelOffset):
                    command = Oscilloscope.Commands.ChannelOffsetCommand;
                    desiredValue = channel.ChannelOffset.ToString("0.###E00", CultureInfo.InvariantCulture);
                    break;
                case nameof(channel.ChannelCouplingIndex):
                    command = Oscilloscope.Commands.ChannelCouplingCommand;
                    desiredValue = Oscilloscope.Commands.ChannelCouplingModes != null
                        ? Oscilloscope.Commands.ChannelCouplingModes.ElementAtOrDefault(channel.ChannelCouplingIndex)
                        : string.Empty;
                    break;
                default:
                    break;
            }
            string channelNumber = channel.ChannelNumber.ToString(CultureInfo.InvariantCulture);
            (string, string) commandParts = CommandList.UniversalCommandString(command, channelNumber, desiredValue);
            ApplyCommandToConfigString(commandParts);
        }
        private void TriggerViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TriggerViewModel trigger = sender as TriggerViewModel;
            string command = string.Empty, desiredValue = string.Empty;
            switch (e.PropertyName)
            {
                case nameof(trigger.TriggerEdgeSourceIndex):
                    command = Oscilloscope.Commands.TriggerEdgeSourceCommand;
                    desiredValue = Oscilloscope.Commands.TriggerEdgeSourceOptions != null
                        ? Oscilloscope.Commands.TriggerEdgeSourceOptions.ElementAtOrDefault(trigger.TriggerEdgeSourceIndex)
                        : string.Empty;

                    break;
                case nameof(trigger.TriggerEdgeSlopeIndex):
                    command = Oscilloscope.Commands.TriggerEdgeSlopeCommand;
                    desiredValue = Oscilloscope.Commands.TriggerEdgeSlopeOptions != null
                        ? Oscilloscope.Commands.TriggerEdgeSlopeOptions.ElementAtOrDefault(trigger.TriggerEdgeSlopeIndex)
                        : string.Empty;
                    break;
                case nameof(trigger.TriggerLevel):
                    command = Oscilloscope.Commands.TriggerLevelCommand;
                    desiredValue = trigger.TriggerLevel.ToString("0.###E00", CultureInfo.InvariantCulture);
                    break;
                default:
                    break;
            }
            (string, string) commandParts = CommandList.UniversalCommandString(command, desiredValue);
            ApplyCommandToConfigString(commandParts);
        }
        private void ApplyCommandToConfigString((string, string) commandParts)
        {
            if (!string.IsNullOrEmpty(commandParts.Item1) && !string.IsNullOrEmpty(commandParts.Item2))
            {
                string partCommand = commandParts.Item1;
                string resultCommand = commandParts.Item2;
                _ = TempOscilloscopeConfigString.RemoveAll(x => x.Contains(partCommand));
                TempOscilloscopeConfigString.Add(resultCommand);
                TempOscilloscopeConfigString.Sort(); //Asi radši nesortit... je to blbý jak to pak skáče.... nvm pak to skáče jinak a je to stejně debilní
            }
            OnPropertyChanged(nameof(TempOscilloscopeConfigString));
        }


        public ICommand OpenConfigFileCommand { get; set; }
        public ICommand SaveConfigFileCommand { get; set; }
        public ICommand ApplyOscilloscopeConfigCommand { get; set; }
        public ICommand CancelOscilloscopeConfigCommand { get; set; }
        private void CreateCommands(OscilloscopeViewControlState oscilloscopeControlState, Func<OscilloscopeViewModel> oscilloscopeVM)
        {
            OpenConfigFileCommand = new OpenConfigFileCommand(this);
            SaveConfigFileCommand = new SaveConfigFileCommand(this);
            ApplyOscilloscopeConfigCommand = new ApplyOscilloscopeConfigCommand(this, oscilloscopeControlState, oscilloscopeVM);
            CancelOscilloscopeConfigCommand = new CancelOscilloscopeConfigCommand(oscilloscopeControlState, oscilloscopeVM);
        }
        public static bool CancelCommandMessageBox()
        {
            MessageBoxResult messageBoxResult = MessageBox.Show($"If you press OK, all changes to the oscilloscope configuration will be discarded!{Environment.NewLine}Do you wish to proceed?",
                "Cancel oscilloscope configuration", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            return messageBoxResult == MessageBoxResult.OK;
        }

        //ToDo ideálně odstranit všechny valueConvertery a mít to tady ve VM

        public override void Dispose()
        {
            TempChannels.CollectionChanged -= TempChannels_Changed;
            TriggerVM.PropertyChanged -= TriggerViewModel_PropertyChanged;
            base.Dispose();
        }

        ~OscilloscopeConfigViewModel()
        {
            Debug.WriteLine("KILLING OCVM");
        }



        #region TESTING
        private void TestOscConfigListBinding()
        {
            //Osciloscope.UpdateConfigString();
            //Debug.WriteLine("VM: " + TempOsciloscopeConfigString.Count);
            //Debug.WriteLine("Model: " + Osciloscope.OsciloscopeConfigString.Count);

            //foreach (OscilloscopeConfig.ChannelSettings item in TempChannels)
            //{
            //    Debug.WriteLine("\nVM");
            //    //Debug.WriteLine(item.ChannelScale.GetType());
            //    Debug.WriteLine(item.ChannelScale);

            //    Debug.WriteLine(item.ChannelNumber + " " + item.ChannelCoupling);
            //    //Debug.WriteLine(item.ChannelCoupling);
            //}
            //foreach (OsciloscopeConfig.ChannelSettings item in Osciloscope.Channels)
            //{
            //    Debug.WriteLine("\nM");
            //    //Debug.WriteLine(item.ChannelScale.GetType());
            //    Debug.WriteLine(item.ChannelScale);

            //    Debug.WriteLine(item.ChannelNumber + " " + item.ChannelCoupling);
            //    //Debug.WriteLine(item.ChannelCoupling);
            //}
        }

        //private decimal _testNumberProp = 0.25E-19M;
        //private decimal _testNumberProp = 0.25E19M;
        private bool _testProp;

        public bool TestProp
        {
            get => _testProp;
            set
            {
                _testProp = value;
                //OnPropertyChanged(nameof(TestProp));
                //OnPropertyChanged(""); //Refresh all forced
                TestOscConfigListBinding();
                //Osciloscope.UpdateConfigString();
            }
        }
        #endregion
    }
}
