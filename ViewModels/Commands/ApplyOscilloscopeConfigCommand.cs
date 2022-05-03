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
    internal class ApplyOscilloscopeConfigCommand : CoreCommand
    {

        //ToDo async? určitě protože budu chtít Thread.Sleep() před write

        //private readonly Oscilloscope _oscilloscope;
        private readonly OscilloscopeConfigViewModel _oscilloscopeConfigViewModel;
        private readonly OscilloscopeViewControlState _oscilloscopeControlState;
        private readonly Func<OscilloscopeViewModel> _oscilloscopeVM;

        //public ApplyOscilloscopeConfigCommand(OscilloscopeConfigViewModel oscilloscopeConfigViewModel, Oscilloscope oscilloscope, OscilloscopeViewControlState oscilloscopeControlState, Func<OscilloscopeViewModel> oscilloscopeVM)
        public ApplyOscilloscopeConfigCommand(OscilloscopeConfigViewModel oscilloscopeConfigViewModel, OscilloscopeViewControlState oscilloscopeControlState, Func<OscilloscopeViewModel> oscilloscopeVM)
        {
            _oscilloscopeConfigViewModel = oscilloscopeConfigViewModel;
            //_oscilloscope = oscilloscope;
            _oscilloscopeControlState = oscilloscopeControlState;
            _oscilloscopeVM = oscilloscopeVM;
        }

        public override void Execute(object parameter)
        {
            _oscilloscopeConfigViewModel.Oscilloscope.InsertNewConfigString(_oscilloscopeConfigViewModel.TempOscilloscopeConfigString.ToList());

            //ToDo dát tam config string a z něj updatovat properties??
            _oscilloscopeConfigViewModel.Oscilloscope.InsertNewChannelSettings(_oscilloscopeConfigViewModel.ChannelSettingsVMtoModel());
            _oscilloscopeConfigViewModel.Oscilloscope.InsertTriggerSettings(_oscilloscopeConfigViewModel.TriggerSettingsVMtoModel());


            decimal timebaseScale = _oscilloscopeConfigViewModel.TimebaseScale;
            decimal timebasePosition = _oscilloscopeConfigViewModel.TimebasePosition;
            int waveformFormatIndex = _oscilloscopeConfigViewModel.WaveformFormatIndex;
            bool waveformStreaming = _oscilloscopeConfigViewModel.WaveformStreaming;
            _oscilloscopeConfigViewModel.Oscilloscope.InsertOtherSettings(timebaseScale, timebasePosition, waveformFormatIndex, waveformStreaming);


            _oscilloscopeConfigViewModel.Oscilloscope.ApplyAllSettingsToDevice();




            //_oscilloscope.ListCurrentCommands();
            _oscilloscopeConfigViewModel.Oscilloscope.ListCurrentCommands();


            _oscilloscopeControlState.OscilloscopeSelectedVM = _oscilloscopeVM();
        }
    }
}
