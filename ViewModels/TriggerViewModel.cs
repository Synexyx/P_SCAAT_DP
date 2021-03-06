using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P_SCAAT.Models;

namespace P_SCAAT.ViewModels
{
    internal class TriggerViewModel : OscilloscopeValueConversionVM
    {
        #region Properties
        private List<string> _triggerEdgeSourceOptions;
        private int _triggerEdgeSourceIndex;
        private List<string> _triggerEdgeSlopeOptions;
        private int _triggerEdgeSlopeIndex;
        private Dictionary<string, decimal> _triggerLevel;

        public List<string> TriggerEdgeSourceOptions
        {
            get => _triggerEdgeSourceOptions;
            set { _triggerEdgeSourceOptions = value; OnPropertyChanged(nameof(TriggerEdgeSourceOptions)); }
        }
        public int TriggerEdgeSourceIndex
        {
            get => _triggerEdgeSourceIndex;
            set { _triggerEdgeSourceIndex = value; OnPropertyChanged(nameof(TriggerEdgeSourceIndex)); OnPropertyChanged(nameof(TriggerLevelForTextBox)); }
        }
        public List<string> TriggerEdgeSlopeOptions
        {
            get => _triggerEdgeSlopeOptions;
            set { _triggerEdgeSlopeOptions = value; OnPropertyChanged(nameof(TriggerEdgeSlopeOptions)); }
        }
        public int TriggerEdgeSlopeIndex
        {
            get => _triggerEdgeSlopeIndex;
            set { _triggerEdgeSlopeIndex = value; OnPropertyChanged(nameof(TriggerEdgeSlopeIndex)); }
        }

        public Dictionary<string, decimal> TriggerLevel
        {
            get => _triggerLevel;
            set { _triggerLevel = value; OnPropertyChanged(nameof(TriggerLevel)); }
        }

        public string TriggerLevelForTextBox
        {
            get
            {
                string selectedSource = TriggerEdgeSourceOptions.ElementAtOrDefault(TriggerEdgeSourceIndex);
                _ = TriggerLevel.TryGetValue(selectedSource, out decimal triggerLevel);
                return DecimalToString(triggerLevel, "V");
            }
            set
            {
                decimal numberValue = StringToDecimal(value);
                string selectedSource = TriggerEdgeSourceOptions.ElementAtOrDefault(TriggerEdgeSourceIndex);
                if (TriggerLevel.ContainsKey(selectedSource))
                {
                    TriggerLevel[selectedSource] = numberValue;
                }
                else
                {
                    TriggerLevel.Add(selectedSource, numberValue);
                }
                OnPropertyChanged(nameof(TriggerLevelForTextBox));
                OnPropertyChanged(nameof(TriggerLevel));
            }
        }
        #endregion
        public TriggerViewModel(TriggerSettings trigger)
        {
            TriggerEdgeSourceOptions = new List<string>(trigger.TriggerEdgeSourceOptions);
            TriggerEdgeSourceIndex = trigger.TriggerEdgeSourceIndex;
            TriggerEdgeSlopeOptions = new List<string>(trigger.TriggerEdgeSlopeOptions);
            TriggerEdgeSlopeIndex = trigger.TriggerEdgeSlopeIndex;
            TriggerLevel = new Dictionary<string, decimal>(trigger.TriggerLevel);
        }
    }
}
