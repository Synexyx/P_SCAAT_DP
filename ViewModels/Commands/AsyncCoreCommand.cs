using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P_SCAAT.ViewModels.Commands
{
    internal abstract class AsyncCoreCommand : CoreCommand
    {
        //private bool _isExecuting;
        //protected bool IsExecuting
        //{
        //    get => _isExecuting;
        //    set { _isExecuting = value; OnCanExecuteChanged(); }
        //}

        public override bool CanExecute(object parameter)
        {
            //return !IsExecuting && base.CanExecute(parameter);
            return base.CanExecute(parameter);
        }

        public override async void Execute(object parameter)
        {
            await ExecuteAsync(parameter);

            //IsExecuting = true;

            //try
            //{
            //    await ExecuteAsync(parameter);
            //}
            //finally
            //{
            //    IsExecuting = false;
            //}
        }

        public abstract Task ExecuteAsync(object parameter);
    }
}
