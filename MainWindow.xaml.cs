using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace Chatroom
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public UserInfo SelfInfo = new UserInfo();
        private readonly FileMethod FileMethod = new FileMethod();
        private string initmessage = "Placse sign in to chatroom server.";
        private List<ChatList> ChatList = new List<ChatList>();
        SynchronizationContext _syncContext = null;
        private int RDataLen = 5120;
        private byte[] KeepAlive()
        {
            uint dummy = 0;
            byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
            BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)1000).CopyTo(inOptionValues, Marshal.SizeOf(dummy));
            BitConverter.GetBytes((uint)500).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);
            return inOptionValues;
        }
        private Socket Socket_sender;
        private DispatcherTimer GetUser_timer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();
            Tbl_message.Text = initmessage;
            chatlist.ItemsSource = ChatList;
            _syncContext = SynchronizationContext.Current;
            /*
            string[] chatinfo;
            chatinfo = new string[] { "13:26 allen", "Hi tina" };
            Chatroom.Children.Add(CreateChatSet(chatinfo, true));
            chatinfo = new string[] { "13:18 tina", "Who r u?" };
            Chatroom.Children.Add(CreateChatSet(chatinfo, false));
            chatinfo = new string[] { "13:19 allen", "Oh, forget me..." };
            Chatroom.Children.Add(CreateChatSet(chatinfo, true));
            chatinfo = new string[] { "13:22 allen", "We used to be colleagues at 2003. And i remeber you site my left." };
            Chatroom.Children.Add(CreateChatSet(chatinfo, true));
            chatinfo = new string[] { "13:28 tina", "I have not worked. " };
            Chatroom.Children.Add(CreateChatSet(chatinfo, false));
            chatinfo = new string[] { "13:29 tina", "Go way." };
            Chatroom.Children.Add(CreateChatSet(chatinfo, false));
            Chatscroll.ScrollToEnd();
            */

        }
        private StackPanel CreateChatSet(string[] Chatinfo, bool Isleft)
        {
            StackPanel Chatset = new StackPanel();
            Chatset.Children.Add(CreateUser(Chatinfo[0]));
            Chatset.Children.Add(CreateContent(Chatinfo[1], Isleft));
            if (Isleft)
            {
                Chatset.Margin = new Thickness(5, 5, 50, 5);
                Chatset.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else
            {
                Chatset.Margin = new Thickness(50, 5, 5, 5);
                Chatset.HorizontalAlignment = HorizontalAlignment.Right;
            }
            return Chatset;
        }
        private Label CreateUser(string userInfo)
        {
            Label username = new Label();
            username.Content = userInfo;
            username.Margin = new Thickness(0, 0, 0, -5);
            return username;
        }
        private TextBlock CreateContent(string cintent, bool Isleft)
        {
            TextBlock Chatcontent = new TextBlock();
            Chatcontent.Text = cintent;
            Chatcontent.Padding = new Thickness(10, 0, 10, 0);
            Chatcontent.TextWrapping = TextWrapping.Wrap;
            if (!Isleft)
            {
                Chatcontent.Background = new SolidColorBrush(Color.FromRgb(162, 203, 91));
            }
            return Chatcontent;
        }

        private void Icon_hover(object sender, MouseEventArgs e)
        {
            Bt_signin.Foreground = new SolidColorBrush(Color.FromRgb(162, 203, 91));
            Bt_signout.Foreground = new SolidColorBrush(Color.FromRgb(162, 203, 91));
            username.Foreground = new SolidColorBrush(Color.FromRgb(162, 203, 91));
        }

        private void Icon_leave(object sender, MouseEventArgs e)
        {
            Bt_signin.Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102));
            Bt_signout.Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102));
            username.Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102));
        }

        private void open_signin(object sender, MouseButtonEventArgs e)
        {
            markpage.Visibility = Visibility.Visible;
            signin SigninPage = new signin(this);
            dialog.BorderBrush = new SolidColorBrush(Color.FromRgb(171, 173, 179));
            dialog.BorderThickness = new Thickness(1);
            dialog.Content = SigninPage;
            dialog.Visibility = Visibility.Visible;
        }
        private void Signout(object sender, MouseButtonEventArgs e)
        {
            if (SelfInfo.Token == "") return;
            JObject jObject = FileMethod.Signout(SelfInfo.Token);
            if ((bool)jObject["success"])
            {
                username.Content = "";
                SelfInfo.Token = "";
                Bt_signin.Visibility = Visibility.Visible;
                Bt_signout.Visibility = Visibility.Hidden;
                Tbl_message.Text = initmessage;
                chatlist.ItemsSource = null;
                SetMessageInfo(true);
            }
            else
            {
                Tbl_message.Text = (string)jObject["message"];
                SetMessageInfo(false);
            }
        }
        private void SetMessageInfo(bool IsInfo)
        {
            Tbl_message.Foreground = IsInfo ? new SolidColorBrush(Color.FromRgb(0, 0, 0)) : new SolidColorBrush(Color.FromRgb(255, 0, 0));
        }

        private void Intro_Chatroom(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (SelfInfo.Token != "" && SelfInfo.Token != null)
            {
                JObject Intro = FileMethod.IntroChatroom(SelfInfo.Token);
                Tbl_message.Text = (string)Intro["message"];
                SetMessageInfo(true);
                if ((bool)Intro["success"])
                {
                    SetMessageInfo(true);
                    JObject GetList = FileMethod.GetChatList(SelfInfo.Token);
                    Tbl_message.Text = (string)GetList["message"];
                    SetMessageInfo(true);
                    if ((bool)GetList["success"])
                    {
                        chatlist.ItemsSource = null;
                        SetMessageInfo(true);
                        foreach (JToken chatroom in GetList["data"])
                        {
                            if ((int)chatroom["userid"] == SelfInfo.Userid)
                            {
                                SelfInfo.Ipaddress = (string)chatroom["ipaddress"];
                                SelfInfo.Port = (int)chatroom["port"];
                            }
                            else
                            {
                                ChatList chatlist = new ChatList
                                {
                                    Name = (string)chatroom["name"],
                                    Ipaddress = (string)chatroom["ipaddress"],
                                    Port = (int)chatroom["port"],
                                    Getcall = false
                                };
                                ChatList.Add(chatlist);
                            }
                        }
                        chatlist.ItemsSource = ChatList;
                        //chatlist.Items.Refresh();
                        BindAndListen();
                    }
                    else
                    {
                        SetMessageInfo(false);
                    }
                }
                else
                {
                    SetMessageInfo(false);
                }
            }
        }
        private void BindAndListen()
        {
            GetUser_timer.Interval = TimeSpan.FromSeconds(5);
            GetUser_timer.Tick += GetUser_Tick;
            GetUser_timer.Start();
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endport = new IPEndPoint(IPAddress.Parse(SelfInfo.Ipaddress), SelfInfo.Port);
            socket.Bind(endport);
            socket.Listen(10);
            socket.IOControl(IOControlCode.KeepAliveValues, KeepAlive(), null);
            Thread thread = new Thread(Recevice);
            thread.IsBackground = true;
            thread.Start(socket);
        }
        private void Recevice(object obj)
        {
            Socket SalfSocketServer = obj as Socket;
            while (true)
            {
                string remoteEpInfo = string.Empty;
                try
                {
                    Socket txSocket = SalfSocketServer.Accept();
                    if (txSocket.Connected)
                    {
                        remoteEpInfo = txSocket.RemoteEndPoint.ToString();
                        ChatList Finduser = FindUser(remoteEpInfo);
                        if (Finduser == null)
                        {
                            txSocket.Shutdown(SocketShutdown.Receive);
                            txSocket.Dispose();
                            txSocket.Close();
                            continue;
                        }
                        else
                        {
                            ReceseMsgGoing(txSocket, Finduser);
                        }
                    }
                }
                catch (Exception)
                {
                    Tbl_message.Text = "Detection channel failed";
                    SetMessageInfo(false);
                }
            }
        }
        private ChatList FindUser(string remoteEpInfo)
        {
            string[] remoteArr = remoteEpInfo.Split(':');
            ChatList client = ChatList.FirstOrDefault(c => c.Ipaddress == remoteArr[0] && c.Port == int.Parse(remoteArr[1]));
            return client;
        }
        private void ReceseMsgGoing(Socket txSocket, ChatList Finduser)
        {
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        byte[] recesiveByte = new byte[RDataLen];
                        int getlength = txSocket.Receive(recesiveByte);
                        if (getlength <= 0) { break; }
                        string getmsg = Encoding.UTF8.GetString(recesiveByte, 0, getlength);
                        if (getmsg == "") continue;
                        ProcessData(Finduser, getmsg);
                    }
                    catch (Exception)
                    {
                        txSocket.Dispose();
                        txSocket.Close();
                        _syncContext.Post(ConnectSocketFalse, Finduser);
                        break;
                    }
                }
            })
            {
                IsBackground = true
            };
            thread.Start();
        }
        private void ProcessData(ChatList Finduser, string remoteData)
        {
            ChatList ChatUser = (ChatList)chatlist.SelectedItem;
            if (ChatUser.Ipaddress == Finduser.Ipaddress && ChatUser.Port == Finduser.Port)
            {
                string NowTime = DateTime.Now.ToString("HH:mm:ss");
                string[] chatinfo = new string[] { NowTime + " " + ChatUser.Name, remoteData };
                _syncContext.Post(AddChatContent, chatinfo);
            }
            else
            {
                _syncContext.Post(RefreshChatList, Finduser);
            }
        }
        private void ConnectSocketFalse(object Finduser)
        {
            ChatList ChatFail = (ChatList)Finduser;
            Tbl_message.Text = ChatFail.Name + " " + "can't get chat content.";
            SetMessageInfo(false);
        }
        private void AddChatContent(object ChatContent)
        {
            string[] chatinfo = (string[])ChatContent;
            Chatroom.Children.Add(CreateChatSet(chatinfo, true));
        }
        private void RefreshChatList(object Finduser)
        {
            ChatList CallUser = (ChatList)Finduser;
            ChatList client = ChatList.FirstOrDefault(c => c.Ipaddress == CallUser.Ipaddress && c.Port == CallUser.Port);
            client.Getcall = true;
            chatlist.Items.Refresh();
        }
        private void SetChatList(object start)
        {
            chatlist.Items.Refresh();
        }
        private void Connect_user(object sender, SelectionChangedEventArgs e)
        {
            ChatList selectrow = (ChatList)chatlist.SelectedItem;
            IPAddress ipAddress = IPAddress.Parse(selectrow.Ipaddress);
            int port = selectrow.Port;
            Socket_sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                Socket_sender.Connect(ipAddress, port);
                Chatroom.Children.Clear();
                Tbl_message.Text = "Chatting with " + selectrow.Name;
                SetMessageInfo(true);
                Bt_send.IsEnabled = true;
            }
            catch (ObjectDisposedException)
            {
                Tbl_message.Text = "failed to Chat with " + selectrow.Name;
                SetMessageInfo(false);
            }
        }

        private void Send_Content(object sender, RoutedEventArgs e)
        {
            ChatList selectrow = (ChatList)chatlist.SelectedItem;
            var msg = Tb_sendcontent.Text.Trim();
            try
            {
                if (Socket_sender.Connected)
                {
                    Encoding ei = Encoding.GetEncoding(950);
                    int sendMsgLength = Socket_sender.Send(ei.GetBytes(msg));
                    string NowTime = DateTime.Now.ToString("HH:mm:ss");
                    string[] chatinfo = new string[] { NowTime + " " + selectrow.Name, msg };
                    Chatroom.Children.Add(CreateChatSet(chatinfo, false));
                }
                else
                {
                    Tbl_message.Text = "failed to Chat with " + selectrow.Name;
                    SetMessageInfo(false);
                }

            }
            catch (ObjectDisposedException)
            {
                Tbl_message.Text = selectrow.Name + " is left.";
                SetMessageInfo(false);
            }
            catch (Exception ex)
            {
                Tbl_message.Text = ex.Message;
                SetMessageInfo(false);
            }
        }
        private void GetUser_Tick(object sender, EventArgs e)
        {
            List<ChatList> newInfo = new List<ChatList>();
            JObject GetList = FileMethod.GetChatList(SelfInfo.Token);
            if ((bool)GetList["success"])
            {
                foreach (JToken chatroom in GetList["data"])
                {
                    if ((int)chatroom["userid"] == SelfInfo.Userid)
                    {
                        continue;
                    }
                    ChatList finduser = ChatList.Find(c => c.Ipaddress == (string)chatroom["ipaddress"] && c.Port == (int)chatroom["port"]);
                    if (finduser == null)
                    {
                        ChatList chatlist = new ChatList
                        {
                            Name = (string)chatroom["name"],
                            Ipaddress = (string)chatroom["ipaddress"],
                            Port = (int)chatroom["port"],
                            Getcall = false
                        };
                        ChatList.Add(chatlist);
                    }
                }
                _syncContext.Post(SetChatList, null);
            }
        }
    }
}
