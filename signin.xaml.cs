using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Chatroom
{
    /// <summary>
    /// signin.xaml 的互動邏輯
    /// </summary>
    public partial class signin : Page
    {
        private MainWindow Chatroom;
        private FileMethod FileMethod = new FileMethod();
        public signin(MainWindow mainWindow)
        {
            InitializeComponent();
            Chatroom = mainWindow;
        }

        private void Close_page(object sender, RoutedEventArgs e)
        {
            Leave_page();
        }

        private void Signin_webapi(object sender, RoutedEventArgs e)
        {
            bool CanSend = false;
            if (Tb_email.Text == "")
            {
                Tbl_emailerror.Text = "Please input email.";
            }
            else
            {
                if (Tb_email.Text.EndsWith("."))
                {
                    Tbl_emailerror.Text = "This is not email address.";
                }
                else
                {
                    try
                    {
                        var addr = new MailAddress(Tb_email.Text);
                        Tbl_emailerror.Text = "";
                        CanSend = true;
                    }
                    catch
                    {
                        Tbl_emailerror.Text = "This is not email address.";
                    }
                }
            }
            if (Tb_password.Password == "")
            {
                Tbl_passworderror.Text = "Please input password.";
            }
            else
            {
                Tbl_passworderror.Text = "";
                if (CanSend) SigninWebApi();
            }
            Tb_email.BorderBrush = (Tbl_emailerror.Text != "") ? new SolidColorBrush(Color.FromRgb(255, 0, 0)) : new SolidColorBrush(Color.FromRgb(103, 103, 103));
            Tb_password.BorderBrush = (Tbl_passworderror.Text != "") ? new SolidColorBrush(Color.FromRgb(255, 0, 0)) : new SolidColorBrush(Color.FromRgb(103, 103, 103));
        }
        private void SigninWebApi()
        {
            JObject Signin = FileMethod.Signin(Tb_email.Text, Tb_password.Password);
            if ((bool)Signin["success"])
            {
                Chatroom.username.Content = Signin["data"]["name"].ToString();
                Chatroom.SelfInfo.Token = Signin["data"]["token"].ToString();
                Chatroom.SelfInfo.Userid = (int)Signin["data"]["userid"];
                Chatroom.Bt_signin.Visibility = Visibility.Hidden;
                Chatroom.Bt_signout.Visibility = Visibility.Visible;
                Chatroom.Tbl_message.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            }
            else
            {
                Chatroom.Tbl_message.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }
            Chatroom.Tbl_message.Text = (string)Signin["message"];
            Leave_page();
        }
        private void Leave_page()
        {
            NavigationService.Navigate(null);
            Chatroom.dialog.Content = null;
            Chatroom.dialog.Visibility = Visibility.Hidden;
            Chatroom.markpage.Visibility = Visibility.Hidden;
            Chatroom.markpage.Visibility = Visibility.Hidden;
        }
    }
}
