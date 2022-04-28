using P_SCAAT.Models;
using P_SCAAT.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace P_SCAAT.ViewModels
{
    class SerialPortRS232ViewModel : SessionDeviceVM
    {
        private CryptoDeviceMessage _cryptoDeviceMessage;
        private ObservableCollection<string> _availablePorts;
        private string _selectedAvailablePort;
        private SerialPortRS232 _serialPortRS232;
        private bool _changingSession;

        //private SerialPort _serialPort;

        public CryptoDeviceMessage CryptoDeviceMessage
        {
            get;
            set;
        }

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
        //public SerialPort SerialPort
        //{
        //    get => _serialPort;
        //    set
        //    {
        //        _serialPort = value;
        //        OnPropertyChanged(nameof(SerialPort));
        //    }
        //}
        
        //public override bool IsSessionOpen => SerialPort.IsOpen;


        public SerialPortRS232ViewModel(CryptoDeviceMessage cryptoDeviceMessage)
        {
            CryptoDeviceMessage = cryptoDeviceMessage;

            SerialPortRS232 = new SerialPortRS232();

            AvailablePorts = new ObservableCollection<string>();
            
            RefreshPortList();

            CreateCommands();
        }


        internal void RefreshPortList()
        {
            AvailablePorts.Clear();
            foreach (string portName in SerialPort.GetPortNames())
            {
                AvailablePorts.Add(portName);
            }
            SelectedAvailablePorts = !string.IsNullOrEmpty(SerialPortRS232.PortName)
                ? SerialPortRS232.PortName
                : AvailablePorts.First();
        }

        public ICommand RefreshPortListCommand { get; set; }
        public ICommand ControlSerialPortSessionCommand { get; set; }

        private void CreateCommands()
        {
            RefreshPortListCommand = new SimpleCommand(RefreshPortList);
            ControlSerialPortSessionCommand = new ControlSessionCommand(this);

        }
    }
}
