namespace WaspIntegration.Service.Interfaces
{
    public interface IMailService
    {
        string ReadInboxLetters(string mail, string password);
    }
}