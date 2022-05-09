using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using P_SCAAT.Models;
using P_SCAAT.ViewModels.Commands;
using P_SCAAT.ViewModels.ViewControlState;

namespace P_SCAAT.ViewModels
{
    /// <summary>
    /// View model for <see cref="OscilloscopeConfig"/>
    /// </summary>
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

        /// <summary>
        /// If new error occurs display <see cref="MessageBox"/> to inform user about it. 
        /// </summary>
        public void ErrorMessage_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Exception item in e.NewItems)
                {
                    _ = MessageBox.Show($"{item.Message} {item.StackTrace}", $"{item.GetType()}", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Used as conversion between <see cref="List{T}"/> and <see cref="string"/> for displaying in TextBox
        /// </summary>
        private string ListToString(List<string> listToConvert)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string item in listToConvert)
            {
                _ = stringBuilder.AppendLine(item);
            }
            return stringBuilder.ToString();
        }
        /// <summary>
        /// Used as reverse conversion between <see cref="List{T}"/> and <see cref="string"/> for displaying in TextBox spliting by lines.
        /// </summary>
        private List<string> StringToList(string stringToConvert)
        {
            List<string> resultList = Regex.Split(stringToConvert, @"\r|\n|\r\n").Where(line => line != string.Empty).ToList();
            return resultList;
        }

        /// <summary>
        /// Initialize and synchronize data with their model equivalent.
        /// </summary>
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

            TimebasePosition = Oscilloscope.TimebasePosition;
            TimebaseScale = Oscilloscope.TimebaseScale;

            WaveformFormatOptions = new List<string>(Oscilloscope.WaveformFormatOptions);
            _waveformFormatIndex = Oscilloscope.WaveformFormatIndex;

            WaveformStreaming = Oscilloscope.WaveformStreaming;
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

        /// <summary>
        /// Subscribing to <see cref="TempChannels"/> collection changed event. Automatically subscribe to all new items property change event.
        /// </summary>
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

        /// <summary>
        /// Subscribe to all elements in <see cref="ChannelSettingsViewModel"/>. If any element changes value, this will automatically create proper command containing changed value.
        /// </summary>
        private void ChannelSettingsViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ChannelSettingsViewModel channel = sender as ChannelSettingsViewModel;
            switch (e.PropertyName)
            {
                case nameof(channel.ChannelDisplay):
                    CreateCommandString(Oscilloscope.Commands.ChannelDisplayCommand, channel.ChannelDisplay, channel.ChannelNumber);
                    break;
                case nameof(channel.ChannelLabel):
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
        /// <summary>
        /// Subscribe to all elements in <see cref="TriggerViewModel"/>. If any element changes value, this will automatically create proper command containing changed value.
        /// </summary>
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
                    string triggerSource = trigger.TriggerEdgeSourceOptions.ElementAtOrDefault(trigger.TriggerEdgeSourceIndex);
                    if (trigger.TriggerLevel.ContainsKey(triggerSource))
                    {
                        decimal triggerLevel = trigger.TriggerLevel[triggerSource];
                        CreateSourceCommandString(Oscilloscope.Commands.TriggerLevelCommand, triggerSource, triggerLevel);
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// All overloads to create proper command to config string. <br/>
        /// Individual methods can deal with channel settings, updating value from collection, using command with single bool value and commands with value that is source dependant.
        /// </summary>
        #region CreateCommandString overloads
        private void CreateCommandString(string command, IEnumerable<string> collection, int index)
        {
            if (collection != null)
            {
                try
                {
                    string desiredValue = collection.ElementAtOrDefault(index);
                    (string, string) commandParts = CommandList.UniversalCommandString(command, desiredValue);
                    ApplyCommandToConfigString(commandParts);
                }
                catch (Exception ex)
                {
                    ErrorMessages.Add(ex);
                    Debug.WriteLine($"{ex.Message}", $"{ex.GetType()}");
                }
            }
        }
        private void CreateCommandString(string command, IEnumerable<string> collection, int index, int channelNumber)
        {
            if (collection != null)
            {
                try
                {
                    string desiredValue = collection.ElementAtOrDefault(index);
                    (string, string) commandParts = CommandList.UniversalCommandString(command, channelNumber.ToString(CultureInfo.InvariantCulture), desiredValue);
                    ApplyCommandToConfigString(commandParts);
                }
                catch (Exception ex)
                {
                    ErrorMessages.Add(ex);
                    Debug.WriteLine($"{ex.Message}", $"{ex.GetType()}");
                }
            }
        }
        private void CreatCommandString(string command, string stringValue)
        {
            try
            {
                (string, string) commandParts = CommandList.UniversalCommandString(command, stringValue);
                ApplyCommandToConfigString(commandParts);
            }
            catch (Exception ex)
            {
                ErrorMessages.Add(ex);
                Debug.WriteLine($"{ex.Message}", $"{ex.GetType()}");
            }
        }
        private void CreateCommandString(string command, string stringValue, int channelNumber)
        {
            try
            {
                (string, string) commandParts = CommandList.UniversalCommandString(command, channelNumber.ToString(CultureInfo.InvariantCulture), stringValue);
                ApplyCommandToConfigString(commandParts);
            }
            catch (Exception ex)
            {
                ErrorMessages.Add(ex);
                Debug.WriteLine($"{ex.Message}", $"{ex.GetType()}");
            }
        }
        private void CreateCommandString(string command, decimal numericValue)
        {
            try
            {
                string desiredValue = numericValue.ToString("0.###E00", CultureInfo.InvariantCulture);
                (string, string) commandParts = CommandList.UniversalCommandString(command, desiredValue);
                ApplyCommandToConfigString(commandParts);
            }
            catch (Exception ex)
            {
                ErrorMessages.Add(ex);
                Debug.WriteLine($"{ex.Message}", $"{ex.GetType()}");
            }
        }
        private void CreateCommandString(string command, decimal numericValue, int channelNumber)
        {
            try
            {
                string desiredValue = numericValue.ToString("0.###E00", CultureInfo.InvariantCulture);
                (string, string) commandParts = CommandList.UniversalCommandString(command, channelNumber.ToString(CultureInfo.InvariantCulture), desiredValue);
                ApplyCommandToConfigString(commandParts);
            }
            catch (Exception ex)
            {
                ErrorMessages.Add(ex);
                Debug.WriteLine($"{ex.Message}", $"{ex.GetType()}");
            }
        }
        private void CreateCommandString(string command, bool trueFalseValue)
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
                try
                {
                    (string, string) commandParts = CommandList.UniversalCommandString(command, desiredValue);
                    ApplyCommandToConfigString(commandParts);
                }
                catch (Exception ex)
                {
                    ErrorMessages.Add(ex);
                    Debug.WriteLine($"{ex.Message}", $"{ex.GetType()}");
                }
            }
        }
        private void CreateCommandString(string command, bool trueFalseValue, int channelNumber)
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
                try
                {
                    (string, string) commandParts = CommandList.UniversalCommandString(command, channelNumber.ToString(CultureInfo.InvariantCulture), desiredValue);
                    ApplyCommandToConfigString(commandParts);
                }
                catch (Exception ex)
                {
                    ErrorMessages.Add(ex);
                    Debug.WriteLine($"{ex.Message}", $"{ex.GetType()}");
                }
            }
        }
        private void CreateSourceCommandString(string command, string stringValue, decimal numericValue)
        {
            string desiredValue = numericValue.ToString("0.###E00", CultureInfo.InvariantCulture);
            (string, string) commandParts;
            try
            {
                commandParts = CommandList.UniversalCommandString(command, stringValue, desiredValue);
                ApplyCommandToConfigString(commandParts);
            }
            catch (Exception ex)
            {
                ErrorMessages.Add(ex);
                Debug.WriteLine($"{ex.Message}", $"{ex.GetType()}");
            }
        }
        #endregion

        /// <summary>
        /// Apply command to <see cref="TempOscilloscopeConfigString"/> and removes duplicates and previous instances of that command with different values.
        /// </summary>
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
