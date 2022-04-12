using System;
using System.Collections.Generic;
using Limilabs.Client.IMAP;
using Limilabs.Mail;
using Microsoft.Extensions.Logging;
using WaspIntegration.Service.Interfaces;

namespace WaspIntegration.Business.Services
{
    public class MailService : IMailService
    {
        private readonly ILogger<MailService> _logger;
        private const string Host = "imap.gmail.com";

        public MailService(ILogger<MailService> logger)
        {
            _logger = logger;
        }

        public string ReadInboxLetters(string mail, string password)
        {
            string textFromEmail = null;
            try
            {
                _logger.LogInformation($"**Trying to connect to this email => {mail}**");
                using (var imap = new Imap())
                {
                    imap.ConnectSSL(Host);
                    imap.UseBestLogin(mail, password);
                    _logger.LogInformation("**Connection was establish successfully**");
                    imap.SelectInbox();
                    List<long> uids = imap.Search(Flag.Unseen);
                    foreach (long uid in uids)
                    {
                        IMail email = new MailBuilder()
                            .CreateFromEml(imap.GetMessageByUID(uid));

                        textFromEmail = email.GetBodyAsText();
                    }

                    imap.Close();
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation($"**Failed while using MailService, with message {e.Message}**");
            }

            return textFromEmail;
        }
    }
}