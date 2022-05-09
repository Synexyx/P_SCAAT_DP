using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace P_SCAAT.ViewModels
{
    /// <summary>
    /// Abstract view model implementing <see cref="OnPropertyChanged(string)"/> to dynamically update <see cref="Views"/>.
    /// Everything in <see cref="Views"/> that has binding to specified property will automatically update if <see cref="PropertyChanged"/> event is invoked.
    /// </summary>
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
