using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P_SCAAT.Models;
using static P_SCAAT.Models.OscilloscopeConfig.ChannelSettings;

namespace P_SCAAT.ViewModels
{
    internal class ChannelSettingsViewModel : CorePropChangedVM
    {
        #region Properties
        private string _channelLabel;
        private bool _channelDisplay;
        private decimal _channelScale;
        private decimal _channelOffset;
        private List<string> _channelCouplingModes;
        private int _channelCouplingIndex;

        public int ChannelNumber { get; private set; }
        public string ChannelLabel
        {
            get => _channelLabel;
            set
            {
                _channelLabel = value;
                if (_channelLabel.Length > 16) { _channelLabel = _channelLabel.Substring(0, 16); }
                OnPropertyChanged(nameof(ChannelLabel));
            }
        }
        public bool ChannelDisplay
        {
            get => _channelDisplay;
            set { _channelDisplay = value; OnPropertyChanged(nameof(ChannelDisplay)); }
        }
        public decimal ChannelScale
        {
            get => _channelScale;
            set { _channelScale = value; OnPropertyChanged(nameof(ChannelScale)); }
        }
        public decimal ChannelOffset
        {
            get => _channelOffset;
            set { _channelOffset = value; OnPropertyChanged(nameof(ChannelOffset)); }
        }
        public List<string> ChannelCouplingModes
        {
            get => _channelCouplingModes;
            set { _channelCouplingModes = value; OnPropertyChanged(nameof(ChannelCouplingModes)); }
        }
        public int ChannelCouplingIndex
        {
            get => _channelCouplingIndex;
            set { _channelCouplingIndex = value; OnPropertyChanged(nameof(ChannelCouplingIndex)); }
        }
        #endregion
        public ChannelSettingsViewModel(OscilloscopeConfig.ChannelSettings channelSettingsToCopy)
        {
            ChannelNumber = channelSettingsToCopy.ChannelNumber;
            ChannelLabel = channelSettingsToCopy.ChannelLabel;
            ChannelDisplay = channelSettingsToCopy.ChannelDisplay;
            ChannelScale = channelSettingsToCopy.ChannelScale;
            ChannelOffset = channelSettingsToCopy.ChannelOffset;
            ChannelCouplingModes = new List<string>(channelSettingsToCopy.ChannelCouplingModes);
            ChannelCouplingIndex = channelSettingsToCopy.ChannelCouplingIndex;
        }



    }
}
