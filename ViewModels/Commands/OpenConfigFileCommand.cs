using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using P_SCAAT.ViewModels;

namespace P_SCAAT.ViewModels.Commands
{
    internal class OpenConfigFileCommand : AsyncIsExcecutingCoreCommand
    {
        private readonly OscilloscopeConfigViewModel _oscilloscopeConfigViewModel;
        public OpenConfigFileCommand(OscilloscopeConfigViewModel oscilloscopeConfigViewModel)
        {
            _oscilloscopeConfigViewModel = oscilloscopeConfigViewModel;
        }

        ////MAYBE ASYNC COMMAND?
        //public override void Execute(object parameter)
        //{
        //    string fileContent;
        //    OpenFileDialog openFileDialog = new();

        //    openFileDialog.InitialDirectory = Path.GetFullPath("..\\..\\..\\OscilloscopeConfigFiles");
        //    openFileDialog.Filter = "Text (*.txt)|*.txt|All files (*.*)|*.*";

        //    if ((bool)openFileDialog.ShowDialog())
        //    {
        //        Stream fileStream = openFileDialog.OpenFile();
        //        using (StreamReader streamReader = new(fileStream))
        //        {
        //            fileContent = streamReader.ReadToEnd();
        //        }
        //        List<string> loadedConfigString = Regex.Split(fileContent, @"\r|\n|\r\n").Where(line => line != string.Empty).ToList();
        //        _oscilloscopeConfigViewModel.TempOscilloscopeConfigString = loadedConfigString;
        //    }
        //}

        public override async Task ExecuteAsync(object parameter)
        {
            //Debug.WriteLine("OPEN 1 " + Thread.CurrentThread.ManagedThreadId);
            string fileContent;
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Path.GetFullPath(@"..\..\OscilloscopeConfigFiles"),
                Filter = "Text (*.txt)|*.txt|All files (*.*)|*.*"
            };
            if ((bool)openFileDialog.ShowDialog())
            {
                //await Task.Run(async () =>
                //{
                    //Debug.WriteLine("OPEN 2 " + Thread.CurrentThread.ManagedThreadId);

                    Stream fileStream = openFileDialog.OpenFile();
                    using (StreamReader streamReader = new StreamReader(fileStream))
                    {
                        //Thread.Sleep(5000);
                        fileContent = await streamReader.ReadToEndAsync();
                    }
                    //List<string> loadedConfigString = Regex.Split(fileContent, @"\r|\n|\r\n").Where(line => line != string.Empty).ToList();
                    List<string> loadedConfigString = Regex.Split(fileContent, Environment.NewLine).Where(line => line != string.Empty).ToList();
                    _oscilloscopeConfigViewModel.TempOscilloscopeConfigString.AddRange(loadedConfigString);
                //});
                List<string> resultList = _oscilloscopeConfigViewModel.TempOscilloscopeConfigString.Distinct().ToList();
                _oscilloscopeConfigViewModel.TempOscilloscopeConfigString = resultList;
            }
        }
    }
}
