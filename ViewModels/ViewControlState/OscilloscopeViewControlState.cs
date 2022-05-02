using System;

namespace P_SCAAT.ViewModels.ViewControlState
{
    /// <summary>
    /// Allows App to dynamically switch between <see cref="OscilloscopeViewModel"/> and <see cref="OscilloscopeConfigViewModel"/>.
    /// </summary>
    //ToDo napsat správně dokumentaci
    internal class OscilloscopeViewControlState
    {
        private CorePropChangedVM _oscilloscopeSelectedVM;
        public CorePropChangedVM OscilloscopeSelectedVM
        {
            get => _oscilloscopeSelectedVM;
            set
            {
                _oscilloscopeSelectedVM?.Dispose();
                _oscilloscopeSelectedVM = value;
                OnOscilloscopeConfigViewSwitch();
            }
        }

        public event Action OscilloscopeConfigViewSwitched;
        private void OnOscilloscopeConfigViewSwitch()
        {
            OscilloscopeConfigViewSwitched?.Invoke();
        }

    }
}
