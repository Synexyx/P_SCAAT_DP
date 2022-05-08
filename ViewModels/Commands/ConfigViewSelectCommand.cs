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
    /// <summary>
    /// Synchronize all settings with real device and open oscilloscope config UI. 
    /// </summary>
    internal class ConfigViewSelectCommand : AsyncIsExcecutingCoreCommand
    {
        private readonly OscilloscopeViewModel _oscilloscopeViewModel;
        private readonly Oscilloscope _oscilloscope;
        private readonly OscilloscopeViewControlState _oscilloscopeControlState;
        private readonly Func<OscilloscopeConfigViewModel> _oscilloscopeConfigVM;

        public ConfigViewSelectCommand(OscilloscopeViewModel oscilloscopeViewModel, Oscilloscope oscilloscope, OscilloscopeViewControlState oscilloscopeControlState, Func<OscilloscopeConfigViewModel> oscilloscopeConfigVM)
        {
            _oscilloscopeViewModel = oscilloscopeViewModel;
            _oscilloscope = oscilloscope;
            _oscilloscopeControlState = oscilloscopeControlState;
            _oscilloscopeConfigVM = oscilloscopeConfigVM;

            _oscilloscopeViewModel.PropertyChanged += OnOscilloscopeViewModel_PropertyChanged;
        }

        public override bool CanExecute(object parameter)
        {
            return !IsExecuting && _oscilloscopeViewModel.IsSessionOpen;
        }

        public override async Task ExecuteAsync(object parameter)
        {
            try
            {
                await Task.Run(() => _oscilloscope.SynchronizeConfig());
            }
            catch (Exception ex)
            {
                _oscilloscopeViewModel.ErrorMessages.Add(ex);
            }
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
