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

        public override async Task ExecuteAsync(object parameter)
        {
            string fileContent;
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Path.GetFullPath(@"..\..\OscilloscopeConfigFiles"),
                Filter = "Text (*.txt)|*.txt|All files (*.*)|*.*"
            };
            if ((bool)openFileDialog.ShowDialog())
            {
                Stream fileStream = openFileDialog.OpenFile();
                using (StreamReader streamReader = new StreamReader(fileStream))
                {

                    fileContent = await streamReader.ReadToEndAsync();
                }
                List<string> loadedConfigString = Regex.Split(fileContent, Environment.NewLine).Where(line => line != string.Empty).ToList();
                _oscilloscopeConfigViewModel.TempOscilloscopeConfigString.AddRange(loadedConfigString);
                List<string> resultList = _oscilloscopeConfigViewModel.TempOscilloscopeConfigString.Distinct().ToList();
                _oscilloscopeConfigViewModel.TempOscilloscopeConfigString = resultList;
            }
        }
    }
}
