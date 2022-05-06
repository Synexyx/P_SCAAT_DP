using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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

namespace P_SCAAT.ViewModels
{
    internal class OscilloscopeViewModel : SessionDeviceVM
    {
        #region Properties
        private CryptoDeviceMessage _cryptoDeviceMessage;
        private List<string> _availableOscilloscopes;
        private string _selectedAvailableOscilloscopes;
        private Oscilloscope _oscilloscope;
        private bool _changingSession;

        private bool _measurementInProgress;

        private string _manualMessageWrite = "*IDN?";
        private string _manualMessageRead = "Response";

        private uint _tracesTotal = 10;
        private uint _tracesPerFile;
        private uint _messageLenght = 32;

        private readonly string _measureButtonContentStart = "START";
        private readonly string _measureButtonContentCancel = "CANCEL";

        private float _progressBarValue;


        public CryptoDeviceMessage CryptoDeviceMessage
        {
            get => _cryptoDeviceMessage;
            private set
            {
                _cryptoDeviceMessage = value;
                OnPropertyChanged(nameof(CryptoDeviceMessage));
            }
        }
        public List<string> AvailableOscilloscopes
        {
            get => _availableOscilloscopes;
            private set
            {
                _availableOscilloscopes = value;
                OnPropertyChanged(nameof(AvailableOscilloscopes));
            }
        }
        public string SelectedAvailableOscilloscopes
        {
            get => _selectedAvailableOscilloscopes;
            set
            {
                _selectedAvailableOscilloscopes = value;
                OnPropertyChanged(nameof(SelectedAvailableOscilloscopes));
            }
        }
        public override string SelectedAvailableResource => SelectedAvailableOscilloscopes;
        public Oscilloscope Oscilloscope
        {
            get => _oscilloscope;
            private set
            {
                _oscilloscope = value;
                OnPropertyChanged(nameof(Oscilloscope));
                OnPropertyChanged(nameof(IsSessionOpen));
            }
        }
        public override ISessionDevice SessionDevice => Oscilloscope;
        public override bool ChangingSession
        {
            get => _changingSession;
            set
            {
                _changingSession = value;
                OnPropertyChanged(nameof(ChangingSession));
                OnPropertyChanged(nameof(IsSessionOpen));
                if (IsSessionOpen)
                {
                    FillWaveformSource();
                }
                else
                {
                    WaveformSource.Clear();
                }
            }
        }
        public bool MeasurementInProgress
        {
            get => _measurementInProgress;
            set { _measurementInProgress = value; OnPropertyChanged(nameof(MeasurementInProgress)); OnPropertyChanged(nameof(MeasureButtonContent)); }
        }
        public string MeasureButtonContent
        {
            get
            {
                if (MeasurementInProgress) { return _measureButtonContentCancel; }
                else { return _measureButtonContentStart; }
            }
        }

        public string ManualMessageWrite
        {
            get => _manualMessageWrite;
            set
            {
                _manualMessageWrite = value;
                OnPropertyChanged(nameof(ManualMessageWrite));
            }
        }
        public string ManualMessageRead
        {
            get => _manualMessageRead;
            set
            {
                _manualMessageRead = value;
                OnPropertyChanged(nameof(ManualMessageRead));
            }
        }
        private ObservableCollection<WaveformSourceViewModel> _waveformSource;
        public ObservableCollection<WaveformSourceViewModel> WaveformSource
        {
            get => _waveformSource;
            set
            {
                _waveformSource = value;
                OnPropertyChanged(nameof(WaveformSource));
            }
        }
        public uint MessageLenght
        {
            get => _messageLenght;
            set
            {
                _messageLenght = value;
                OnPropertyChanged(nameof(MessageLenght));
            }
        }
        public uint TracesTotal
        {
            get => _tracesTotal;
            set
            {
                _tracesTotal = value;
                OnPropertyChanged(nameof(TracesTotal));
            }
        }
        public uint TracesPerFile
        {
            get => _tracesPerFile;
            set
            {
                if (_tracesTotal >= value)
                {
                    _tracesPerFile = value;
                }
                else
                {
                    _tracesPerFile = _tracesTotal;
                }
                OnPropertyChanged(nameof(TracesPerFile));
            }
        }
        public float ProgressBarValue
        {
            get => _progressBarValue;
            set
            {
                _progressBarValue = value / _tracesTotal * 100;
                OnPropertyChanged(nameof(ProgressBarValue));
            }
        }
        #endregion


        public OscilloscopeViewModel(CryptoDeviceMessage cryptoDeviceMessage, Oscilloscope oscilloscope, OscilloscopeViewControlState oscilloscopeControlState,
            Func<OscilloscopeConfigViewModel> oscilloscopeConfigVM) : base()
        {
            CryptoDeviceMessage = cryptoDeviceMessage;

            Oscilloscope = oscilloscope;
            WaveformSource = new ObservableCollection<WaveformSourceViewModel>();
            FillWaveformSource();

            RefreshOscilloscopeList();

            CreateCommands(oscilloscope, cryptoDeviceMessage, oscilloscopeControlState, oscilloscopeConfigVM);
        }

        /// <summary>
        /// Fills list of available waveform sources for user to choose source for measurement.
        /// </summary>
        public void FillWaveformSource()
        {
            if (Oscilloscope.WaveformSourceOptions != null && Oscilloscope.WaveformSourceOptions.Any())
            {
                foreach (string source in Oscilloscope.WaveformSourceOptions)
                {
                    WaveformSource.Add(new WaveformSourceViewModel(source, false));
                }
                WaveformSource.First().IsSelected = true;
                OnPropertyChanged(nameof(WaveformSource));
            }
        }

        public void RefreshOscilloscopeList()
        {
            try
            {
                AvailableOscilloscopes = Oscilloscope.GetOscilloscopeList();
            }
            catch (Exception ex)
            {
                AvailableOscilloscopes = new List<string> { "EMPTY" };
                ErrorMessages.Add(ex);
            }
            if (string.IsNullOrEmpty(Oscilloscope.SessionName))
            {
                SelectedAvailableOscilloscopes = AvailableOscilloscopes.First();
            }
            else
            {
                SelectedAvailableOscilloscopes = Oscilloscope.SessionName;
            }
        }

        #region Commands
        public ICommand ControlOscilloscopeSessionCommand { get; set; }
        public ICommand RefreshOscilloscopeListCommand { get; set; }
        public ICommand ConfigViewSelectCommand { get; set; }
        public ICommand ManualControlCommand { get; set; }
        public ICommand MeasureCommand { get; set; }


        private void CreateCommands(Oscilloscope oscilloscope, CryptoDeviceMessage cryptoDeviceMessage, OscilloscopeViewControlState oscilloscopeControlState, Func<OscilloscopeConfigViewModel> oscilloscopeConfigVM)
        {
            ControlOscilloscopeSessionCommand = new ControlSessionCommand(this);
            RefreshOscilloscopeListCommand = new SimpleCommand(RefreshOscilloscopeList);
            ConfigViewSelectCommand = new ConfigViewSelectCommand(this, oscilloscope, oscilloscopeControlState, oscilloscopeConfigVM);
            ManualControlCommand = new ManualControlCommand(this, oscilloscope);
            MeasureCommand = new MeasureCommand(this, oscilloscope, cryptoDeviceMessage);

        }
        #endregion



        public override void Dispose()
        {
        }
        ~OscilloscopeViewModel()
        {
            Debug.WriteLine("KILLING OVM");
        }
    }
}