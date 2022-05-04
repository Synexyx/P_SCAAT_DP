using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P_SCAAT.Models;
using P_SCAAT.ViewModels;
using P_SCAAT.ViewModels.ViewControlState;

namespace P_SCAAT.ViewModels.Commands
{
    internal class ApplyOscilloscopeConfigCommand : AsyncIsExcecutingCoreCommand
    {
        private readonly OscilloscopeConfigViewModel _oscilloscopeConfigViewModel;
        private readonly OscilloscopeViewControlState _oscilloscopeControlState;
        private readonly Func<OscilloscopeViewModel> _oscilloscopeVM;

        public ApplyOscilloscopeConfigCommand(OscilloscopeConfigViewModel oscilloscopeConfigViewModel, OscilloscopeViewControlState oscilloscopeControlState, Func<OscilloscopeViewModel> oscilloscopeVM)
        {
            _oscilloscopeConfigViewModel = oscilloscopeConfigViewModel;
            _oscilloscopeControlState = oscilloscopeControlState;
            _oscilloscopeVM = oscilloscopeVM;
        }

        public override async Task ExecuteAsync(object parameter)
        {
            await Task.Run(() =>
            {
                _oscilloscopeConfigViewModel.Oscilloscope.InsertNewConfigString(_oscilloscopeConfigViewModel.TempOscilloscopeConfigString.ToList());

                _oscilloscopeConfigViewModel.Oscilloscope.InsertNewChannelSettings(_oscilloscopeConfigViewModel.ChannelSettingsVMtoModel());
                _oscilloscopeConfigViewModel.Oscilloscope.InsertTriggerSettings(_oscilloscopeConfigViewModel.TriggerSettingsVMtoModel());


                decimal timebaseScale = _oscilloscopeConfigViewModel.TimebaseScale;
                decimal timebasePosition = _oscilloscopeConfigViewModel.TimebasePosition;
                int waveformFormatIndex = _oscilloscopeConfigViewModel.WaveformFormatIndex;
                bool waveformStreaming = _oscilloscopeConfigViewModel.WaveformStreaming;
                _oscilloscopeConfigViewModel.Oscilloscope.InsertOtherSettings(timebaseScale, timebasePosition, waveformFormatIndex, waveformStreaming);


                _oscilloscopeConfigViewModel.Oscilloscope.ApplyAllSettingsToDevice();


                _oscilloscopeConfigViewModel.Oscilloscope.ListCurrentCommands();


                _oscilloscopeControlState.OscilloscopeSelectedVM = _oscilloscopeVM();
            });

        }
    }
}
