using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using P_SCAAT.Exceptions;
using P_SCAAT.Models;
using P_SCAAT.ViewModels;

namespace P_SCAAT.ViewModels.Commands
{
    internal class ControlSessionCommand : AsyncIsExcecutingCoreCommand
    {
        private readonly SessionDeviceVM _sessionDeviceVM;
        public ControlSessionCommand(SessionDeviceVM sessionDeviceVM)
        {
            _sessionDeviceVM = sessionDeviceVM;

            _sessionDeviceVM.PropertyChanged += OnSessionDeviceViewModel_PropertyChanged;
        }

        public override bool CanExecute(object parameter)
        {
            return !IsExecuting && (parameter.ToString().Equals("OPEN", StringComparison.OrdinalIgnoreCase)
                ? _sessionDeviceVM.IsSessionClosed
                : _sessionDeviceVM.IsSessionOpen);
        }

        public override async Task ExecuteAsync(object parameter)
        {
            _sessionDeviceVM.ChangingSession = true;

            if (parameter.ToString().Equals("OPEN", StringComparison.OrdinalIgnoreCase))
            {
                await Task.Run(() =>
                {
                    try
                    {
                        _sessionDeviceVM.SessionDevice.OpenSession(_sessionDeviceVM.SelectedAvailableResource);
                    }
                    catch (SessionControlException sessionCtrlExp)
                    {
                        _sessionDeviceVM.SessionDevice.CloseSession();
                        _sessionDeviceVM.ErrorMessages.Add(sessionCtrlExp);
                    }
                });
            }

            else if (parameter.ToString().Equals("CLOSE", StringComparison.OrdinalIgnoreCase))
            {
                _sessionDeviceVM.SessionDevice.CloseSession();
            }

            _sessionDeviceVM.ChangingSession = false;
        }

        private void OnSessionDeviceViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SessionDeviceVM.IsSessionOpen))
            {
                OnCanExecuteChanged();
            }
        }
    }
}
