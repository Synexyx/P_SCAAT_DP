using P_SCAAT.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace P_SCAAT.ViewModels
{
    internal abstract class SessionDeviceVM : CorePropChangedVM, IErrorMessage
    {
        public abstract ISessionDevice SessionDevice { get; }
        public bool IsSessionOpen => SessionDevice.IsSessionOpen;
        public bool IsSessionClosed => !SessionDevice.IsSessionOpen;
        public abstract bool ChangingSession { get; set; }
        public abstract string SelectedAvailableResource { get; }
        public ObservableCollection<Exception> ErrorMessages { get; } = new ObservableCollection<Exception>();

        public SessionDeviceVM()
        {
            ErrorMessages.CollectionChanged += ErrorMessage_Changed;
        }

        public virtual void ErrorMessage_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            //_ = Task.Run(() =>
            //  {
            if (e.NewItems != null)
            {
                //List<string> errorMessages = new List<string>();
                foreach (Exception item in e.NewItems)
                {
                    //_ = MessageBox.Show(item.Message);
                    _ = MessageBox.Show($"{item.Message}{Environment.NewLine}{item.StackTrace}", $"{item.GetType()}", MessageBoxButton.OK, MessageBoxImage.Error);

                    //              errorMessages.Add($"{DateTime.Now} {item.Message}{Environment.NewLine}");
                }
                //          File.AppendAllLines("../../sessionLog.log", errorMessages);
            }
            //});
        }

        public override void Dispose()
        {
            ErrorMessages.CollectionChanged -= ErrorMessage_Changed;
            base.Dispose();
        }
    }
}
