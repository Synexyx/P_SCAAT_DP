using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace P_SCAAT.Models
{
    internal class RNGMessageGenerator
    {
        private readonly byte[] messageBytes;
        private readonly RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();

        public RNGMessageGenerator(uint messageLenght)
        {
            messageBytes = new byte[messageLenght];
        }

        public byte[] GetNewMessage()
        {
            randomNumberGenerator.GetBytes(messageBytes);
            return messageBytes;
        }
    }
}
