using System.Windows;
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
    }
}
