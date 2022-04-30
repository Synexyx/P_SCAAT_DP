using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P_SCAAT.Models
{
    internal class CryptoDeviceMessage
    {
        public event Action MessageCreation;

        public byte[] MessageBytes { get; set; }
        public int MessageLenght => MessageBytes.Length;
        public DateTime TimeCreated { get; set; }
        public RNGMessageGenerator RNGMessageGenerator { get; private set; }

        public void InitializeRNGMessageGenerator(int messageLenght)
        {
            RNGMessageGenerator = new RNGMessageGenerator(messageLenght);
        }
        public void GenerateNewMessage()
        {
            if (RNGMessageGenerator != null)
            {
                MessageBytes = RNGMessageGenerator.GetNewMessage();
                TimeCreated = DateTime.Now;
                MessageCreation?.Invoke();
            }
        }
    }
}
