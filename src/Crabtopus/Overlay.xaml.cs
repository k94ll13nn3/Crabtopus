using System.Windows;
using Crabtopus.ViewModels;

namespace Crabtopus
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
