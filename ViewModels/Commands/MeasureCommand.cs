using P_SCAAT.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        public MeasureCommand(OscilloscopeViewModel oscilloscopeViewModel, Oscilloscope oscilloscope, CryptoDeviceMessage cryptoDeviceMessage)
        {
            _oscilloscopeViewModel = oscilloscopeViewModel;
            ;

            _oscilloscope = oscilloscope;
            _cryptoDeviceMessage = cryptoDeviceMessage;

            _oscilloscopeViewModel.PropertyChanged += OnOscilloscopeViewModel_PropertyChanged;
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
                _oscilloscopeViewModel.ProgressBarValue = 0;
                _oscilloscopeViewModel.MeasureButtonContent = _oscilloscopeViewModel.MeasureButtonContentCancel;

                string fileNameSessionID = DateTime.Now.ToString("HHmmss");

                await Task.Run(async () =>
                {
                    try
                    {
                        if (tokenSource.Token.IsCancellationRequested)
                        {
                            Debug.WriteLine("CANCELED");
                            tokenSource.Token.ThrowIfCancellationRequested();
                        }
                        _cryptoDeviceMessage.InitializeRNGMessageGenerator(_oscilloscopeViewModel.MessageLenght);
                        int perFile = 0;
                        int fileNumber = 0;


                        for (int total = 0; total < _oscilloscopeViewModel.TracesTotal; total++)
                        {
                            //    //ToDo nastavit osciloskop do čekacího režimu
                            _oscilloscope.MeasurePrep();
                            //    //ToDo await nový task co pošle data
                            await Task.Run(() => { _cryptoDeviceMessage.GenerateNewMessage(); });
                            //    //ToDo maybe zeptat se osciloskopu na triggerEvent??
                            //    //ToDo cykl pro čtení všech source z osciloskopu
                            if (tokenSource.Token.IsCancellationRequested)
                            {
                                Debug.WriteLine("CANCELED");
                                tokenSource.Token.ThrowIfCancellationRequested();
                            }
                            //ToDo save to buffer ??
                            string response = Convert.ToBase64String(_oscilloscope.GetMeasuredData("src"));
                            //string response = BitConverter.ToString(_oscilloscope.GetMeasuredData());


                            //    //ToDo jakmile přečteno -- zjistím po skončení cyklu -- uložit hodnoty do souboru (new Task?)
                            if (perFile == _oscilloscopeViewModel.TracesPerFile && _oscilloscopeViewModel.TracesPerFile != 0)
                            {
                                fileNumber++;
                                perFile = 0;
                            }
                            StringBuilder stringBuilder = new StringBuilder();
                            string messageBytesToBase64 = Convert.ToBase64String(_cryptoDeviceMessage.MessageBytes);
                            _ = stringBuilder.Append($"{_cryptoDeviceMessage.TimeCreated:T},{messageBytesToBase64},{response}{Environment.NewLine}");
                            Debug.WriteLine(total);
                            string directoryToSave = Path.GetFullPath(@"..\..\Measurment");
                            File.AppendAllText($"{directoryToSave}\\{DateTime.Now:yyyyMMdd}-{fileNameSessionID}-measurment-{fileNumber.ToString($"##0")}.txt", stringBuilder.ToString());
                            _ = stringBuilder.Clear();

                            perFile++;
                            _oscilloscopeViewModel.ProgressBarValue = total + 1;
                            if (tokenSource.Token.IsCancellationRequested)
                            {
                                Debug.WriteLine("CANCELED");
                                tokenSource.Token.ThrowIfCancellationRequested();
                            }
                            //tokenSource.Token.ThrowIfCancellationRequested();
                            //for (int perFile = 0; perFile < _oscilloscopeViewModel.TracesPerFile; perFile++)
                            //{
                            //    foreach(var source in _oscilloscopeViewModel.WaveformSource)
                            //    {
                            //        _cryptoDeviceMessage.GenerateNewMessage();
                            //    }
                            //}
                        }
                    }
                    //try
                    //{
                    //    //ToDo cykl podle počtu záznamů celkem a na soubor



                    //    //ToDo continue

                    //}
                    catch (OperationCanceledException e) when (e.CancellationToken == tokenSource.Token)
                    {
                        Debug.WriteLine("TASK CANCELED");
                        _oscilloscopeViewModel.ProgressBarValue = 0;
                    }
                }, tokenSource.Token);
            }
            tokenSource.Cancel();
            _oscilloscopeViewModel.MeasureButtonContent = _oscilloscopeViewModel.MeasureButtonContentStart;
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
