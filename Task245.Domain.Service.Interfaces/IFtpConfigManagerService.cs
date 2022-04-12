using Renci.SshNet;

namespace WaspIntegration.Service.Interfaces
{
    public interface IFtpConfigManagerService
    {
        int Port { get; }
        string UserName { get; }
        string Password { get; }
        string Host { get; }
        string ReadPath { get; }
        string WritePath { get; }
        PrivateKeyFile Key { get; }
    }
}