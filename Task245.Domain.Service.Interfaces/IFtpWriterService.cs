using WaspIntegration.Domain;

namespace WaspIntegration.Service.Interfaces
{
    public interface IFtpWriterService
    {
        FtpSettingsModel FtpSettings { get; set; }
        bool WriteFilesToServer(string content);
    }
}