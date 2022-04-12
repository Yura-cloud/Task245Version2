using LinnworksAPI;
using LinnworksMacroHelpers;
using LinnworksMacroHelpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLibrary
{
    public class Myclass : LinnworksMacroBase, IMainService
    {
        public Myclass(ILogger logger, ApiObjectManager api)
        {
            Logger = logger;
            Api = api;
        }
        public void LoadManifest()
        {
            var ids = new List<Guid>
                {
                    new Guid("29d7b0fa-42a8-4ce4-85e3-7a14576f0b2a"),
                    new Guid("d493c15d-a2be-4fb2-9c52-862ea2ed36c1")
                };

            Execute(ids.ToArray());

            base.Logger.WriteDebug("Macro is started");

            var orders = GetOrders(ids.ToArray());

            var info = GetManifestInfos(orders);

            Console.WriteLine(info);

            base.Logger.WriteDebug("Macro is finished");
        }

        public void LoadOrders()
        {
            throw new NotImplementedException();
        }

        public void Execute(Guid[] OrdersIds)
        {
            base.Logger.WriteDebug("Macro is started");

            var orders = GetOrders(OrdersIds);

            var info = GetManifestInfos(orders);

            Console.WriteLine(info);

            base.Logger.WriteDebug("Macro is finished");
        }

        private StringBuilder GetManifestInfos(List<OrderDetails> orders)
        {
            var totalInfo = new StringBuilder();

            foreach (var order in orders)
            {
                totalInfo.Append(CreateInfoFromOrder(order));
            }

            return totalInfo;
        }

        private string CreateInfoFromOrder(OrderDetails order)
        {
            string info = order.GeneralInfo.Source
                        + order.GeneralInfo.SiteCode
                        + order.CustomerInfo.ChannelBuyerName
                        + order.GeneralInfo.ReferenceNum
                        + order.GeneralInfo.ReceivedDate.ToString("dd.MM.yyyy")
                        + "100001"
                        + order.GeneralInfo.ExternalReferenceNum
                        + order.GeneralInfo.DespatchByDate.ToString("dd.MM.yyyy")
                        + order.GeneralInfo.SubSource
                        + "\n";

            return info;
        }

        private List<OrderDetails> GetOrders(Guid[] ids)
        {
            var orders = new List<OrderDetails>();
            foreach (var id in ids)
            {
                var order = GetOrder(id);
                if (order.OrderId != Guid.Empty)
                {
                    orders.Add(order);
                }
                else
                {
                    base.Logger.WriteDebug($"Order with this Id -> {id} does not exist!");
                }
            }
            return orders;
        }


        private OrderDetails GetOrder(Guid id)
        {
            try
            {
                var order = Api.Orders.GetOrderById(id);

                return order ?? new OrderDetails();
            }
            catch (Exception ex)
            {
                base.Logger.WriteError($"Failed while getting order with this id -> {id}, with message{ex.Message}");
                return new OrderDetails();
            }

        }
    }
}
