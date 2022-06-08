using System;
using System.Text;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.Extensions.Logging;
using WaspIntegration.Service.Interfaces;

namespace WaspIntegration.Business.Services
{
    public class MailKitService : IMailWaspService
    {
        private const string Host = "imap.gmail.com";
        private const int Port = 993;

        private readonly ILogger<MailKitService> _logger;

        public MailKitService(ILogger<MailKitService> logger)
        {
            _logger = logger;
        }

        public string ReadInboxLetters(string mail, string password, string subject)
        {
            try
            {
                var messages = new StringBuilder();
                using (var client = new ImapClient())
                {
                    client.Connect(Host, 993);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(mail, password);
                    var inbox = client.Inbox;
                    inbox.Open(FolderAccess.ReadWrite);
                    var results = inbox.Search(SearchOptions.All, SearchQuery.NotSeen);
                    foreach (var uniqueId in results.UniqueIds)
                    {
                        var message = inbox.GetMessage(uniqueId);
                        if (message.Subject == "Cancellation")
                        {
                            messages.Append(message.TextBody);
                            inbox.AddFlags(uniqueId, MessageFlags.Seen, true);
                        }
                    }

                    client.Disconnect(true);
                }

                return messages.ToString();
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed while using MailService, with message {e.Message}");
            }

            return string.Empty;
        }
    }
}