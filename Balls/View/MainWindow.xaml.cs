using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Logic;
using Data;

namespace View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
          
            LogicClass logic = new LogicClass();
            var balls = logic.CreateBalls();

            StringBuilder sb = new StringBuilder();

            foreach (var ball in balls)
            {
                sb.AppendLine($"Ball: X={ball.X}, Y={ball.Y}");
            }

            string result = logic.SayHi("Jan Kowalski");

            HelloTextBlock.Text = sb.ToString();


        }
    }
}