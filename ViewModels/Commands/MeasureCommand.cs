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
    /// <summary>
    /// Excecutes measure loop. Within it is creation of new message, getting waveform data from oscilloscope and saving results into file. Loop can be canceled with second button click, while previous loop is active.
    /// </summary>
    internal class MeasureCommand : AsyncCoreCommand
    {
        private readonly OscilloscopeViewModel _oscilloscopeViewModel;
        private readonly Oscilloscope _oscilloscope;
        private readonly CryptoDeviceMessage _cryptoDeviceMessage;

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
            var watch = new Stopwatch();
            watch.Start();
            Debug.WriteLine($"Begin time measure");

            if (!_oscilloscopeViewModel.MeasurementInProgress)
            {
                _oscilloscopeViewModel.MeasurementInProgress = true;
                _oscilloscopeViewModel.ProgressBarValue = 0;

                tokenSource = new CancellationTokenSource();

                string fileNameSessionID = DateTime.Now.ToString("HHmmss");
                _cryptoDeviceMessage.InitializeRNGMessageGenerator(_oscilloscopeViewModel.MessageLenght);

                await Task.Run(async () =>
                {
                    try
                    {
                        //if (tokenSource.Token.IsCancellationRequested)
                        //{
                        //    Debug.WriteLine("CANCELED");
                        tokenSource.Token.ThrowIfCancellationRequested();
                        //}
                        int perFile = 0;
                        int fileNumber = 0;
                        List<WaveformSourceViewModel> sourcesToMeasure = _oscilloscopeViewModel.WaveformSource.Where(x => x.IsSelected).ToList();
                        string response = string.Empty;

                        for (int totalTraces = 0; totalTraces < _oscilloscopeViewModel.TracesTotal; totalTraces++)
                        {

                            watch.Stop();
                            Debug.WriteLine($"New Task created {watch.ElapsedMilliseconds}");
                            watch.Reset();
                            watch.Start();

                            _oscilloscope.MeasurePrep();

                            watch.Stop();
                            Debug.WriteLine($"Measure prep {watch.ElapsedMilliseconds}");
                            watch.Reset();
                            watch.Start();

                            await Task.Run(() => { _cryptoDeviceMessage.GenerateNewMessage(); });

                            watch.Stop();
                            Debug.WriteLine($"Message created and sent {watch.ElapsedMilliseconds}");
                            watch.Reset();
                            watch.Start();

                            //if (tokenSource.Token.IsCancellationRequested)
                            //{
                            //    Debug.WriteLine("CANCELED");
                            tokenSource.Token.ThrowIfCancellationRequested();
                            //}
                            string selectedSource = string.Empty;
                            if (!sourcesToMeasure.Any())
                            {
                                response = Convert.ToBase64String(_oscilloscope.GetWaveformData());
                            }
                            else
                            {
                                foreach (WaveformSourceViewModel source in sourcesToMeasure)
                                {
                                    selectedSource = source.SourceName;
                                    _oscilloscope.ChangeWaveformSource(selectedSource);
                                    response = Convert.ToBase64String(_oscilloscope.GetWaveformData());
                                }
                            }
                            if (_oscilloscopeViewModel.TracesPerFile == perFile && _oscilloscopeViewModel.TracesPerFile != 0)
                            {
                                fileNumber++;
                                perFile = 0;
                            }

                            watch.Stop();
                            Debug.WriteLine($"Waveform data acquired {watch.ElapsedMilliseconds}");
                            watch.Reset();
                            watch.Start();

                            await SaveToFile(fileNameSessionID, fileNumber, selectedSource, response);

                            watch.Stop();
                            Debug.WriteLine($"Saved to file {watch.ElapsedMilliseconds}");

                            perFile++;
                            _oscilloscopeViewModel.ProgressBarValue = totalTraces + 1;
                            //if (tokenSource.Token.IsCancellationRequested)
                            //{
                            //    Debug.WriteLine("CANCELED");
                            tokenSource.Token.ThrowIfCancellationRequested();
                            //}
                        }
                    }
                    catch (OperationCanceledException ex) when (ex.CancellationToken == tokenSource.Token)
                    {
                        Debug.WriteLine("TASK CANCELED");
                        _oscilloscopeViewModel.ProgressBarValue = 0;
                    }
                    catch (Exception ex)
                    {
                        _oscilloscopeViewModel.ErrorMessages.Add(ex);
                        tokenSource.Cancel();
                    }
                }, tokenSource.Token);
            }
            else
            {
                tokenSource.Cancel();
            }
            _oscilloscopeViewModel.MeasurementInProgress = false;
        }

        private async Task SaveToFile(string fileNameSessionID, int fileNumber, string sourceName, string response)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string messageBytesToBase64 = Convert.ToBase64String(_cryptoDeviceMessage.MessageBytes);
            _ = stringBuilder.Append($"{_cryptoDeviceMessage.TimeCreated:T},{sourceName},{messageBytesToBase64},{response}{Environment.NewLine}");
            string directoryToSave = Path.GetFullPath(@"..\..\Measurment");
            await Task.Run(() =>
            {
                File.AppendAllText($"{directoryToSave}\\{DateTime.Now:yyyyMMdd}-{fileNameSessionID}-measurment-{fileNumber.ToString($"00")}.txt", stringBuilder.ToString());
            });
            _ = stringBuilder.Clear();
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
