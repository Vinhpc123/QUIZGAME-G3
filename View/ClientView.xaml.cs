
using QuizGame.ViewModel;
using Windows.UI.Xaml.Controls;

namespace QuizGame.View
{
    public sealed partial class ClientView : UserControl
    {
        public ClientView()
        {
            this.InitializeComponent();
        }

        public ClientViewModel ViewModel { get; set; }
    }
}
