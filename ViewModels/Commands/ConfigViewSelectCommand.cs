using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P_SCAAT.Models;
using P_SCAAT.ViewModels;
using P_SCAAT.ViewModels.ViewControlState;

namespace P_SCAAT.ViewModels.Commands
{
    //internal class OscilloscopeConfigViewSelectCommand : CoreCommand
    internal class ConfigViewSelectCommand : AsyncIsExcecutingCoreCommand
    {
        private readonly OscilloscopeViewModel _oscilloscopeViewModel;
        private readonly Oscilloscope _osciloscope;
        private readonly OscilloscopeViewControlState _oscilloscopeControlState;
        private readonly Func<OscilloscopeConfigViewModel> _oscilloscopeConfigVM;

        public ConfigViewSelectCommand(OscilloscopeViewModel oscilloscopeViewModel, Oscilloscope oscilloscope, OscilloscopeViewControlState oscilloscopeControlState, Func<OscilloscopeConfigViewModel> oscilloscopeConfigVM)
        {
            _oscilloscopeViewModel = oscilloscopeViewModel;
            _osciloscope = oscilloscope;
            _oscilloscopeControlState = oscilloscopeControlState;
            _oscilloscopeConfigVM = oscilloscopeConfigVM;

            _oscilloscopeViewModel.PropertyChanged += OnOscilloscopeViewModel_PropertyChanged;
        }

        public override bool CanExecute(object parameter)
        {
            return !IsExecuting && _oscilloscopeViewModel.IsSessionOpen;
        }
        //public override void Execute(object parameter)
        //{
        //    //_osciloscope.UpdateAllResources();
        //    //_osciloscopeControlState.OsciloscopeSelectedVM = new OsciloscopeConfigViewModel(_osciloscope, _osciloscopeControlState);
        //    _oscilloscopeControlState.OscilloscopeSelectedVM = _oscilloscopeConfigVM();
        //}

        public override async Task ExecuteAsync(object parameter)
        {
            await Task.Run(() => _osciloscope.SynchronizeConfig());
            _oscilloscopeControlState.OscilloscopeSelectedVM = _oscilloscopeConfigVM();
        }
        private void OnOscilloscopeViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OscilloscopeViewModel.IsSessionOpen))
            {
                OnCanExecuteChanged();
            }
        }
    }
}
