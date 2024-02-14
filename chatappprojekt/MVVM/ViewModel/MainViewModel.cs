using ChatClient.MVVM.Core;
using ChatClient.MVVM.Model;
using ChatClient.Net;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ChatClient.MVVM.ViewModel
{
    public class Chat
    {
        public string FriendName { get; set; }
        public ObservableCollection<string> ChatMessages { get; } = new ObservableCollection<string>();

        public Chat(string friendName)
        {
            FriendName = friendName;
        }
    }

    class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<BenutzerModell> Benutzern { get; set; }
        public ObservableCollection<String> Messages { get; set; }
        public ObservableCollection<Chat> Chats { get; set; }
        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }
        public RelayCommand LoginCommand { get; set; }
        public RelayCommand ToRegisterUserControlCommand { get; set; }
        public RelayCommand ReturnToLoginCommand { get; set; }
        public RelayCommand RegisterCommand { get; set; }
        public RelayCommand FriendsButtonCommand { get; set; }
        public RelayCommand PublicChatCommand {  get; set; }
        public RelayCommand DeleteFriendCommand { get; set; }
        public ObservableCollection<string> FreundeListe { get; set; }

        public string BenutzerName { get; set; } = "";
        public string Passwort {  get; set; } = "";
        public string BenutzerNameRegister { get; set; } = "";
        public string PasswortRegister { get; set; } = "";
        public string PasswortConfirmRegister { get; set; } = "";
        public string EmailRegister { get; set; } = "";
        public string EingeloggtAls { get; set; } = "";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _message;

        public string message
        {
            get { return _message; }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged(nameof(message));
                }
            }
        }

        private string _SelectedChat;
        public string SelectedChat
        {
            get { return _SelectedChat; }
            set
            {
                if (_SelectedChat != value)
                {
                    _SelectedChat = value;
                    OnPropertyChanged(nameof(SelectedChat));
                    OnSelectedChatChanged();
                }
            }
        }

        bool userWindow_Loaded = false;

        private void OnSelectedChatChanged()
        {
            if (userWindow_Loaded)
            {
                _server.Load_userwindow();
                userWindow_Loaded = true;
            }
            if (Chats != null)
            {
                ObservableCollection<string> updatedMessages = new ObservableCollection<string>();
            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Clear();
            });
                foreach (Chat chat in Chats)
                {
                    if (chat.FriendName == SelectedChat)
                    {
                        foreach (string message in chat.ChatMessages)
                        {
                            updatedMessages.Add(message);
                        }
                    }
                }

                // Instead of creating a new instance, update the existing Messages property
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Clear(); // Clear the existing messages
                    foreach (string message in updatedMessages)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Messages.Add(message);
                        });
                    }
                });
            }
        }

        public static MainViewModel Instance;
        private Server _server;

        public MainViewModel()
        {
            Instance = this;
            Benutzern = new ObservableCollection<BenutzerModell>();
            FreundeListe = new ObservableCollection<string>();
            Messages = new ObservableCollection<String>();
            Chats = new ObservableCollection<Chat>();
            _server = new Server();
            _server.connectedEvent += BenutzerVerbunden;
            _server.msgReceivedEvent += MessageReceived;
            _server.BenutzerDisconectEvent += EntferneBenutzer;
            _server.BenutzerEingelogtEvent += OnBenutzerEingelogt;
            _server.BenutzerbefreundetEvent += OnUserAdded;

            ConnectToServerCommand = new RelayCommand(o => _server.ConnectToServer(BenutzerName), o => !string.IsNullOrEmpty(BenutzerName));
            LoginCommand = new RelayCommand(o => _server.Login(BenutzerName, Passwort));
            ToRegisterUserControlCommand = new RelayCommand(o => _server.ToRegisterUserControl());
            ReturnToLoginCommand = new RelayCommand(o => _server.Load_Login());
            RegisterCommand = new RelayCommand(o => _server.Register(BenutzerNameRegister, PasswortRegister, PasswortConfirmRegister));
            FriendsButtonCommand = new RelayCommand(o => _server.AddFriends(BenutzerName));
            PublicChatCommand = new RelayCommand(o =>
            {
                _server.Load_userwindow();
                OnPublicChatSelected();
            });

            DeleteFriendCommand = new RelayCommand(o =>
            {
                DeleteFriend_Click();
            });

            SendMessageCommand = new RelayCommand(o =>
                {
                    _server.SendMessageToServer(message,SelectedChat,BenutzerName, _SelectedChat);
                    Application.Current.Dispatcher.Invoke(() => message = string.Empty);
                },
                o => !string.IsNullOrEmpty(message));

            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add("Welcome To BWUP");
                Messages.Add(" ");
                Messages.Add("Select a Chat and start chatting");
            });
        }

        private void EntferneBenutzer()
        {
            var uid = _server.PacketReader.ReadMessage();
            var Benutzer = Benutzern.Where(x => x.UID == uid).FirstOrDefault();
            Application.Current.Dispatcher.Invoke(() => Benutzern.Remove(Benutzer));
        }

        private void MessageReceived()
        {
            var sender = _server.PacketReader.ReadSender();
            var recipient = _server.PacketReader.ReadRecipient();
            var msg = _server.PacketReader.ReadMessage();

            foreach (Chat chat in Chats)
            {
                if (chat.FriendName ==  recipient || chat.FriendName == sender)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        chat.ChatMessages.Add(msg);
                        // Scroll to the last item in the ListBox
                        ScrollToBottom();
                    });
                }
            }
            OnSelectedChatChanged();
        }

        private void ScrollToBottom()
        {
            if (Messages.Count > 0)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Find the UserControl
                    UserControl userControl = Application.Current.MainWindow.FindName("userwindow") as UserControl;

                    // Find the ListView within the UserControl
                    ListView messagesListView = userControl?.FindName("MessagesListBox") as ListView;

                    // Programmatically select the last item
                    messagesListView?.ScrollIntoView(Messages[Messages.Count - 1]);
                });
            }
        }

        private void BenutzerVerbunden()
        {
            var Benutzer = new BenutzerModell
            {
                BenutzerName = _server.PacketReader.ReadMessage(),
                UID = _server.PacketReader.ReadMessage(),
            };

            if (!Benutzern.Any(x => x.UID == Benutzer.UID))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Benutzern.Add(Benutzer);

                    // Display a system message when a new user connects
                    foreach (Chat chat in Chats)
                    {
                        if (chat.FriendName == "Public")
                        {
                            chat.ChatMessages.Add($"{Benutzer.BenutzerName} connected!");
                        }
                    }
                });
            }
        }

        private MySqlConnection connection;

        const string DataSource = "192.168.178.22";

        // The name of the database
        const string InitialCatalog = "messengerdb";

        // Should the database require a specific log in
        const string DBUserID = "USERS";
        const string DBPassword = "12345";
        const string Port = "3306";
        bool connectedToDB = false;

        string connectionString = "Server=" + DataSource + ";Port=" + Port + ";Database=" + InitialCatalog + ";User ID=" + DBUserID + ";Password=" + DBPassword + ";";

        public void Freundeanzeigen()
        {
            EingeloggtAls = $"eingelogt als: {BenutzerName}";

            FreundeListe.Clear();

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
                string query = $"SELECT * FROM friendship WHERE user = '{BenutzerName}'";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    // Execute the query
                    command.ExecuteNonQuery();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        // Check if there are any rows returned
                        while (reader.Read())
                        {
                            // Get the username from the "username" column
                            string Freund = reader["friend"].ToString();
                            FreundeListe.Add(Freund);
                        }
                    }
                }
            }
            connection.Close();
        }

        public void DeleteFriend_Click()
        {
            string connectionString = "Server=" + DataSource + ";Database=" + InitialCatalog + ";User ID=" + DBUserID + ";Password=" + DBPassword + ";";
            
            // Replace "YourTableName" with your actual table name
            string tableName = "friendship";
            
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    
                    string deleteQuery = $"DELETE FROM {tableName} WHERE user = '{BenutzerName}' AND friend = '{SelectedChat}'";
                    
                    using (MySqlCommand command = new MySqlCommand(deleteQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
            Freundeanzeigen();
        }

        private void LoadChats()
        {
            foreach (string Freund in FreundeListe)
            {
                Chats.Add(new Chat(Freund));
            }
            Chats.Add(new Chat("Public"));
        }

        protected virtual void OnBenutzerEingelogt()
        {
            EingeloggtAls = _server.loggedInUsername;
            Freundeanzeigen();

            LoadChats();
        }
        private void OnPublicChatSelected()
        {
            SelectedChat = "Public";
            userWindow_Loaded = true;
        }
        private void OnUserAdded()
        {
            Freundeanzeigen();
            Chats.Add(new Chat(FreundeListe.Last()));
        }
    }
}