using System.Windows;
using System.Windows.Input;

namespace Crabtopus.ViewModels
{
    internal class NotifyIconViewModel : ViewModelBase
    {
        public ICommand ExitApplicationCommand => new DelegateCommand<object>(_ => Application.Current.Shutdown());
    }
}
