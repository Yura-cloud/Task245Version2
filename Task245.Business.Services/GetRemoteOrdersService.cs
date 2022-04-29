using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using WaspIntegration.Domain;
using WaspIntegration.Service.Interfaces;

namespace WaspIntegration.Business.Services
{
    public class GetRemoteOrdersService : IDownloadOrdersService
    {
        private readonly ILogger<GetRemoteOrdersService> _logger;
        public FtpSettingsModel FtpSettings { get; set; }

        public GetRemoteOrdersService(IFtpConfigManagerService ftpConfig, ILogger<GetRemoteOrdersService> logger)
        {
            _logger = logger;
            FtpSettings = new FtpSettingsModel(ftpConfig.Port, ftpConfig.UserName, ftpConfig.Password,
                ftpConfig.Host, ftpConfig.ReadPath, ftpConfig.WritePath, ftpConfig.Key);
        }

        public string[] GetRowsOfOrders()
        {
            try
            {
                using (var client = new SftpClient(FtpSettings.Host, FtpSettings.Port, FtpSettings.UserName,
                           FtpSettings.Key))
                {
                    _logger.LogInformation("**Trying to connect**");
                    client.Connect();
                    _logger.LogInformation("**Connection to Server, was established successfully**");

                    var files = client.ListDirectory(FtpSettings.ReadPath);
                    if (files == null) return Array.Empty<string>();

                    var totalOrders = new List<string>();
                    foreach (var file in files)
                    {
                        if (file.Name == "." || file.Name == "..") continue;

                        var orders = client.ReadLines(file.FullName).ToList();
                        if (!orders.Any()) continue;

                        totalOrders.AddRange(orders);
                    }

                    return totalOrders.ToArray();
                }
            }
            catch (Exception e)
            {
                _logger.LogDebug($"**Failed while working with FTP Server, with message {e.Message}**");
                return Array.Empty<string>();
            }
        }
    }
}