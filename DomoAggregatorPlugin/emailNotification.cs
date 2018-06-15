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
using Newtonsoft.Json;
using WorkbenchPlugin.Views.Plugin.v3;

namespace DomoAggregatorPlugin
{
    public class EmailNotification
    {
        private List<string> _toList;

        public EmailNotification()
        {
            _toList = new List<string>();
            string path = Directory.GetParent(Directory.GetParent(Path.Combine(AppDomain.CurrentDomain.BaseDirectory)).ToString()).ToString() +"\\emailJsonConfig.json";
            var jsonFileLocation = File.ReadAllText(path);
            var jsonEmailList = JsonConvert.DeserializeObject<dynamic>(jsonFileLocation);
            for (int i = 0; i < jsonEmailList.emailList.Count; i++)
            {
                string emailAddress = jsonEmailList.emailList[i].emailAddress.ToString();
                _toList.Add(emailAddress);
            }

        }

        public void EmailNotificationSender(string exception)
        {
            //Establishes client connection
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("buildiumtestdummy@gmail.com", "buildium123");
           
            MailMessage msg = new MailMessage();
            //destination email address
            foreach (var emailAddress in _toList)
            {
                msg.To.Add(emailAddress);
            }
            msg.From = new MailAddress("buildiumtestdummy@gmail.com");
            msg.Subject = "DOMO Aggregator Plugin Exception";
            msg.Body = ($"Plugin failed and threw Exception: {Environment.NewLine}{exception}");
            client.Send(msg);
           
        }
    }
}
