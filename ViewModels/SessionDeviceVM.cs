using P_SCAAT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P_SCAAT.ViewModels
{
    internal abstract class SessionDeviceVM : CorePropChangedVM
    {
        public abstract ISessionDevice SessionDevice { get; }
        public bool IsSessionOpen => SessionDevice.IsSessionOpen;
        public bool IsSessionClosed => !SessionDevice.IsSessionOpen;
        public abstract bool ChangingSession { get; set; }
        public abstract string SelectedAvailableResource { get; }
    }
}
