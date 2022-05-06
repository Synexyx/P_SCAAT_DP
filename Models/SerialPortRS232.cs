using System;
using System.IO.Ports;
using P_SCAAT.Exceptions;
using System.Diagnostics;

namespace P_SCAAT.Models
{
    internal class SerialPortRS232 : ISessionDevice
    {
        #region Properties
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public Parity Parity { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
        public SerialPort SerialPort { get; }
        public bool IsSessionOpen => SerialPort.IsOpen;
        #endregion

        public SerialPortRS232()
        {
            SerialPort = new SerialPort();
            PortName = string.Empty;
            BaudRate = 115200;
            Parity = Parity.None;
            DataBits = 8;
            StopBits = StopBits.Two;
        }

        public void OpenSession(string portName)
        {
            PortName = portName;

            try
            {
                SerialPort.PortName = PortName;
                SerialPort.BaudRate = BaudRate;
                SerialPort.Parity = Parity;
                SerialPort.DataBits = DataBits;
                SerialPort.StopBits = StopBits;
                //SerialPort.DtrEnable = true;
                //SerialPort.RtsEnable = true;
                //SerialPort.WriteTimeout = 1000;
                //SerialPort.ReadTimeout = 1000;
                SerialPort.Open();
            }
            catch (Exception ex)
            {
                throw new SessionControlException($"Serial port session cannot be estabilished!{Environment.NewLine}REASON :{ex.GetType()}{Environment.NewLine}{ex.Message}", ex);
            }
        }

        public void CloseSession()
        {
            using (SerialPort)
            {
                SerialPort.Close();
            }
        }

        internal void Send(byte[] messageBytes)
        {
            //ToDo don't forget SerialPort sending
            SerialPort.Write(messageBytes, 0, messageBytes.Length);
            Debug.WriteLine($"{DateTime.Now} SENDING MESSAGE OF LENGHT {messageBytes.Length}");
        }
    }
}
