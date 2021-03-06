using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using P_SCAAT.ViewModels;

namespace P_SCAAT.ViewModels.Commands
{
    /// <summary>
    /// Save current configuration string into desired file
    /// </summary>
    internal class SaveConfigFileCommand : AsyncIsExcecutingCoreCommand
    {
        private readonly OscilloscopeConfigViewModel _oscilloscopeConfigViewModel;
        public SaveConfigFileCommand(OscilloscopeConfigViewModel oscilloscopeConfigViewModel)
        {
            _oscilloscopeConfigViewModel = oscilloscopeConfigViewModel;
        }
        public override async Task ExecuteAsync(object parameter)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                InitialDirectory = Path.GetFullPath(@"..\..\OscilloscopeConfigFiles"),
                Filter = "Text (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if ((bool)saveFileDialog.ShowDialog())
            {
                await Task.Run(() =>
                {
                    File.WriteAllLines(saveFileDialog.FileName, _oscilloscopeConfigViewModel.TempOscilloscopeConfigString);
                });
            }
        }
    }
}
