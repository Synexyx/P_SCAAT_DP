using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P_SCAAT.ViewModels.Commands
{
    internal class SimpleCommand : CoreCommand
    {
        private Action _action;
        public SimpleCommand(Action action)
        {
            _action = action;
        }
        public override void Execute(object parameter)
        {
            _action?.Invoke();
        }
    }
}
