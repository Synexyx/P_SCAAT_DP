using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P_SCAAT.Models
{
    internal class CryptoDeviceMessage
    {
        public byte[] MessageBytes { get; set; }
        public int MessageLenght => MessageBytes.Length;
        public DateTime TimeCreated { get; set; }
        public RNGMessageGenerator RNGMessageGenerator { get; }

        public void GenerateNewMessage()
        {
            MessageBytes = RNGMessageGenerator.GetNewMessage();
            TimeCreated = DateTime.Now;
        }
    }
}
