﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;

namespace TrackerLibrary
{
    public static class EmailLogic
    {
        public static void SendEmail(string to, string subject, string body)
        {
            string fromAddress = GlobalConfig.AppKeyLookup("senderEmail");
            string fromName = GlobalConfig.AppKeyLookup("senderName");

            MailAddress fromMailAddress = new(fromAddress, fromName);
            MailMessage mail = new();

            mail.To.Add(to);
            mail.From = fromMailAddress;
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            SmtpClient client = new();

            client.Host = "127.0.0.1";
            client.Port = 25;
            client.EnableSsl = false;

            client.Send(mail);
        }
    }
}
