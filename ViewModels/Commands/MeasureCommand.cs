using P_SCAAT.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P_SCAAT.ViewModels.Commands
{
    internal class MeasureCommand : AsyncCoreCommand
    {
        private readonly OscilloscopeViewModel _oscilloscopeViewModel;
        private readonly Oscilloscope _oscilloscope;
        private readonly CryptoDeviceMessage _cryptoDeviceMessage;

        private string _measureButtonContent;
        //private bool _isCancellationRequested;

        private CancellationTokenSource tokenSource = new CancellationTokenSource();


        public MeasureCommand(OscilloscopeViewModel oscilloscopeViewModel, Oscilloscope oscilloscope, CryptoDeviceMessage cryptoDeviceMessage)
        {
            _oscilloscopeViewModel = oscilloscopeViewModel;
            _oscilloscope = oscilloscope;
            _cryptoDeviceMessage = cryptoDeviceMessage;

            //_measureButtonContent = _oscilloscopeViewModel.MeasureButtonContent;
        }
        public override async Task ExecuteAsync(object parameter)
        {
            _measureButtonContent = _oscilloscopeViewModel.MeasureButtonContent;
            if (_measureButtonContent.Equals("start", StringComparison.OrdinalIgnoreCase))
            {
                tokenSource = new CancellationTokenSource();
                _oscilloscopeViewModel.MeasureButtonContent = "CANCEL";

                await Task.Run(() =>
                {
                    try
                    {
                        //ToDo cykl podle počtu záznamů celkem a na soubor
                        //ToDo nastavit osciloskop do čekacího režimu
                        //ToDo await nový task co pošle data
                        //ToDo maybe zeptat se osciloskopu na triggerEvent??
                        //ToDo cykl pro čtení všech source z osciloskopu
                        //ToDo jakmile přečteno -- zjistím po skončení cyklu -- uložit hodnoty do souboru (new Task?)
                        //ToDo continue
                        _cryptoDeviceMessage.InitializeRNGMessageGenerator(_oscilloscopeViewModel.MessageLenght);
                        _ = _oscilloscope.Measure(_cryptoDeviceMessage, _oscilloscopeViewModel.MessageLenght, tokenSource.Token);
                    }
                    catch (OperationCanceledException e) when (e.CancellationToken == tokenSource.Token)
                    {
                        Debug.WriteLine("TASK CANCELED");
                    }
                });
            }
            else
            {
                tokenSource.Cancel(false);
                _oscilloscopeViewModel.MeasureButtonContent = "START";
                //_ = _oscilloscope.Measure(_cryptoDeviceMessage, _messageLenght);
            }




        }
    }
}
