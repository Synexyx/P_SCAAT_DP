using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P_SCAAT.Exceptions
{
    [Serializable]
    internal class SessionControlException : Exception
    {
        public SessionControlException(string message, Exception exception) : base(message, exception) { }
    }
}
