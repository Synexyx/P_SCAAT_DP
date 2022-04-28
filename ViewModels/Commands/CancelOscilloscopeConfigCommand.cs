using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P_SCAAT.ViewModels;
using P_SCAAT.ViewModels.ViewControlState;

namespace P_SCAAT.ViewModels.Commands
{
    internal class CancelOscilloscopeConfigCommand : CoreCommand
    {
        private readonly OscilloscopeViewControlState _oscilloscopeControlState;
        private readonly Func<OscilloscopeViewModel> _oscilloscopeVM;

        public CancelOscilloscopeConfigCommand(OscilloscopeViewControlState oscilloscopeControlState, Func<OscilloscopeViewModel> oscilloscopeVM)
        {
            _oscilloscopeControlState = oscilloscopeControlState;
            _oscilloscopeVM = oscilloscopeVM;
        }

        public override void Execute(object parameter)
        {
            if (OscilloscopeConfigViewModel.CancelCommandMessageBox())
            {
                _oscilloscopeControlState.OscilloscopeSelectedVM = _oscilloscopeVM();
            }
        }
    }
}
