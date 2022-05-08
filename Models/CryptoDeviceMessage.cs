using System;

namespace P_SCAAT.Models
{
    /// <summary>
    /// Message for cryptographic device model
    /// </summary>
    internal class CryptoDeviceMessage
    {
        /// <summary>
        /// Notify all subscribers that new message has been created. <see cref="ViewModels.SerialPortRS232ViewModel.SerialPortRS232ViewModel(CryptoDeviceMessage)"/>
        /// </summary>
        public event Action MessageCreation;

        #region Properties
        public byte[] MessageBytes { get; set; }
        public int MessageLength => MessageBytes.Length;
        public DateTime TimeCreated { get; set; }
        public RNGMessageGenerator RNGMessageGenerator { get; private set; }
        #endregion

        /// <summary>
        /// Initialize new <see cref="Models.RNGMessageGenerator"/> with <paramref name="messageLength"/> as desired byte length.
        /// </summary>
        /// <param name="messageLength"></param>
        public void InitializeRNGMessageGenerator(uint messageLength)
        {
            RNGMessageGenerator = new RNGMessageGenerator(messageLength);
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
