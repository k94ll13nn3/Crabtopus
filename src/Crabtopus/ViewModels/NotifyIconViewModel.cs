using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Crabtopus.ViewModels
{
    internal class NotifyIconViewModel : ViewModelBase
    {
        public NotifyIconViewModel()
        {
            ExitApplicationCommand = new DelegateCommand(() => Application.Current.Shutdown());
            Version = $"v{FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(NotifyIconViewModel))?.Location).ProductVersion}";
            About = $@"Crabtopus {Version}
by @k94ll13nn3

Crabtopus is unofficial Fan Content permitted under the 
Fan Content Policy. Not approved/endorsed by Wizards. 
Portions of the materials used are property of Wizards of the Coast. 
©Wizards of the Coast LLC.

Used packages:
- Hardcodet.NotifyIcon.Wpf
- AngleSharp
- Humanizer
- EFCore.BulkExtensions";
        }

        public ICommand ExitApplicationCommand { get; set; }

        public string Version { get; set; }

        public string About { get; set; }
    }
}
