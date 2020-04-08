using System.Windows;
using System.Windows.Input;

namespace Crabtopus.ViewModels
{
    internal class NotifyIconViewModel : ViewModelBase
    {
        public static ICommand ExitApplicationCommand => new DelegateCommand(() => Application.Current.Shutdown());
    }
}
