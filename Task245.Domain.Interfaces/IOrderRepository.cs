using LinnworksAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task245.Domain.Interfaces
{
    public interface IOrderRepository
    {
        OrderDetails GetOrderById(Guid id);
    }
}
