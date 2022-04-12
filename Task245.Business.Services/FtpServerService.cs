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
    public class FtpServerService : IFtpServerService
    {
        private readonly ILogger<FtpServerService> _logger;
        public FtpSettingsModel FtpSettings { get; set; }

        public FtpServerService(IFtpConfigManagerService ftpConfig, ILogger<FtpServerService> logger)
        {
            _logger = logger;
            FtpSettings = new FtpSettingsModel(ftpConfig.Port, ftpConfig.UserName, ftpConfig.Password,
                ftpConfig.Host, ftpConfig.ReadPath, ftpConfig.WritePath, ftpConfig.Key);
        }

        //This method is only for testing purpose, not for release
        public string[] GetLinesOfOrdersFromLocalComputer()
        {
            var textFile = "C:\\Users\\Yura\\OneDrive\\Desktop\\New folder\\OneOrder.txt";
            string[] totalOrders = File.ReadAllLines(textFile);
            return totalOrders;
        }


        public string[] GetLinesOfOrdersFromServer()
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