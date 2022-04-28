using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace P_SCAAT.ViewModels
{
    internal abstract class CorePropChangedVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual void Dispose() { }
    }
}
