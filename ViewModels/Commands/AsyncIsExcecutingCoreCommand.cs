using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P_SCAAT.ViewModels.Commands
{
    internal abstract class AsyncIsExcecutingCoreCommand : AsyncCoreCommand
    {
        private bool _isExecuting;
        protected bool IsExecuting
        {
            get => _isExecuting;
            set { _isExecuting = value; OnCanExecuteChanged(); }
        }
        public override bool CanExecute(object parameter)
        {
            return !IsExecuting && base.CanExecute(parameter);
        }
        public override async void Execute(object parameter)
        {
            IsExecuting = true;

            try
            {
                await ExecuteAsync(parameter);
            }
            finally
            {
                IsExecuting = false;
            }
        }
    }
}
