using System.Collections.Generic;

namespace P_SCAAT.Models
{

    internal class TriggerSettings
    {
        #region Properties
        public List<string> TriggerEdgeSourceOptions { get; set; }
        public int TriggerEdgeSourceIndex { get; set; }
        public List<string> TriggerEdgeSlopeOptions { get; set; }
        public int TriggerEdgeSlopeIndex { get; set; }
        public Dictionary<string, decimal> TriggerLevel { get; set; }
        #endregion

        /// <summary>
        /// Creates blank <see cref="TriggerSettings"/> with default property values.
        /// </summary>
        public TriggerSettings()
        {
            TriggerEdgeSourceOptions = new List<string>();
            TriggerEdgeSourceIndex = 0;
            TriggerEdgeSlopeOptions = new List<string>
                {
                    "Positive",
                    "Negative",
                    "Either"
                };
            TriggerEdgeSlopeIndex = 0;
            TriggerLevel = new Dictionary<string, decimal>();
        }
        /// <summary>
        /// Creates hard copy of <see cref="TriggerSettings"/> using all known property values.
        /// </summary>
        public TriggerSettings(List<string> triggerEdgeSourceOptions, int triggerEdgeSourceIndex, List<string> triggerEdgeSlopeOptions, int triggerEdgeSlopeIndex, Dictionary<string, decimal> triggerLevel)
        {
            TriggerEdgeSourceOptions = new List<string>(triggerEdgeSourceOptions);
            TriggerEdgeSourceIndex = triggerEdgeSourceIndex;
            TriggerEdgeSlopeOptions = new List<string>(triggerEdgeSlopeOptions);
            TriggerEdgeSlopeIndex = triggerEdgeSlopeIndex;
            TriggerLevel = triggerLevel;
        }
    }
}



