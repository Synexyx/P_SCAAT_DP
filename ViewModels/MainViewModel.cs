using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using P_SCAAT.Models;
using P_SCAAT.ViewModels.ViewControlState;

namespace P_SCAAT.ViewModels
{
    internal class MainViewModel : CorePropChangedVM
    {
        #region Properties
        private readonly OscilloscopeViewControlState _oscilloscopeViewControlState;
        private readonly SerialPortRS232ViewModel _serialPortRS232ViewModel;
        public CorePropChangedVM OscilloscopeSelectedVM => _oscilloscopeViewControlState.OscilloscopeSelectedVM;
        public CorePropChangedVM SerialPort232 => _serialPortRS232ViewModel;
        #endregion
        //public OsciloscopeViewModel OsciloscopeViewModel { get; set; }
        //public OsciloscopeConfigViewModel OsciloscopeConfigViewModel { get; set; }

        //public MainViewModel(Osciloscope osciloscope, OsciloscopeControlState osciloscopeControlState)
        //{
        //    Osciloscope = osciloscope;
        //    _osciloscopeControlState = osciloscopeControlState;

        //    _osciloscopeControlState.OsciloscopeConfigViewSwitched += OnOsciloscopeConfigViewSwitch;

        //    //OsciloscopeSelectedVM = new OsciloscopeViewModel(Osciloscope);


        //    //OsciloscopeViewModel = new OsciloscopeViewModel(Osciloscope);
        //    //OsciloscopeConfigViewModel = new OsciloscopeConfigViewModel(Osciloscope);
        //}
        public MainViewModel(OscilloscopeViewControlState oscilloscopeViewControlState, SerialPortRS232ViewModel serialPortRS232ViewModel)
        {
            _oscilloscopeViewControlState = oscilloscopeViewControlState;
            _serialPortRS232ViewModel = serialPortRS232ViewModel;

            _oscilloscopeViewControlState.OscilloscopeConfigViewSwitched += OnOscilloscopeConfigViewSwitch;

            //OsciloscopeSelectedVM = new OsciloscopeViewModel(Osciloscope);


            //OsciloscopeViewModel = new OsciloscopeViewModel(Osciloscope);
            //OsciloscopeConfigViewModel = new OsciloscopeConfigViewModel(Osciloscope);
        }
        private void OnOscilloscopeConfigViewSwitch()
        {
            OnPropertyChanged(nameof(OscilloscopeSelectedVM));
        }

        public override void Dispose()
        {
            _oscilloscopeViewControlState.OscilloscopeConfigViewSwitched -= OnOscilloscopeConfigViewSwitch;
        }
    }
}
