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
    internal class SaveConfigFileCommand : AsyncCoreCommand
    {
        private readonly OscilloscopeConfigViewModel _oscilloscopeConfigViewModel;
        public SaveConfigFileCommand(OscilloscopeConfigViewModel oscilloscopeConfigViewModel)
        {
            _oscilloscopeConfigViewModel = oscilloscopeConfigViewModel;
        }
        public override async Task ExecuteAsync(object parameter)
        {
            //Debug.WriteLine("SAVE 1 " + Thread.CurrentThread.ManagedThreadId);
            SaveFileDialog saveFileDialog = new SaveFileDialog ();
            saveFileDialog.InitialDirectory = Path.GetFullPath("..\\..\\..\\OscilloscopeConfigFiles");
            saveFileDialog.Filter = "Text (*.txt)|*.txt|All files (*.*)|*.*";

            if ((bool)saveFileDialog.ShowDialog())
            {
                await Task.Run(() =>
                {
                    //Debug.WriteLine("SAVE 2 " + Thread.CurrentThread.ManagedThreadId);
                    //Thread.Sleep(5000);
                    File.WriteAllLines(saveFileDialog.FileName, _oscilloscopeConfigViewModel.TempOscilloscopeConfigString);
                });
            }
        }
    }
}
