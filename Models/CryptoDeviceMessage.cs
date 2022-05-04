using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P_SCAAT.Models
{
    internal class CryptoDeviceMessage
    {
        /// <summary>
        /// Notify all subscribers that new message has been created. <see cref="ViewModels.SerialPortRS232ViewModel.SerialPortRS232ViewModel(CryptoDeviceMessage)"/>
        /// </summary>
        public event Action MessageCreation;

        public byte[] MessageBytes { get; set; }
        public int MessageLenght => MessageBytes.Length;
        public DateTime TimeCreated { get; set; }
        public RNGMessageGenerator RNGMessageGenerator { get; private set; }

        /// <summary>
        /// Initialize new <see cref="Models.RNGMessageGenerator"/> with <paramref name="messageLenght"/> as desired byte lenght.
        /// </summary>
        /// <param name="messageLenght"></param>
        public void InitializeRNGMessageGenerator(uint messageLenght)
        {
            RNGMessageGenerator = new RNGMessageGenerator(messageLenght);
        }
        /// <summary>
        /// Generate new message using <see cref="Models.RNGMessageGenerator"/>. Attaches timestamp to <see cref="TimeCreated"/> and invokes <see cref="MessageCreation"/> event.
        /// </summary>
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
