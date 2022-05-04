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
        /// <summary>
        /// Creates new <see cref="messageBytes"/> with lenght defined by <paramref name="messageLenght"/>  
        /// </summary>
        /// <param name="messageLenght"></param>
        public RNGMessageGenerator(uint messageLenght)
        {
            messageBytes = new byte[messageLenght];
        }
        /// <summary>
        /// Using System.Security.Cryptography fills <see cref="messageBytes"/> with pseudo-random bytes.
        /// </summary>
        public byte[] GetNewMessage()
        {
            randomNumberGenerator.GetBytes(messageBytes);
            return messageBytes;
        }
    }
}
