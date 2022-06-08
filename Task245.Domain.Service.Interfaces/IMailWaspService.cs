namespace WaspIntegration.Service.Interfaces
{
    public interface IMailWaspService
    {
        string ReadInboxLetters(string mail, string password, string subject);
    }
}