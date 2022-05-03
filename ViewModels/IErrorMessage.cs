using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P_SCAAT.ViewModels
{
    internal interface IErrorMessage : IDisposable
    {
        ObservableCollection<Exception> ErrorMessages { get; }
        void ErrorMessage_Changed(object sender, NotifyCollectionChangedEventArgs e);
    }
}
