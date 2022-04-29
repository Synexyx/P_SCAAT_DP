using P_SCAAT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P_SCAAT.ViewModels.Commands
{
    internal class MeasureCommand : AsyncCoreCommand
    {
        private readonly Oscilloscope _oscilloscope;
        private readonly CryptoDeviceMessage _cryptoDeviceMessage;
        private readonly int _messageLenght;

        public MeasureCommand(Oscilloscope oscilloscope, CryptoDeviceMessage cryptoDeviceMessage, int messageLenght)
        {
            _oscilloscope = oscilloscope;
            _cryptoDeviceMessage = cryptoDeviceMessage;
            _messageLenght = messageLenght;
        }
        public override async Task ExecuteAsync(object parameter)
        {
            await Task.Run(async () =>
            {
                await _oscilloscope.Measure(_cryptoDeviceMessage, _messageLenght);
            });
        }
    }
}
