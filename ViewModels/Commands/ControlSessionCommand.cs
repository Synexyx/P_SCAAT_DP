using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using P_SCAAT.Models;
using P_SCAAT.ViewModels;

namespace P_SCAAT.ViewModels.Commands
{
    internal class ControlSessionCommand : CoreCommand
    {
        //private readonly OscilloscopeViewModel _oscilloscopeViewModel;

        //public ControlOscilloscopeSessionCommand(OscilloscopeViewModel oscilloscopeViewModel)
        private readonly SessionDeviceVM _sessionDeviceVM;
        public ControlSessionCommand(SessionDeviceVM sessionDeviceVM)
        {
            //_oscilloscopeViewModel = oscilloscopeViewModel;
            _sessionDeviceVM = sessionDeviceVM;

            _sessionDeviceVM.PropertyChanged += OnSessionDeviceViewModel_PropertyChanged;
            //_oscilloscopeViewModel.PropertyChanged += OnOscilloscopeViewModel_PropertyChanged;
        }

        public override bool CanExecute(object parameter)
        {
            //return parameter.ToString().Equals("OPEN", StringComparison.OrdinalIgnoreCase)
            //    ? !_oscilloscopeViewModel.IsSessionOpen
            //    : _oscilloscopeViewModel.IsSessionOpen;
            return parameter.ToString().Equals("OPEN", StringComparison.OrdinalIgnoreCase)
                ? _sessionDeviceVM.IsSessionClosed
                : _sessionDeviceVM.IsSessionOpen;
        }
        public override void Execute(object parameter)
        {
            //_oscilloscopeViewModel.ChangingSession = true;
            _sessionDeviceVM.ChangingSession = true;

            //if (parameter.ToString().Equals("OPEN", StringComparison.Ordinal) && !_oscilloscopeViewModel.Oscilloscope.IsSessionOpen)
            if (parameter.ToString().Equals("OPEN", StringComparison.OrdinalIgnoreCase))
            {
                //_oscilloscopeViewModel.Oscilloscope.OpenSession(_oscilloscopeViewModel.SelectedAvailableOscilloscopes);
                _sessionDeviceVM.SessionDevice.OpenSession(_sessionDeviceVM.SelectedAvailableResource);
            }

            else if (parameter.ToString().Equals("CLOSE", StringComparison.OrdinalIgnoreCase))
            {
                //_oscilloscopeViewModel.Oscilloscope.CloseSession();
                _sessionDeviceVM.SessionDevice.CloseSession();
            }

            //_oscilloscopeViewModel.ChangingSession = false;
            _sessionDeviceVM.ChangingSession = false;
        }
        private void OnSessionDeviceViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == nameof(OscilloscopeViewModel.IsSessionOpen))
            if (e.PropertyName == nameof(SessionDeviceVM.IsSessionOpen))
            {
                OnCanExecuteChanged();
            }
        }
    }
}
