using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P_SCAAT.Models
{
    internal interface ISessionDevice
    {
        void OpenSession(string sessionName);
        void CloseSession();

        bool IsSessionOpen { get; }
    }
}
