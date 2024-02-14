using chatappprojekt;
using ChatClient.MVVM.View;
using ChatClient.MVVM.View.Net;
using ChatClient.MVVM.View.usercontroll;
using ChatClient.Net.IO;
using MySql.Data.MySqlClient;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ChatClient.Net
{
    internal class Server
    {
        TcpClient _client;
        public PacketReader PacketReader;

        public event Action connectedEvent;
        public event Action msgReceivedEvent;
        public event Action BenutzerDisconectEvent;
        public event Action BenutzerEingelogtEvent;
        public event Action BenutzerbefreundetEvent;

        // Declare friendsListView at the class level
        ListView friendsToAddListView;

        private MySqlConnection connection;
        bool connectedToDB = false;

        public Server()
        {
            _client = new TcpClient();
        }

        public void ConnectToServer(string BenutzerName)
        {
            if (!_client.Connected)
            {
                _client.Connect("192.168.178.22", 54888);
                PacketReader = new PacketReader(_client.GetStream());

                if (!string.IsNullOrEmpty(BenutzerName))
                {
                    var connectPacket = new PacketBuilder();
                    connectPacket.WriteOpCode(0);
                    connectPacket.WriteMessage(BenutzerName);
                    _client.Client.Send(connectPacket.GetPacketByte());
                }
                ReadPacket();
            }
        }

        private void ReadPacket()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    var opcode = PacketReader.ReadByte();
                    switch (opcode)
                    {
                        case 1:
                            connectedEvent?.Invoke();
                            break;
                        case 5:
                            msgReceivedEvent?.Invoke();
                            break;
                        case 69:
                            BenutzerDisconectEvent?.Invoke();
                            break;
                        default:
                            Console.WriteLine("Ach Ja...");
                            break;
                    }
                }
            });
        }

        public void SendMessageToServer(string message, string recipient, string sender, string selectedChat)
        {
            if (_client.Connected && selectedChat != null)
            {
                var messagePacket = new PacketBuilder();
                if (recipient == "Public")
                {
                    messagePacket.WriteOpCode(5);
                }
                else
                {
                    messagePacket.WriteOpCode(6);
                }
                messagePacket.WriteSender(sender);
                messagePacket.WriteRecipient(recipient);
                messagePacket.WriteMessage(message);
                _client.Client.Send(messagePacket.GetPacketByte());
            }
            else
            {
                MessageBox.Show("Error: Socket not connected. Unable to send message.");
            }
        }

        //----------------------login------------------------

        // IP of the Database host
        const string DataSource = "192.168.178.22";

        // The name of the database
        const string InitialCatalog = "messengerdb";

        // Should the database require a specific log in
        const string DBUserID = "USERS";
        const string DBPassword = "12345";

        // Create the connection string for MySQL
        const string connectionString = "Server=" +
            DataSource +
            ";Database=" +
            InitialCatalog +
            ";User ID=" +
            DBUserID +
            ";Password=" +
            DBPassword + ";";

        public string loggedInUsername = "";

        public void Login(string benutzername, string passwort)
        {
            try
            {
                // Initialize the MySqlConnection object for MySQL
                connection = new MySqlConnection(connectionString);
                // Open the connection
                connection.Open();
                connectedToDB = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }

            if (connectedToDB)
            {
                string loggedInPassword;

                string query = $"SELECT * FROM users WHERE username = '{benutzername}'";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    // Execute the query
                    command.ExecuteNonQuery();

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        // Check if there are any rows returned
                        if (reader.Read())
                        {
                            // Get the username from the "username" column
                            loggedInUsername = reader["username"].ToString();
                            loggedInPassword = reader["password"].ToString();

                            if (passwort == loggedInPassword)
                            {
                                Load_userwindow();
                                ConnectToServer(benutzername);
                                BenutzerEingelogtEvent?.Invoke();
                            }
                        }
                    }
                }
            }
            connection.Close();
        }

        public void Register(string benutzername, string passwort, string confirmPasswort)
        {
            try
            {
                // Initialize the MySqlConnection object for MySQL
                connection = new MySqlConnection(connectionString);
                // Open the connection
                connection.Open();
                connectedToDB = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }

            if (connectedToDB)
            {
                string loggedInUsername = "";
                string loggedInPassword;

                string query = $"SELECT * FROM users WHERE username = '{benutzername}'";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    // Execute the query
                    command.ExecuteNonQuery();

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        // Check if there are any rows returned
                        if (reader.Read())
                        {
                            // Get the username from the "username" column
                            loggedInUsername = reader["username"].ToString();
                        }
                    }
                }

                if (loggedInUsername != benutzername)
                {
                    if (passwort == confirmPasswort)
                    {
                        string Insertquery = "INSERT INTO users(username, email, password)" +
                            "VALUES(@username, @email, @password)";
                        using (MySqlCommand command = new MySqlCommand(Insertquery, connection))
                        {
                            command.Parameters.AddWithValue("@username", benutzername);
                            command.Parameters.AddWithValue("@email", "testEmail");
                            command.Parameters.AddWithValue("@password", passwort);
                            command.ExecuteNonQuery();

                            MessageBox.Show("Sucess");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Confirm Passwort");
                    }
                }
                else
                {
                    MessageBox.Show("Username '" + benutzername + "' Already exists");
                }
            }
            connection.Close();
        }

        public void ToRegisterUserControl()
        {
            Load_RegisterUserControl();
        }

        public void Load_userwindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                userwindow userwindow = new userwindow();

                // set UserControl position
                Grid.SetRow(userwindow, 0);

                // get MainWindow Grid
                MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
                Grid mainGrid = mainWindow.MainGrid;

                // Delete existing Elements in Grid
                RemoveAllElementsInRow(0, mainGrid);
                mainGrid.Children.Add(userwindow);
            });
        }

        private void Load_RegisterUserControl()
        {
            RegisterUserControl RegisterUserControl = new RegisterUserControl();

            // set UserControl position
            Grid.SetRow(RegisterUserControl, 0);

            // get MainWindow Grid
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            Grid mainGrid = mainWindow.MainGrid;

            // Delete existing Elements in Grid
            RemoveAllElementsInRow(0, mainGrid);
            mainGrid.Children.Add(RegisterUserControl);
        }

        public void Load_Login()
        {
            LoginUserControl LoginUserControl = new LoginUserControl();

            // set UserControl position
            Grid.SetRow(LoginUserControl, 0);

            // get MainWindow Grid
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            Grid mainGrid = mainWindow.MainGrid;

            // Delete existing Elements in Grid
            RemoveAllElementsInRow(0, mainGrid);
            mainGrid.Children.Add(LoginUserControl);
        }

        private void RemoveAllElementsInRow(int rowIndex, Grid yourGrid)
        {
            List<UIElement> elementsToRemove = new List<UIElement>();

            foreach (UIElement child in yourGrid.Children)
            {
                int row = Grid.GetRow(child);

                if (row == rowIndex)
                {
                    elementsToRemove.Add(child);
                }
            }

            foreach (UIElement elementToRemove in elementsToRemove)
            {
                yourGrid.Children.Remove(elementToRemove);
            }
        }

        List<string> userList = new List<string>();
        string username = "";

        public void AddFriends(string benutzername)
        {
            userList.Clear();

            username = benutzername;

            userwindow Userwindow = userwindow.Instance;
            StackPanel stackPanel = Userwindow.GetStackPanel();
            stackPanel.Children.Clear();

            CleanTextBox searchFriendTextbox = new CleanTextBox();
            searchFriendTextbox.Placeholder = "Search friend...";
            searchFriendTextbox.Margin = new Thickness(10);
            searchFriendTextbox.txtinput.TextChanged += SearchFriendTextbox_TextChanged;
            searchFriendTextbox.txtinput.Background = (Brush)new BrushConverter().ConvertFrom("#2A2726");
            searchFriendTextbox.txtinput.Foreground = Brushes.White;
            searchFriendTextbox.btnClear.Background = (Brush)new BrushConverter().ConvertFrom("#2A2726");
            stackPanel.Children.Add(searchFriendTextbox);

            Button searchFriendButton = new Button();
            searchFriendButton.Content = "Add";
            searchFriendButton.Click += AddFriendButton_Click;
            searchFriendButton.Background = (Brush)new BrushConverter().ConvertFrom("#2A2726");
            searchFriendButton.Foreground = Brushes.White;
            stackPanel.Children.Add(searchFriendButton);

            try
            {
                // Initialize the MySqlConnection object for MySQL
                connection = new MySqlConnection(connectionString);
                // Open the connection
                connection.Open();
                connectedToDB = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }

            if (connectedToDB)
            {
                string query = "SELECT username FROM users ORDER BY username";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string friendUsername = reader["username"].ToString();
                            userList.Add(friendUsername);
                        }
                    }
                }
            }
            connection.Close();

            friendsToAddListView = new ListView();
            friendsToAddListView.ItemsSource = userList;
            friendsToAddListView.Background = (Brush)new BrushConverter().ConvertFrom("#2A2726");
            friendsToAddListView.Foreground = Brushes.White;
            stackPanel.Children.Add(friendsToAddListView);
        }



        private void SearchFriendTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            userwindow Userwindow = userwindow.Instance;
            StackPanel stackPanel = Userwindow.GetStackPanel();

            TextBox searchFriendTextbox = (TextBox)sender;
            string searchBarText = searchFriendTextbox.Text;

            var listViewToRemove = stackPanel.Children.OfType<ListView>().FirstOrDefault();
            if (listViewToRemove != null)
            {
                // Remove the ListView from the StackPanel
                stackPanel.Children.Remove(listViewToRemove);
            }

            friendsToAddListView = new ListView();

            // Set the item source and template for the ListView
            List<string> filteredList = userList
                .Where(s => s.StartsWith(searchBarText.ToString(), StringComparison.OrdinalIgnoreCase))
                .ToList();

            friendsToAddListView.ItemsSource = filteredList;
            friendsToAddListView.Background = (Brush)new BrushConverter().ConvertFrom("#2A2726");
            friendsToAddListView.Foreground = Brushes.White;
            stackPanel.Children.Add(friendsToAddListView);
        }

        private void AddFriendButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = friendsToAddListView.SelectedItem;
            string selectedUserStr = (string)selectedUser;

            try
            {
                // Initialize the MySqlConnection object for MySQL
                connection = new MySqlConnection(connectionString);
                // Open the connection
                connection.Open();
                connectedToDB = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }

            if (connectedToDB)
            {
                List<string> FreundeListe = new List<string>();
                string query = $"SELECT friend FROM friendship WHERE user = '{loggedInUsername}'";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string Freundusername = reader["friend"].ToString();
                            FreundeListe.Add(Freundusername);
                        }
                    }
                }

                bool friendAlreadyAdded = false;

                foreach (string freund in FreundeListe)
                {
                    if (selectedUserStr == freund)
                    {
                        friendAlreadyAdded = true;
                    }
                }

                if (selectedUserStr != "" && !friendAlreadyAdded)
                {
                    string Insertquery = "INSERT INTO friendship(user, friend) VALUES(@user, @friend)";
                    using (MySqlCommand command = new MySqlCommand(Insertquery, connection))
                    {
                        command.Parameters.AddWithValue("@user", username);
                        command.Parameters.AddWithValue("@friend", selectedUserStr);
                        command.ExecuteNonQuery();
                    }
                }
            }
            connection.Close();
            BenutzerbefreundetEvent?.Invoke();
        }
    }
}
