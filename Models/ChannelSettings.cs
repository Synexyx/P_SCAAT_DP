using System.Collections.Generic;

namespace P_SCAAT.Models
{
    /// <summary>
    /// Oscilloscope channel model
    /// </summary>
    internal class ChannelSettings
    {
        #region Properties
        public int ChannelNumber { get; }
        public string ChannelLabel { get; set; }
        public bool ChannelDisplay { get; set; }
        public decimal ChannelScale { get; set; }
        public decimal ChannelOffset { get; set; }
        public List<string> ChannelCouplingModes { get; set; }
        public int ChannelCouplingIndex { get; set; }
        #endregion
        /// <summary>
        /// Creates blank <see cref="ChannelSettings"/> with <paramref name="channelNumber"/> and default property values.
        /// </summary>
        public ChannelSettings(int channelNumber)
        {
            ChannelNumber = channelNumber;
            ChannelLabel = "Channel " + channelNumber;
            ChannelDisplay = false;
            ChannelScale = 0;
            ChannelOffset = 0;

            ChannelCouplingModes = new List<string>
                {
                    "AC",
                    "DC"
                };
            ChannelCouplingIndex = 0;
        }
        /// <summary>
        /// Creates hard copy of <see cref="ChannelSettings"/> using all known property values.
        /// </summary>
        public ChannelSettings(int channelNumber, string label, bool channelDisplay,
            decimal channelScale, decimal channelOffset, List<string> channelCouplingModes, int channelCouplingIndex)
        {
            ChannelNumber = channelNumber;
            ChannelLabel = label;
            ChannelDisplay = channelDisplay;
            ChannelScale = channelScale;
            ChannelOffset = channelOffset;
            ChannelCouplingModes = new List<string>(channelCouplingModes);
            ChannelCouplingIndex = channelCouplingIndex;
        }

    }
}

