using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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
    internal class OscilloscopeConfigViewModel : OscilloscopeValueConversionVM, IErrorMessage
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

        public ObservableCollection<Exception> ErrorMessages { get; } = new ObservableCollection<Exception>();
        public Oscilloscope Oscilloscope
        {
            get => _oscilloscope;
            private set { _oscilloscope = value; OnPropertyChanged(nameof(Oscilloscope)); }
        }
        public List<string> TempOscilloscopeConfigString
        {
            get => _tempOscilloscopeConfigString;
            set
            {
                _tempOscilloscopeConfigString = value;
                OnPropertyChanged(nameof(TempOscilloscopeConfigString));
                OnPropertyChanged(nameof(TempOscilloscopeConfigStringForTextBox));
            }
        }
        public string TempOscilloscopeConfigStringForTextBox
        {
            get => ListToString(TempOscilloscopeConfigString);
            set { TempOscilloscopeConfigString = StringToList(value); OnPropertyChanged(nameof(TempOscilloscopeConfigStringForTextBox)); }
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
                CreateCommandString(Oscilloscope.Commands.TimebaseScaleCommand, TimebaseScale);
            }
        }
        public string TimebaseScaleForTextBox
        {
            get => DecimalToString(TimebaseScale, "s");
            set { TimebaseScale = StringToDecimal(value); OnPropertyChanged(nameof(TimebaseScaleForTextBox)); }
        }
        public decimal TimebasePosition
        {
            get => _timebasePosition;
            set
            {
                _timebasePosition = value;
                OnPropertyChanged(nameof(TimebasePosition));
                CreateCommandString(Oscilloscope.Commands.TimebasePositionCommand, TimebasePosition);
            }
        }
        public string TimebasePositionForTextBox
        {
            get => DecimalToString(TimebasePosition, "s");
            set { TimebasePosition = StringToDecimal(value); OnPropertyChanged(nameof(TimebasePositionForTextBox)); }
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
                CreateCommandString(Oscilloscope.Commands.WaveformFormatCommand, WaveformFormatOptions, WaveformFormatIndex);
            }
        }
        public bool WaveformStreaming
        {
            get => _waveformStreaming;
            set
            {
                _waveformStreaming = value;
                OnPropertyChanged(nameof(WaveformStreaming));
                CreateCommandString(Oscilloscope.Commands.WaveformStreamingCommand, WaveformStreaming);
            }
        }
        #endregion


        public OscilloscopeConfigViewModel(Oscilloscope oscilloscope, OscilloscopeViewControlState oscilloscopeControlState, Func<OscilloscopeViewModel> oscilloscopeVM)
        {
            Oscilloscope = oscilloscope;


            GetOscilloscopeResourcesToVM();

            ErrorMessages.CollectionChanged += ErrorMessage_Changed;
            CreateCommands(oscilloscopeControlState, oscilloscopeVM);
        }

        public void ErrorMessage_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Exception item in e.NewItems)
                {
                    _ = MessageBox.Show($"{item.Message}{Environment.NewLine}{item.StackTrace}", $"{item.GetType()}", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string ListToString(List<string> listToConvert)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string item in listToConvert)
            {
                _ = stringBuilder.AppendLine(item);
            }
            return stringBuilder.ToString();
        }
        private List<string> StringToList(string stringToConvert)
        {
            List<string> resultList = Regex.Split(stringToConvert, @"\r|\n|\r\n").Where(line => line != string.Empty).ToList();
            return resultList;
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
            switch (e.PropertyName)
            {
                case nameof(channel.ChannelDisplay):
                    CreateCommandString(Oscilloscope.Commands.ChannelDisplayCommand, channel.ChannelDisplay, channel.ChannelNumber);
                    break;
                case nameof(channel.ChannelLabel):
                    //CreateCommandString(Oscilloscope.Commands.ChannelLabelCommand, channel.ChannelLabel, channel.ChannelNumber);
                    CreateCommandString(Oscilloscope.Commands.ChannelLabelCommand, $"\"{channel.ChannelLabel}\"", channel.ChannelNumber);
                    break;
                case nameof(channel.ChannelScale):
                    CreateCommandString(Oscilloscope.Commands.ChannelScaleCommand, channel.ChannelScale, channel.ChannelNumber);
                    break;
                case nameof(channel.ChannelOffset):
                    CreateCommandString(Oscilloscope.Commands.ChannelOffsetCommand, channel.ChannelOffset, channel.ChannelNumber);
                    break;
                case nameof(channel.ChannelCouplingIndex):
                    CreateCommandString(Oscilloscope.Commands.ChannelCouplingCommand, channel.ChannelCouplingModes, channel.ChannelCouplingIndex, channel.ChannelNumber);
                    break;
                default:
                    break;
            }
        }
        private void TriggerViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TriggerViewModel trigger = sender as TriggerViewModel;
            switch (e.PropertyName)
            {
                case nameof(trigger.TriggerEdgeSourceIndex):
                    CreateCommandString(Oscilloscope.Commands.TriggerEdgeSourceCommand, trigger.TriggerEdgeSourceOptions, trigger.TriggerEdgeSourceIndex);
                    break;
                case nameof(trigger.TriggerEdgeSlopeIndex):
                    CreateCommandString(Oscilloscope.Commands.TriggerEdgeSlopeCommand, trigger.TriggerEdgeSlopeOptions, trigger.TriggerEdgeSlopeIndex);
                    break;
                case nameof(trigger.TriggerLevel):
                    CreateCommandString(Oscilloscope.Commands.TriggerLevelCommand, trigger.TriggerLevel);
                    break;
                default:
                    break;
            }
        }

        private void CreateCommandString(string command, IEnumerable<string> collection, int index, int channelNumber = 0)
        {
            if (collection != null)
            {
                (string, string) commandParts;
                string desiredValue = collection.ElementAtOrDefault(index);
                try
                {
                    if (channelNumber == 0)
                    {
                        commandParts = CommandList.UniversalCommandString(command, desiredValue);
                    }
                    else
                    {
                        commandParts = CommandList.UniversalCommandString(command, channelNumber.ToString(CultureInfo.InvariantCulture), desiredValue);
                    }
                    ApplyCommandToConfigString(commandParts);
                }
                catch (Exception ex)
                {
                    ErrorMessages.Add(ex);
                    //_ = MessageBox.Show($"{exp.Message}", $"{exp.GetType()}", MessageBoxButton.OK, MessageBoxImage.Error);
                    Debug.WriteLine($"{ex.Message}", $"{ex.GetType()}");
                }
            }
        }
        private void CreateCommandString(string command, string stringValue, int channelNumber = 0)
        {
            (string, string) commandParts;
            try
            {
                if (channelNumber == 0)
                {
                    commandParts = CommandList.UniversalCommandString(command, stringValue);
                }
                else
                {
                    commandParts = CommandList.UniversalCommandString(command, channelNumber.ToString(CultureInfo.InvariantCulture), stringValue);
                }
                ApplyCommandToConfigString(commandParts);
            }
            catch (Exception ex)
            {
                ErrorMessages.Add(ex);
                //_ = MessageBox.Show($"{exp.Message}", $"{exp.GetType()}", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"{ex.Message}", $"{ex.GetType()}");
            }
        }
        private void CreateCommandString(string command, decimal numericValue, int channelNumber = 0)
        {
            string desiredValue = numericValue.ToString("0.###E00", CultureInfo.InvariantCulture);
            (string, string) commandParts;
            try
            {
                if (channelNumber == 0)
                {
                    commandParts = CommandList.UniversalCommandString(command, desiredValue);
                }
                else
                {
                    commandParts = CommandList.UniversalCommandString(command, channelNumber.ToString(CultureInfo.InvariantCulture), desiredValue);
                }
                ApplyCommandToConfigString(commandParts);
            }
            catch (Exception ex)
            {
                ErrorMessages.Add(ex);
                //_ = MessageBox.Show($"{exp.Message}", $"{exp.GetType()}", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"{ex.Message}", $"{ex.GetType()}");
            }
        }
        private void CreateCommandString(string command, bool trueFalseValue, int channelNumber = 0)
        {
            if (Oscilloscope.Commands.TrueFalseOptions != null)
            {
                string desiredValue;
                if (trueFalseValue)
                {
                    desiredValue = Oscilloscope.Commands.TrueFalseOptions.ElementAtOrDefault(0);
                }
                else
                {
                    desiredValue = Oscilloscope.Commands.TrueFalseOptions.ElementAtOrDefault(1);
                }
                (string, string) commandParts;
                try
                {
                    if (channelNumber == 0)
                    {
                        commandParts = CommandList.UniversalCommandString(command, desiredValue);
                    }
                    else
                    {
                        commandParts = CommandList.UniversalCommandString(command, channelNumber.ToString(CultureInfo.InvariantCulture), desiredValue);
                    }
                    ApplyCommandToConfigString(commandParts);
                }
                catch (Exception ex)
                {
                    ErrorMessages.Add(ex);
                    //_ = MessageBox.Show($"{exp.Message}", $"{exp.GetType()}", MessageBoxButton.OK, MessageBoxImage.Error);
                    Debug.WriteLine($"{ex.Message}", $"{ex.GetType()}");
                }
            }
        }

        private void ApplyCommandToConfigString((string, string) commandParts)
        {
            if (!string.IsNullOrEmpty(commandParts.Item1) && !string.IsNullOrEmpty(commandParts.Item2))
            {
                string partCommand = commandParts.Item1;
                string resultCommand = commandParts.Item2;
                _ = TempOscilloscopeConfigString.RemoveAll(x => x.Contains(partCommand));
                TempOscilloscopeConfigString.Add(resultCommand);
                TempOscilloscopeConfigString.Sort();
            }
            OnPropertyChanged(nameof(TempOscilloscopeConfigString));
            OnPropertyChanged(nameof(TempOscilloscopeConfigStringForTextBox));
        }


        public ICommand OpenConfigFileCommand { get; set; }
        public ICommand SaveConfigFileCommand { get; set; }
        public ICommand ApplyOscilloscopeConfigCommand { get; set; }
        public ICommand CancelOscilloscopeConfigCommand { get; set; }
        public ICommand RadioButtonEdgeSlopeCommand { get; set; }

        private void CreateCommands(OscilloscopeViewControlState oscilloscopeControlState, Func<OscilloscopeViewModel> oscilloscopeVM)
        {
            OpenConfigFileCommand = new OpenConfigFileCommand(this);
            SaveConfigFileCommand = new SaveConfigFileCommand(this);
            ApplyOscilloscopeConfigCommand = new ApplyOscilloscopeConfigCommand(this, oscilloscopeControlState, oscilloscopeVM);
            CancelOscilloscopeConfigCommand = new CancelOscilloscopeConfigCommand(oscilloscopeControlState, oscilloscopeVM);
            RadioButtonEdgeSlopeCommand = new RadioButtonEdgeSlopeCommand(this);
        }
        public static bool CancelCommandMessageBox()
        {
            MessageBoxResult messageBoxResult = MessageBox.Show($"If you press OK, all changes to the oscilloscope configuration will be discarded!{Environment.NewLine}Do you wish to proceed?",
                "Cancel oscilloscope configuration", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            return messageBoxResult == MessageBoxResult.OK;
        }

        public override void Dispose()
        {
            ErrorMessages.CollectionChanged -= ErrorMessage_Changed;

            TempChannels.CollectionChanged -= TempChannels_Changed;
            TriggerVM.PropertyChanged -= TriggerViewModel_PropertyChanged;
            base.Dispose();
        }

        ~OscilloscopeConfigViewModel()
        {
            Debug.WriteLine("KILLING OCVM");
        }
    }
}
