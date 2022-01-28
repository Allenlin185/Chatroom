
using System.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Sockets;

namespace Chatroom
{
    public class FileMethod
    {
        public string GetConfigSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
        public JObject Signin(string email, string password)
        {
            HttpClient Client = new HttpClient();
            JObject jObject;
            try
            {
                Client.BaseAddress = new Uri(GetConfigSetting("WebServer"));
                Client.DefaultRequestHeaders.Accept.Clear();
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                FormUrlEncodedContent formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("email", email),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("source", "Chatroom")
                });
                HttpResponseMessage ResponseMessage = Client.PostAsync("api/login", formContent).Result;
                string responseJson = ResponseMessage.Content.ReadAsStringAsync().Result;
                jObject = JObject.Parse(responseJson);
            }
            catch (Exception)
            {
                string responseJson = @"{
                    ""success"" : false,
                    ""message"" : ""Can't connect web chatroom.""
                }";
                jObject = JObject.Parse(responseJson);
            }
            return jObject;
        }
        public JObject Signout(string token)
        {
            HttpClient Client = new HttpClient();
            JObject jObject;
            try
            {
                Client.BaseAddress = new Uri(GetConfigSetting("WebServer"));
                Client.DefaultRequestHeaders.Accept.Clear();
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage ResponseMessage = Client.PostAsync("api/logout", null).Result;
                string responseJson = ResponseMessage.Content.ReadAsStringAsync().Result;
                jObject = JObject.Parse(responseJson);
            }
            catch (Exception)
            {
                string responseJson = @"{
                    ""success"" : false,
                    ""message"" : ""Can't connect web chatroom.""
                }";
                jObject = JObject.Parse(responseJson);
            }
            return jObject;
        }
        public JObject IntroChatroom(string token)
        {
            HttpClient Client = new HttpClient();
            string responseJson = @"{
                ""success"" : false,
                ""message"" : ""Can't intro chatroom.""
            }";
            JObject jObject = JObject.Parse(responseJson);
            string ipaddress = "";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipaddress = ip.ToString();
                }
            }
            try
            {
                Client.BaseAddress = new Uri(GetConfigSetting("WebServer"));
                Client.DefaultRequestHeaders.Accept.Clear();
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                FormUrlEncodedContent formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("ipaddress", ipaddress),
                });
                HttpResponseMessage ResponseMessage = Client.PostAsync("api/introchatroom", formContent).Result;
                responseJson = ResponseMessage.Content.ReadAsStringAsync().Result;
                jObject = JObject.Parse(responseJson);
            }
            catch (Exception)
            {
                //jObject = JObject.Parse(responseJson);
            }
            return jObject;
        }
        public JObject GetChatList(string token)
        {
            HttpClient Client = new HttpClient();
            JObject jObject;
            try
            {
                Client.BaseAddress = new Uri(GetConfigSetting("WebServer"));
                Client.DefaultRequestHeaders.Accept.Clear();
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage ResponseMessage = Client.PostAsync("api/getchatlist", null).Result;
                string responseJson = ResponseMessage.Content.ReadAsStringAsync().Result;
                jObject = JObject.Parse(responseJson);
            }
            catch (Exception)
            {
                string responseJson = @"{
                    ""success"" : false,
                    ""message"" : ""Failed to get chat list.""
                }";
                jObject = JObject.Parse(responseJson);
            }
            return jObject;
        }
    }
    public class ChatList
    {
        public string Name { get; set; }
        public bool Getcall { get; set; }
        public string Ipaddress { get; set; }
        public int Port { get; set; }
    }

    public class UserInfo
    {
        public int Userid { get; set; }
        public string Ipaddress { get; set; }
        public int Port { get; set; }
        public string Token { get; set; }
    }
}
