using P_SCAAT.Exceptions;
using P_SCAAT.Models;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace P_SCAAT.ViewModels.Commands
{
    internal class ManualControlCommand : CoreCommand
    {
        private readonly OscilloscopeViewModel _oscilloscopeViewModel;
        private readonly Oscilloscope _oscilloscope;

        public ManualControlCommand(OscilloscopeViewModel oscilloscopeViewModel, Oscilloscope oscilloscope)
        {
            _oscilloscopeViewModel = oscilloscopeViewModel;
            _oscilloscope = oscilloscope;

            _oscilloscopeViewModel.PropertyChanged += OnOscilloscopeViewModel_PropertyChanged;
        }

        public override bool CanExecute(object parameter)
        {
            return _oscilloscopeViewModel.IsSessionOpen;
        }
        public override void Execute(object parameter)
        {
            try
            {
                switch (parameter.ToString())
                {
                    case "WRITE":
                        _oscilloscope.SendData(_oscilloscopeViewModel.ManualMessageWrite);
                        break;
                    case "READ":
                        _oscilloscopeViewModel.ManualMessageRead = _oscilloscope.ReadStringData();
                        break;
                    case "QUERY":
                        _oscilloscopeViewModel.ManualMessageRead = _oscilloscope.QueryData(_oscilloscopeViewModel.ManualMessageWrite);
                        break;
                    default:
                        break;
                }
            }
            catch (SessionCommunicationException ex)
            {
                _oscilloscopeViewModel.ErrorMessages.Add(ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
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