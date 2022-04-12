using LinnworksAPI;
using LinnworksMacroHelpers;
using System;
using Task245.Domain.Interfaces;

namespace Task245.Data
{
    public class OrderRepository : LinnworksMacroBase, IOrderRepository
    {
        public OrderDetails GetOrderById(Guid id)
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
