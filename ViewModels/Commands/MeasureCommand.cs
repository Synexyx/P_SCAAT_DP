﻿using P_SCAAT.Models;
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

                await Task.Run(async () =>
                {
                    _cryptoDeviceMessage.InitializeRNGMessageGenerator(_oscilloscopeViewModel.MessageLenght);
                    int perFile = 0;
                    int fileNumber = 0;

                    for (int total = 0; total < _oscilloscopeViewModel.TracesTotal; total++)
                    {
                        //    //ToDo nastavit osciloskop do čekacího režimu
                        _oscilloscope.MeasurePrep();
                        await Task.Run(() => { _cryptoDeviceMessage.GenerateNewMessage(); });
                        //    //ToDo maybe zeptat se osciloskopu na triggerEvent??
                        //    //ToDo cykl pro čtení všech source z osciloskopu
                        //MemoryStream memoryStream = new MemoryStream();

                        string response = Convert.ToBase64String(_oscilloscope.GetMeasuredData());
                        //string response = BitConverter.ToString(_oscilloscope.GetMeasuredData());
                        //ToDo save to buffer ??


                        //    //ToDo jakmile přečteno -- zjistím po skončení cyklu -- uložit hodnoty do souboru (new Task?)
                        if (perFile == _oscilloscopeViewModel.TracesPerFile && _oscilloscopeViewModel.TracesPerFile != 0)
                        {
                            fileNumber++;
                            //ToDo await Task.Run
                            //ToDo in measurment class method
                            //Debug.WriteLine(stringBuilder.ToString());
                            Debug.WriteLine("SAVING FILE");

                            //await Task.Run(() => { });
                            perFile = 0;
                        }
                        StringBuilder stringBuilder = new StringBuilder();
                        string messageBytesToBase64 = Convert.ToBase64String(_cryptoDeviceMessage.MessageBytes);
                        _ = stringBuilder.Append($"{_cryptoDeviceMessage.TimeCreated:T},{messageBytesToBase64},{response}{Environment.NewLine}");
                        Debug.WriteLine(total);
                        string directoryToSave = Path.GetFullPath(@"..\..\Measurment");
                        File.AppendAllText($"{directoryToSave}\\{DateTime.Now:dd-MM}-Measurment-{fileNumber}.txt", stringBuilder.ToString());
                        _ = stringBuilder.Clear();

                        perFile++;
                        _oscilloscopeViewModel.ProgressBarValue = total + 1;
                        //tokenSource.Token.ThrowIfCancellationRequested();
                        //ToDo nezapomentou cancel
                        //for (int perFile = 0; perFile < _oscilloscopeViewModel.TracesPerFile; perFile++)
                        //{
                        //    foreach(var source in _oscilloscopeViewModel.WaveformSource)
                        //    {
                        //        _cryptoDeviceMessage.GenerateNewMessage();
                        //    }
                        //}
                    }
                    //try
                    //{
                    //    //ToDo cykl podle počtu záznamů celkem a na soubor
                    //    //ToDo await nový task co pošle data



                    //    //ToDo continue

                    //    _ = _oscilloscope.Measure(_cryptoDeviceMessage, _oscilloscopeViewModel.MessageLenght, tokenSource.Token);
                    //}
                    //catch (OperationCanceledException e) when (e.CancellationToken == tokenSource.Token)
                    //{
                    //    Debug.WriteLine("TASK CANCELED");
                    //}
                });
            }
            tokenSource.Cancel();
            _oscilloscopeViewModel.MeasureButtonContent = _oscilloscopeViewModel.MeasureButtonContentStart;
            //_ = _oscilloscope.Measure(_cryptoDeviceMessage, _messageLenght);
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
