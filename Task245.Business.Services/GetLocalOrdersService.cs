using System.IO;
using WaspIntegration.Service.Interfaces;

namespace WaspIntegration.Business.Services
{
    public class GetLocalOrdersService : IFtpDownLoaderService
    {
        public string[] GetRowsOfOrders()
        {
            var textFile = "C:\\Users\\Yura\\OneDrive\\Desktop\\WaspTest\\Orders.txt";
            string[] totalOrders = File.ReadAllLines(textFile);
            return totalOrders;
        }
    }
}