using System.Windows;
using System.Windows.Controls.Primitives;
using Crabtopus.ViewModels;

namespace Crabtopus.Views
{
    internal partial class Overlay : Window
    {
        public Overlay(OverlayViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
        }

        private void ToggleContentPopup(object sender, RoutedEventArgs e)
        {
            ContentPopup.SetCurrentValue(Popup.IsOpenProperty, !ContentPopup.IsOpen);
            if (ContentPopup.IsOpen)
            {
                ContentPopup.Focus();
            }
        }
    }
}
