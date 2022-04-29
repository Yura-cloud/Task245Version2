using System.IO;
using WaspIntegration.Service.Interfaces;

namespace WaspIntegration.Business.Services
{
    public class GetLocalOrdersService : IDownloadOrdersService
    {
        public string[] GetRowsOfOrders()
        {
            var textFile = "C:\\Users\\Yura\\OneDrive\\Desktop\\New folder\\OneOrder.txt";
            string[] totalOrders = File.ReadAllLines(textFile);
            return totalOrders;
        }
    }
}