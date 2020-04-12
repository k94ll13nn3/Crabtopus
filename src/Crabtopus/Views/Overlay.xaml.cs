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

            IsVisibleChanged += Overlay_IsVisibleChanged;
        }

        private void Overlay_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility != Visibility.Visible)
            {
                ContentPopup.SetCurrentValue(Popup.IsOpenProperty, false);
            }
        }
    }
}
