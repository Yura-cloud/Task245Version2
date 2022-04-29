using System;
using System.Collections.Generic;
using System.Linq;
using LinnworksAPI;
using LinnworksMacroHelpers;
using LinnworksMacroHelpers.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WaspIntegration.Domain;
using WaspIntegration.Service.Interfaces;

namespace WaspIntegration.Business.Services
{
    public class CanceledOrdersService : ICanceledOrdersService
    {
        private readonly ILogger<CanceledOrdersService> _logger;
        private const int ParkedTag = 7;
        private const string SupplierCode = "R0404";
        private Guid FulfilmentCenter { get; set; }

        public List<ManifestOrderInfoModel> ManifestInfos { get; set; }
        public LinnworksMacroBase LinnWorks { get; set; }

        public CanceledOrdersService(ILogger<CanceledOrdersService> logger)
        {
            _logger = logger;
            LinnWorks = new LinnworksMacroBase();
        }

        public void ParkingCanceledOrders(string emailText, IConfiguration configuration, string token,
            string locationName)
        {
            LinnWorks.Api = InitializeHelper.GetApiManagerForCanceledOrders(configuration, token);

            var locationId = GetLocationId(locationName);
            if (locationId == null)
            {
                _logger.LogInformation($"**There is no location name with the given parameter => {locationName}**");
                return;
            }

            FulfilmentCenter = locationId.Value;

            var ordersDetails = GetOrdersDetailsFromEmail(emailText);
            if (ordersDetails.Count == 0)
            {
                _logger.LogInformation("**There are no canceled orders in Mail**");
                return;
            }

            var tagChanged = ChangeOrdersTag(ordersDetails.Select(o => o.OrderId).ToList(), ParkedTag);

            if (tagChanged)
            {
                AddNotes(ordersDetails, "**This order was canceled in Wasp`s system**");
                return;
            }

            _logger.LogInformation($"**It is not possible to add Note cause program failed to change Tag**");
        }

        private List<OrderDetails> GetOrdersDetailsFromEmail(string emailText) 
        {
            var manifestOrders = GetManifestOrders(emailText);
            if (manifestOrders.Count == 0)
            {
                return new List<OrderDetails>();
            }

            var ordersDetails = new List<OrderDetails>();
            foreach (var order in manifestOrders)
            {
                var orderDetails = GetOrderDetails(order);
                if (orderDetails.OrderId != Guid.Empty)
                {
                    ordersDetails.Add(orderDetails);
                }
            }

            return ordersDetails;
        }

        private OrderDetails GetOrderDetails(ManifestOrderInfoModel orderOrderInfo)
        {
            var ordersIds = GetOrdersIds(orderOrderInfo);
            if (ordersIds.Count == 0)
            {
                _logger.LogInformation($"**There is no OpenOrders, whit this {orderOrderInfo.OrderNumber} refNumber**");
                return new OrderDetails();
            }

            if (ordersIds.Count > 1)
            {
                _logger.LogInformation(
                    $"**There are several OpenOrders with this {orderOrderInfo.OrderNumber} refNumber**");
                return new OrderDetails();
            }

            return GetOpenOrderDetails(ordersIds.FirstOrDefault());
        }

        private OrderDetails GetOpenOrderDetails(Guid orderId)
        {
            try
            {
                return LinnWorks.Api.Orders.GetOrderById(orderId);
            }
            catch (Exception e)
            {
                _logger.LogError($"**Failed while using GetOpenOrdersDetails, with message {e.Message}**");
                return new OrderDetails();
            }
        }

        private List<Guid> GetOrdersIds(ManifestOrderInfoModel orderOrderInfo)
        {
            var filters = new FieldsFilter
            {
                TextFields = new List<TextFieldFilter>
                {
                    new TextFieldFilter
                    {
                        FieldCode = FieldCode.GENERAL_INFO_REFERENCE_NUMBER,
                        Text = orderOrderInfo.OrderNumber,
                        Type = TextFieldFilterType.Equal
                    }
                },
                DateFields = new List<DateFieldFilter>
                {
                    new DateFieldFilter
                    {
                        FieldCode = FieldCode.GENERAL_INFO_DATE,
                        DateFrom = Convert.ToDateTime(
                            ConvertorDateTimeHelper.ParseToCurrentCulture(orderOrderInfo.OrderDate)),
                        DateTo = Convert.ToDateTime(
                            ConvertorDateTimeHelper.ParseToCurrentCulture(orderOrderInfo.OrderDate))
                    }
                }
            };
            try
            {
                return LinnWorks.Api.Orders.GetAllOpenOrders(filters, null, FulfilmentCenter, null, true);
            }
            catch (Exception e)
            {
                _logger.LogError($"**Failed while GetOpenOrders, whit message {e.Message}**");
                return new List<Guid>();
            }
        }

        private void AddNotes(List<OrderDetails> ordersDetails, string message)
        {
            foreach (var orderDetail in ordersDetails)
            {
                orderDetail.Notes.Add(
                    new OrderNote
                    {
                        OrderId = orderDetail.OrderId,
                        NoteDate = DateTime.Now,
                        Note = message
                    }
                );
                SetNote(orderDetail.OrderId, orderDetail.Notes);
            }
        }

        private List<ManifestOrderInfoModel> GetManifestOrders(string textFromEmail)
        {
            var manifestOrdersInfo = new List<ManifestOrderInfoModel>();
            string[] separator = {SupplierCode};
            var orders = textFromEmail.Split(separator, StringSplitOptions.None);
            for (int i = 1; i < orders.Length; i++)
            {
                manifestOrdersInfo.Add(new ManifestOrderInfoModel
                {
                    OrderNumber = orders[i].Substring(11, 13),
                    CompanyCode = orders[i].Substring(0, 3),
                    OrderDate = orders[i].Substring(24, 10)
                });
            }

            return manifestOrdersInfo;
        }

        private Guid? GetLocationId(string locationName)
        {
            var stockLocations = GetStockLocations();

            var stockLocation = stockLocations.FirstOrDefault(l =>
                string.Equals(l.LocationName, locationName, StringComparison.CurrentCultureIgnoreCase));

            return stockLocation?.StockLocationId;
        }

        private IEnumerable<InventoryStockLocation> GetStockLocations()
        {
            try
            {
                return LinnWorks.Api.Inventory.GetStockLocations();
            }
            catch (Exception e)
            {
                _logger.LogError($"**Failed while GetStockLocations() with, message{e.Message}**");
                return new List<InventoryStockLocation>();
            }
        }

        private void SetNote(Guid orderId, List<OrderNote> notes)
        {
            try
            {
                LinnWorks.Api.Orders.SetOrderNotes(orderId, notes);
            }
            catch (Exception e)
            {
                _logger.LogInformation(
                    $"**Failed while SetOrderNotes, in this OrderId => {orderId}, with message {e.Message}**");
            }
        }

        private bool ChangeOrdersTag(List<Guid> ordersIds, int tag)
        {
            try
            {
                LinnWorks.Api.Orders.ChangeOrderTag(ordersIds, tag);
            }
            catch (Exception e)
            {
                _logger.LogInformation($"**Failed while ChangeOrderTag, with message {e.Message}**");
                return false;
            }

            return true;
        }
    }
}