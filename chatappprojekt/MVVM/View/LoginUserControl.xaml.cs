using System;
using System.Windows;
using System.Windows.Controls;
using ChatClient.MVVM.Core;
using System.Windows.Input;
using MySql.Data.MySqlClient;

namespace ChatClient.MVVM.View.Net
{
    public partial class LoginUserControl : UserControl
    {
        private MySqlConnection connection;

        public LoginUserControl()
        {
            InitializeComponent();
        }                   
        public ICommand ToRegisterUserControlCommand => new RelayCommand((object parameter) =>
        {
            if (parameter is string enteredUsername)
            {
            }
        });


    }
}