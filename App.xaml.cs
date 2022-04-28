using System.Windows;
using P_SCAAT.Models;
using P_SCAAT.ViewModels;
using P_SCAAT.ViewModels.ViewControlState;

namespace P_SCAAT
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly Oscilloscope _oscilloscope;
        private readonly CryptoDeviceMessage _cryptoDeviceMessage;

        private readonly OscilloscopeViewControlState _oscilloscopeViewControlState;



        public App()
        {
            _oscilloscope = new Oscilloscope();
            _cryptoDeviceMessage = new CryptoDeviceMessage();
            _oscilloscopeViewControlState = new OscilloscopeViewControlState();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            //_osciloscopeControlState.OsciloscopeSelectedVM = new OsciloscopeViewModel(_osciloscope, _osciloscopeControlState);
            _oscilloscopeViewControlState.OscilloscopeSelectedVM = CreateOscilloscopeVM();
            MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(_oscilloscopeViewControlState, CreateSerialPortRS232VM())
            };
            MainWindow.Show();
            base.OnStartup(e);
        }

        private OscilloscopeViewModel CreateOscilloscopeVM()
        {
            return new OscilloscopeViewModel(_cryptoDeviceMessage, _oscilloscope, _oscilloscopeViewControlState, CreateOscilloscopeConfigVM);
        }

        private OscilloscopeConfigViewModel CreateOscilloscopeConfigVM()
        {
            return new OscilloscopeConfigViewModel(_oscilloscope, _oscilloscopeViewControlState, CreateOscilloscopeVM);
        }

        private SerialPortRS232ViewModel CreateSerialPortRS232VM()
        {
            return new SerialPortRS232ViewModel(_cryptoDeviceMessage);
        }
    }
}
