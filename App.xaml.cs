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

        /// <summary>
        /// Creates permanent instances (singletons) of <see cref="Oscilloscope"/> and <see cref="CryptoDeviceMessage"/>. This instances are initialy empty, but can be changed during runtime of application.<br/>
        /// This will allow for application to pass this instances between different classes as singletons.
        /// </summary>
        public App()
        {
            _oscilloscope = new Oscilloscope();
            _cryptoDeviceMessage = new CryptoDeviceMessage();
            _oscilloscopeViewControlState = new OscilloscopeViewControlState();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _oscilloscopeViewControlState.OscilloscopeSelectedVM = CreateOscilloscopeVM();
            MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(_oscilloscopeViewControlState, CreateSerialPortRS232VM())
            };
            MainWindow.Show();
            base.OnStartup(e);
        }

        /// <summary>
        /// <see cref="System.Func{TResult}"/> called when changing views. Will create new <see cref="OscilloscopeViewModel"/> and fill it with permanent data.
        /// </summary>
        private OscilloscopeViewModel CreateOscilloscopeVM()
        {
            return new OscilloscopeViewModel(_cryptoDeviceMessage, _oscilloscope, _oscilloscopeViewControlState, CreateOscilloscopeConfigVM);
        }

        /// <summary>
        /// <see cref="System.Func{TResult}"/> called when changing views. Will create new <see cref="OscilloscopeConfigViewModel"/> and fill it with permanent data.
        /// </summary>
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
