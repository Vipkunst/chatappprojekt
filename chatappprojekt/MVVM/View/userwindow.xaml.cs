using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChatClient.MVVM.View.Net
{
    /// <summary>
    /// Interaktionslogik für userwindow.xaml
    /// </summary>
    public partial class userwindow : UserControl
    {
        public static userwindow Instance { get; private set; }

        public userwindow()
        {
            InitializeComponent();

            Instance = this;
        }

        public StackPanel GetStackPanel()
        {
            return ChatStackPanel;
        }

    }
}
