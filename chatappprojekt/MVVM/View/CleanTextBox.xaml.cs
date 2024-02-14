using System.Windows;
using System.Windows.Controls;

namespace ChatClient.MVVM.View.usercontroll
{
    public partial class CleanTextBox : UserControl
    {
        private string placeholder;

        public string Placeholder { get; set; }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(CleanTextBox), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public CleanTextBox()
        {
            InitializeComponent();
        }

        private void clearclick(object sender, RoutedEventArgs e)
        {
            txtinput.Clear();
            txtinput.Focus();
            Text = string.Empty;
        }

        private void txtinput_TextChanged(object sender, TextChangedEventArgs e)
        {
            Text = txtinput.Text;

            if (string.IsNullOrEmpty(txtinput.Text))
            {
                tbPlaceholder.Visibility = Visibility.Visible;
            }
            else
            {
                tbPlaceholder.Visibility = Visibility.Hidden;
            }
        }
    }
}
