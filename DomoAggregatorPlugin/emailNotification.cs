using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using System.ComponentModel;
using System.IO;
using System.Net.Configuration;
using System.Reflection;
using GalaSoft.MvvmLight.Helpers;
using Newtonsoft.Json;
using WorkbenchPlugin.Views.Plugin.v3;

namespace DomoAggregatorPlugin
{
    public class EmailNotification
    {
        readonly string _username;
        readonly string _password;
        readonly dynamic _toList;
        

        public EmailNotification()
        {
            string path = Directory.GetParent(Directory.GetParent(Path.Combine(AppDomain.CurrentDomain.BaseDirectory)).ToString()) +"\\emailJsonConfig.json";
            var jsonFileLocation = File.ReadAllText(path);
            var jsonEmailList = JsonConvert.DeserializeObject<dynamic>(jsonFileLocation);
            var destinationEmails = JsonConvert.DeserializeObject<List<dynamic>>(jsonEmailList.emailList.ToString());

            _toList = destinationEmails;
            _username = jsonEmailList.sendingEmailCredentials.username.ToString();
            _password = jsonEmailList.sendingEmailCredentials.password.ToString();
        }

        public void EmailNotificationSender(string exception)
        {
            //Establishes client connection
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(_username, _password);
           
            MailMessage msg = new MailMessage();

            //foreach (var emailList in _toList[0].emailList)
            //{
            //    msg.To.Add(emailList.emailAddress.ToString());
            //}
            foreach (var emailAddress in _toList)
            {
                msg.To.Add(emailAddress);
            }
            msg.From = new MailAddress("DomoPluginError@gmail.com");
            msg.Subject = "DOMO Aggregator Plugin Exception";
            msg.Body = ($"Plugin failed and threw Exception: {Environment.NewLine}{exception}");
            client.Send(msg);
           
        }
    }
}
