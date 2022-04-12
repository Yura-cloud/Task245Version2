using Renci.SshNet;

namespace WaspIntegration.Domain
{
    public class FtpSettingsModel
    {
        public FtpSettingsModel(int port, string userName, string password, string host, string readPath, string writePath, PrivateKeyFile key)
        {
            Port = port;
            UserName = userName;
            Password = password;
            Host = host;
            ReadPath = readPath;
            WritePath = writePath;
            Key = key;
        }

        public int Port { get; }
        public string UserName { get; }
        public string Password { get; }
        public string Host { get; }
        public string ReadPath { get; }
        public string WritePath { get; }
        public PrivateKeyFile Key { get; }
    }
}