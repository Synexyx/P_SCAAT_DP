using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P_SCAAT.Exceptions
{
    [Serializable]
    internal class SessionCommunicationException : Exception
    {
        public SessionCommunicationException(string message) : base(message) { }

    }
}
