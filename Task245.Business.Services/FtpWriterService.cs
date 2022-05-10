using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using WaspIntegration.Domain;
using WaspIntegration.Service.Interfaces;

namespace WaspIntegration.Business.Services
{
    public class FtpWriterService : IFtpWriterService
    {
        private readonly ILogger<FtpWriterService> _logger;


        public FtpSettingsModel FtpSettings { get; set; }

        public FtpWriterService(IFtpConfigManagerService ftpConfig, ILogger<FtpWriterService> logger)
        {
            _logger = logger;

            FtpSettings = new FtpSettingsModel(ftpConfig.Port, ftpConfig.UserName, ftpConfig.Password,
                ftpConfig.Host, ftpConfig.ReadPath, ftpConfig.WritePath, ftpConfig.Key);
        }

        public bool WriteFilesToServer(string content)
        {
            try
            {
                using (var client = new SftpClient(FtpSettings.Host, FtpSettings.Port, FtpSettings.UserName,
                           FtpSettings.Key))
                {
                    _logger.LogInformation("**Trying to connect**");
                    client.Connect();
                    _logger.LogInformation("**Connection to Server, was established successfully**");

                    if (client.Exists(FtpSettings.WritePath))
                    {
                        client.DeleteFile(FtpSettings.WritePath);
                    }

                    client.WriteAllText(FtpSettings.WritePath, content);
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogDebug($"**Failed while working with FTP Server, with message {e.Message}**");
                return false;
            }
        }
    }
}