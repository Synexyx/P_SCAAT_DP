using System.Collections.Generic;

namespace P_SCAAT.Models
{
    internal abstract partial class OscilloscopeConfig
    {
        internal class TriggerSettings
        {
            public List<string> TriggerEdgeSourceOptions { get; set; }
            public int TriggerEdgeSourceIndex { get; set; }
            public List<string> TriggerEdgeSlopeOptions { get; set; }
            public int TriggerEdgeSlopeIndex { get; set; }

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
            }
            public TriggerSettings(List<string> triggerEdgeSourceOptions, int triggerEdgeSourceIndex, List<string> triggerEdgeSlopeOptions, int triggerEdgeSlopeIndex)
            {
                TriggerEdgeSourceOptions = new List<string>(triggerEdgeSourceOptions);
                TriggerEdgeSourceIndex = triggerEdgeSourceIndex;
                TriggerEdgeSlopeOptions = new List<string>(triggerEdgeSlopeOptions);
                TriggerEdgeSlopeIndex = triggerEdgeSlopeIndex;
            }
        }







    }


}
