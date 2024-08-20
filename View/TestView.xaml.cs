

using QuizGame.ViewModel;
using Windows.UI.Xaml.Controls;

namespace QuizGame
{
    public sealed partial class TestView : Page
    {
        public TestView()
        {
            this.InitializeComponent();
#if LOCALTESTMODEON
            this.ClientView.ViewModel = ViewModelLocator.ClientViewModel;
            this.ClientView2.ViewModel = ViewModelLocator.ClientViewModel2;
#endif
        }
    }
}