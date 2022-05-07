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
        private readonly Oscilloscope _oscilloscope;
        private readonly OscilloscopeViewControlState _oscilloscopeControlState;
        private readonly Func<OscilloscopeViewModel> _oscilloscopeVM;

        public ApplyOscilloscopeConfigCommand(OscilloscopeConfigViewModel oscilloscopeConfigViewModel, OscilloscopeViewControlState oscilloscopeControlState, Func<OscilloscopeViewModel> oscilloscopeVM)
        {
            _oscilloscopeConfigViewModel = oscilloscopeConfigViewModel;
            _oscilloscope = _oscilloscopeConfigViewModel.Oscilloscope;
            _oscilloscopeControlState = oscilloscopeControlState;
            _oscilloscopeVM = oscilloscopeVM;
        }

        public override async Task ExecuteAsync(object parameter)
        {
            await Task.Run(() =>
            {
                _oscilloscope.InsertConfigString(_oscilloscopeConfigViewModel.TempOscilloscopeConfigString.ToList());
                _oscilloscope.InsertChannelSettings(_oscilloscopeConfigViewModel.ChannelSettingsVMtoModel());
                _oscilloscope.InsertTriggerSettings(_oscilloscopeConfigViewModel.TriggerSettingsVMtoModel());


                decimal timebaseScale = _oscilloscopeConfigViewModel.TimebaseScale;
                decimal timebasePosition = _oscilloscopeConfigViewModel.TimebasePosition;
                int waveformFormatIndex = _oscilloscopeConfigViewModel.WaveformFormatIndex;
                bool waveformStreaming = _oscilloscopeConfigViewModel.WaveformStreaming;
                _oscilloscope.InsertOtherSettings(timebaseScale, timebasePosition, waveformFormatIndex, waveformStreaming);


                _oscilloscope.ApplyAllSettingsToDevice();

                _oscilloscopeControlState.OscilloscopeSelectedVM = _oscilloscopeVM();
            });

        }
    }
}
