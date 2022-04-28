using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace P_SCAAT.Models
{
    internal class SerialPortRS232 : ISessionDevice
    {
        public string PortName { get; set; }
        public int BaudRate  { get; set; }
        public Parity Parity { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
        public SerialPort SerialPort { get; }
        public bool IsSessionOpen => SerialPort.IsOpen;

        public SerialPortRS232()
        {
            SerialPort = new SerialPort();
            BaudRate = 115200;
            Parity = Parity.None;
            DataBits = 8;
            StopBits = StopBits.Two;
        }

        public void OpenSession(string portName)
        {
            PortName = portName;

            SerialPort.PortName = PortName;
            SerialPort.BaudRate = BaudRate;
            SerialPort.Parity = Parity;
            SerialPort.DataBits = DataBits;
            SerialPort.StopBits = StopBits;
            SerialPort.DtrEnable = true;
            SerialPort.RtsEnable = true;
            SerialPort.WriteTimeout = 1000;
            SerialPort.ReadTimeout = 1000;
            SerialPort.Open();
        }

        public void CloseSession()
        {
            SerialPort.Close();
            SerialPort.Dispose();
        }



    }
}
