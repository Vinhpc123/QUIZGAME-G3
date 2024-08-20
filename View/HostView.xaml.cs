
using QuizGame.ViewModel;
using Windows.UI.Xaml.Controls;

namespace QuizGame.View
{
    public sealed partial class HostView : UserControl
    {
        public HostView()
        {
            this.InitializeComponent();
        }

        public HostViewModel ViewModel { get; } = ViewModelLocator.HostViewModel;
    }
}
