using System.Threading.Tasks;

namespace P_SCAAT.ViewModels.Commands
{
    internal abstract class AsyncCoreCommand : CoreCommand
    {

        public override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter);
        }

        public override async void Execute(object parameter)
        {
            await ExecuteAsync(parameter);
        }

        public abstract Task ExecuteAsync(object parameter);
    }
}
