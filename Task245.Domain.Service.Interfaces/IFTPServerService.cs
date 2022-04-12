using Renci.SshNet;
using WaspIntegration.Domain;

namespace WaspIntegration.Service.Interfaces
{
    public interface IFtpServerService
    {
        FtpSettingsModel FtpSettings { get; set; }
        string[] GetLinesOfOrdersFromServer();
        string[] GetLinesOfOrdersFromLocalComputer();
        bool WriteFilesToServer(string content);
    }
}