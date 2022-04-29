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

        private string _errorMessage = string.Empty;

        private string _manualMessageWrite = "*IDN?";
        private string _manualMessageRead = "Response";


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
                FillWaveformSource();
            }
        }
        //public bool IsSessionOpen => Oscilloscope.IsSessionOpen;

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                //_ = MessageBox.Show(ErrorMessage);
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
        #endregion


        public OscilloscopeViewModel(CryptoDeviceMessage cryptoDeviceMessage, Oscilloscope oscilloscope, OscilloscopeViewControlState oscilloscopeControlState, Func<OscilloscopeConfigViewModel> oscilloscopeConfigVM)
        {
            CryptoDeviceMessage = cryptoDeviceMessage;

            Oscilloscope = oscilloscope;
            WaveformSource = new ObservableCollection<WaveformSourceViewModel>();
            FillWaveformSource();

            RefreshOscilloscopeList();

            CreateCommands(oscilloscope, cryptoDeviceMessage, oscilloscopeControlState, oscilloscopeConfigVM);



            //foreach (string item in AvailableOsciloscopes)
            //{
            //    Debug.WriteLine(item);
            //}
        }

        public void FillWaveformSource()
        {
            if(Oscilloscope.Channels != null && Oscilloscope.Channels.Any())
            {
                foreach (OscilloscopeConfig.ChannelSettings channel in Oscilloscope.Channels)
                {
                    WaveformSource.Add(new WaveformSourceViewModel(channel.ChannelLabel, false));
                }
            }
        }

        public void RefreshOscilloscopeList()
        {
            try
            {
                AvailableOscilloscopes = Oscilloscope.GetOscilloscopeList();
            }
            catch (Exception e)
            {
                ErrorMessage += e.Message + Environment.NewLine;
                AvailableOscilloscopes = new List<string> { "EMPTY" };
            }
            SelectedAvailableOscilloscopes = !string.IsNullOrEmpty(Oscilloscope.SessionName)
                ? Oscilloscope.SessionName
                : AvailableOscilloscopes.First();
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
            //OsciloscopeConfigViewSelectCommand = new OsciloscopeConfigViewSelectCommand(osciloscope, osciloscopeControlState);
            //OsciloscopeConfigViewSelectCommand = new OsciloscopeConfigViewSelectCommand(osciloscope, osciloscopeControlState, osciloscopeConfigVM);
            ConfigViewSelectCommand = new ConfigViewSelectCommand(this, oscilloscope, oscilloscopeControlState, oscilloscopeConfigVM);
            ManualControlCommand = new ManualControlCommand(this, oscilloscope);
            MeasureCommand = new MeasureCommand(oscilloscope, cryptoDeviceMessage, 20);

        }
        #endregion



        public override void Dispose()
        {
        }
        ~OscilloscopeViewModel()
        {
            Debug.WriteLine("KILLING OVM");
        }

        //public void CreateOsciloscopeSession()
        //{
        //    ActiveOsciloscope.OpenSession(SelectedAvailableOsciloscopes);
        //}

        //public void CreateNewOsciloscopeSession()
        //{
        //    //if (ActiveOsciloscope != null)
        //    //{
        //    //    ActiveOsciloscope = null;
        //    //}
        //    //ActiveOsciloscope = new Osciloscope(SelectedAvailableOsciloscopes);
        //    //ActiveOsciloscope = osciloscope;
        //    ActiveOsciloscope.OpenSession(SelectedAvailableOsciloscopes);
        //    OnPropertyChanged(nameof(ActiveOsciloscope));
        //    //OnPropertyChanged(propertyName: nameof(ActiveOsciloscope));
        //    //return ActiveOsciloscope.IsSessionOpen;
        //}

        //public void DiscardOsciloscopeSession()
        //{
        //    //if (ActiveOsciloscope != null)
        //    //{
        //    //    ActiveOsciloscope.CloseSession();
        //    //    ActiveOsciloscope = null;
        //    //}
        //    ActiveOsciloscope.CloseSession();
        //}

        //public static List<string> GetOsciloscopeList()
        //{
        //    return Osciloscope.GetResources();
        //}
    }
}