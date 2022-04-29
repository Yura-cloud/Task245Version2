using System;
using Microsoft.Extensions.Configuration;
using Renci.SshNet;
using WaspIntegration.Service.Interfaces;

namespace WaspIntegration.Business.Services
{
    public class FtpConfigManagerService : IFtpConfigManagerService
    {
        public int Port => Convert.ToInt32(_configuration.GetSection("FtpSettings").GetSection("Port").Value);
        public string UserName => _configuration.GetSection(("FtpSettings")).GetSection("UserName").Value;
        public string Password => _configuration.GetSection("FtpSettings").GetSection("Password").Value;
        public string Host => _configuration.GetSection(("FtpSettings")).GetSection("Host").Value;
        public string ReadPath => _configuration.GetSection(("FtpSettings")).GetSection("ReadPath").Value;
        public string WritePath => _configuration.GetSection(("FtpSettings")).GetSection("WritePath").Value;

        public PrivateKeyFile Key =>
            new PrivateKeyFile(_configuration.GetSection(("FtpSettings")).GetSection("Key").Value,
                _configuration.GetSection("FtpSettings").GetSection("Password").Value);


        private readonly IConfiguration _configuration;

        public FtpConfigManagerService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
    }
}