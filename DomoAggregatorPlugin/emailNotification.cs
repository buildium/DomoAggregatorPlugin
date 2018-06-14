using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using System.ComponentModel;
using System.Net.Configuration;

namespace DomoAggregatorPlugin
{
    public class EmailNotification
    {
        public static void EmailNotificationSender(string exception)
        {
            //Establishes client connection
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("buildiumtestdummy@gmail.com", "buildium123");
           
            //Sets up desired message and recipient
            MailMessage msg = new MailMessage();
            msg.To.Add("alex.yuan@buildium.com");
            msg.To.Add("alexyuan24@gmail.com");
            msg.From = new MailAddress("buildiumtestdummy@gmail.com");
            msg.Subject = "DOMO Aggregator Plugin Exception";
            msg.Body = ($"Plugin failed and thre Exception: {Environment.NewLine}{exception}");
            client.Send(msg);
           
        }
        
    }
}
