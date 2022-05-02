using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P_SCAAT.ViewModels.Commands
{
    internal class RadioButtonEdgeSlopeCommand : CoreCommand
    {
        private readonly OscilloscopeConfigViewModel _oscilloscopeConfigViewModel;

        public RadioButtonEdgeSlopeCommand(OscilloscopeConfigViewModel oscilloscopeConfigViewModel)
        {
            _oscilloscopeConfigViewModel = oscilloscopeConfigViewModel;
        }

        public override void Execute(object parameter)
        {
            _ = int.TryParse(parameter as string, out int selectedIndex);
            _oscilloscopeConfigViewModel.TriggerVM.TriggerEdgeSlopeIndex = selectedIndex;
        }
    }
}
