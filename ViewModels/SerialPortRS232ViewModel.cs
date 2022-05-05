using P_SCAAT.Models;
using P_SCAAT.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace P_SCAAT.ViewModels
{
    internal class SerialPortRS232ViewModel : SessionDeviceVM
    {
        #region Properties
        private ObservableCollection<string> _availablePorts;
        private string _selectedAvailablePort;
        private SerialPortRS232 _serialPortRS232;
        private bool _changingSession;

        private int _baudRate;
        private string _selectedParity;
        private int _dataBits;
        private string _selectedStopBits;

        public CryptoDeviceMessage CryptoDeviceMessage { get; set; }

        public ObservableCollection<string> AvailablePorts
        {
            get => _availablePorts;
            private set
            {
                _availablePorts = value;
                OnPropertyChanged(nameof(AvailablePorts));
            }
        }

        public string SelectedAvailablePorts
        {
            get => _selectedAvailablePort;
            set
            {
                _selectedAvailablePort = value;
                OnPropertyChanged(nameof(SelectedAvailablePorts));
            }
        }
        public override string SelectedAvailableResource => SelectedAvailablePorts;
        public SerialPortRS232 SerialPortRS232
        {
            get => _serialPortRS232;
            set
            {
                _serialPortRS232 = value;
                OnPropertyChanged(nameof(SerialPortRS232));
            }
        }
        public override ISessionDevice SessionDevice => SerialPortRS232;
        public override bool ChangingSession
        {
            get => _changingSession;
            set
            {
                _changingSession = value;
                OnPropertyChanged(nameof(ChangingSession));
                OnPropertyChanged(nameof(IsSessionOpen));
            }
        }

        public int BaudRate
        {
            get => _baudRate; set
            {
                _baudRate = value;
                OnPropertyChanged(nameof(BaudRate));
                SerialPortRS232.BaudRate = BaudRate;
            }
        }
        public string[] SerialPortParity => Enum.GetNames(typeof(Parity));
        public string SelectedParity
        {
            get => _selectedParity;
            set
            {
                _selectedParity = value;
                OnPropertyChanged(nameof(SelectedParity));
                if (Enum.TryParse(SelectedParity, out Parity result))
                {
                    SerialPortRS232.Parity = result;
                }
            }
        }
        public int DataBits
        {
            get => _dataBits;
            set
            {
                _dataBits = value;
                OnPropertyChanged(nameof(DataBits));
                SerialPortRS232.DataBits = DataBits;
            }
        }
        public string[] SerialPortStopBits => Enum.GetNames(typeof(StopBits));
        public string SelectedStopBits
        {
            get => _selectedStopBits;
            set
            {
                _selectedStopBits = value;
                OnPropertyChanged(nameof(SelectedStopBits));
                if (Enum.TryParse(SelectedStopBits, out StopBits result))
                {
                    SerialPortRS232.StopBits = result;
                }
            }
        }
        #endregion

        public SerialPortRS232ViewModel(CryptoDeviceMessage cryptoDeviceMessage) : base()
        {
            CryptoDeviceMessage = cryptoDeviceMessage;

            SerialPortRS232 = new SerialPortRS232();
            BaudRate = SerialPortRS232.BaudRate;
            SelectedParity = SerialPortRS232.Parity.ToString();
            DataBits = SerialPortRS232.DataBits;
            SelectedStopBits = SerialPortRS232.StopBits.ToString();


            AvailablePorts = new ObservableCollection<string>();

            RefreshPortList();

            CryptoDeviceMessage.MessageCreation += OnCryptoMessageCreated;

            CreateCommands();
        }

        /// <summary>
        /// This method is called when new message in <see cref="CryptoDeviceMessage.GenerateNewMessage"/> is created. (Subscribtion to <see cref="CryptoDeviceMessage.MessageCreation"/> event)
        /// </summary>
        private void OnCryptoMessageCreated()
        {
            Thread.Sleep(5);
            SerialPortRS232.Send(CryptoDeviceMessage.MessageBytes);
        }

        internal void RefreshPortList()
        {
            AvailablePorts.Clear();
            foreach (string portName in SerialPort.GetPortNames())
            {
                AvailablePorts.Add(portName);
            }
            if (AvailablePorts.Any())
            {
                SelectedAvailablePorts = !string.IsNullOrEmpty(SerialPortRS232.PortName)
                    ? SerialPortRS232.PortName
                    : AvailablePorts.First();
            }
            else
            {
                AvailablePorts.Add("EMPTY");
                SelectedAvailablePorts = AvailablePorts.First();
            }
        }

        public ICommand RefreshPortListCommand { get; set; }
        public ICommand ControlSerialPortSessionCommand { get; set; }


        private void CreateCommands()
        {
            RefreshPortListCommand = new SimpleCommand(RefreshPortList);
            ControlSerialPortSessionCommand = new ControlSessionCommand(this);
        }

        public override void Dispose()
        {
            CryptoDeviceMessage.MessageCreation -= OnCryptoMessageCreated;
            base.Dispose();
        }
    }
}
