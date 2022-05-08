using System.Security.Cryptography;

namespace P_SCAAT.Models
{
    internal class RNGMessageGenerator
    {
        private readonly byte[] messageBytes;
        private readonly RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
        
        /// <summary>
        /// Creates new <see cref="messageBytes"/> with length defined by <paramref name="messageLength"/>  
        /// </summary>
        /// <param name="messageLength"></param>
        public RNGMessageGenerator(uint messageLength)
        {
            messageBytes = new byte[messageLength];
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
