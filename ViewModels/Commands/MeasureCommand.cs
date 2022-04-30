using P_SCAAT.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        //ToDo implement through this
        private Measurement measurement = new Measurement();


        public MeasureCommand(OscilloscopeViewModel oscilloscopeViewModel, Oscilloscope oscilloscope, CryptoDeviceMessage cryptoDeviceMessage)
        {
            _oscilloscopeViewModel = oscilloscopeViewModel;
;

            _oscilloscope = oscilloscope;
            _cryptoDeviceMessage = cryptoDeviceMessage;

            _oscilloscopeViewModel.PropertyChanged += OnOscilloscopeViewModel_PropertyChanged;
            //_measureButtonContent = _oscilloscopeViewModel.MeasureButtonContent;
        }

        private void _oscilloscopeViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override bool CanExecute(object parameter)
        {
            return _oscilloscopeViewModel.IsSessionOpen;
        }
        public override async Task ExecuteAsync(object parameter)
        {
            _measureButtonContent = _oscilloscopeViewModel.MeasureButtonContent;
            if (_measureButtonContent.Equals(_oscilloscopeViewModel.MeasureButtonContentStart, StringComparison.OrdinalIgnoreCase))
            {
                tokenSource = new CancellationTokenSource();
                _oscilloscopeViewModel.MeasureButtonContent = _oscilloscopeViewModel.MeasureButtonContentCancel;

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
                tokenSource.Cancel();
                _oscilloscopeViewModel.MeasureButtonContent = _oscilloscopeViewModel.MeasureButtonContentStart;
                //_ = _oscilloscope.Measure(_cryptoDeviceMessage, _messageLenght);
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
