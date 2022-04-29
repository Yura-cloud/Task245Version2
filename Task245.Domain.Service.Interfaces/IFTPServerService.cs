using Renci.SshNet;
using WaspIntegration.Domain;

namespace WaspIntegration.Service.Interfaces
{
    public interface IFtpServerService
    {
        FtpSettingsModel FtpSettings { get; set; }
        string[] GetRowsOfOrdersFromServer();
        string[] GetRowsOfOrdersFromLocalComputer();
        bool WriteFilesToServer(string content);
    }
}