using System;
using LinnworksAPI;
using LinnworksMacroHelpers;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinnworksMacroHelpers.Helpers;
using Microsoft.Extensions.Configuration;
using WaspIntegration.Service.Interfaces;

namespace WaspIntegration.Business.Services
{
    public class ManifestService : IManifestService
    {
        private readonly ILogger<ManifestService> _logger;
        private readonly IFtpServerService _ftpServerService;
        private readonly string _orderDelayDate = new string(' ', 10);
        private readonly string _orderDelayReason = new string(' ', 2);
        private const int ParcelDespatchOutcome = 0;
        private const string SupplierCode = "A1234";
        private const string CompanyCode = "001";
        public LinnworksMacroBase LinnWorks { get; set; }

        public ManifestService(ILogger<ManifestService> logger, IFtpServerService ftpServerService)
        {
            _logger = logger;
            _ftpServerService = ftpServerService;
            LinnWorks = new LinnworksMacroBase();
        }

        public void UploadManifest(Guid? locationId, IConfiguration configuration, string token)
        {
            LinnWorks.Api = InitializeHelper.GetApiManagerForPullOrders(configuration, token);

            var query =
                $@"select O.pkOrderID from [Order] as O left join Order_ExtendedProperties as E on O.pkOrderID = E.fkOrderiD where O.bProcessed = 1 and FulfillmentLocationId = '{locationId}' and PropertyName = 'Dispatched' and PropertyValue = 'NoAction'";

            var response = ExecuteCustomScriptQuery(query);
            var ordersIds = new List<Guid>();
            if (response?.Results != null)
            {
                foreach (var keyValuePairs in response.Results)
                {
                    foreach (var pair in keyValuePairs)
                    {
                        ordersIds.Add(new Guid(pair.Value.ToString()));
                    }
                }
            }

            if (ordersIds.Count <= 0)
            {
                return;
            }

            var ordersDetails = GetOrdersDetails(ordersIds);
            if (ordersDetails.Count <= 0)
            {
                return;
            }

            var info = GetManifestInfos(ordersDetails);
            var writeToWaspServer = _ftpServerService.WriteFilesToServer(info);
            _logger.LogInformation(info);

            if (!writeToWaspServer)
            {
                return;
            }
            foreach (var orderId in ordersDetails.Select(o => o.OrderId))
            {
                SetOrderExtendedProperty(orderId);
            }
        }

        private void SetOrderExtendedProperty(Guid orderId)
        {
            var ordersExtendedProperties = GetProcessedOrderExtendedProperties(orderId);

            if (ordersExtendedProperties.Count <= 0)
            {
                return;
            }

            var extendedProperties = MapToExtendedProperties(ordersExtendedProperties);
            SetExtendedProperties(orderId, extendedProperties);
        }

        private void SetExtendedProperties(Guid orderId, ExtendedProperty[] extendedProperties)
        {
            try
            {
                LinnWorks.Api.Orders.SetExtendedProperties(orderId, extendedProperties);
            }
            catch (Exception e)
            {
                _logger.LogError(
                    $"**Failed while SetExtendedProperties, with this OrderId => {orderId}, with message {e.Message}**");
            }
        }

        private ExtendedProperty[] MapToExtendedProperties(List<OrderExtendedProperty> ordersExtendedProperties)
        {
            var extendedProperties = new ExtendedProperty[ordersExtendedProperties.Count];
            for (int i = 0; i < ordersExtendedProperties.Count; i++)
            {
                extendedProperties[i] = new ExtendedProperty
                {
                    Name = ordersExtendedProperties[i].PropertyName,
                    RowId = ordersExtendedProperties[i].rowid,
                    Type = ordersExtendedProperties[i].PropertyType,
                    Value = ordersExtendedProperties[i].PropertyName == "Dispatched"
                        ? "Sent"
                        : ordersExtendedProperties[i].PropertyValue
                };
            }

            return extendedProperties;
        }

        private List<OrderExtendedProperty> GetProcessedOrderExtendedProperties(Guid orderId)
        {
            try
            {
                return LinnWorks.Api.ProcessedOrders.GetProcessedOrderExtendedProperties(orderId);
            }
            catch (Exception e)
            {
                _logger.LogError(
                    $"**Failed while GetProcessedOrderExtendedProperties, with this OrderId => {orderId}, with this message {e.Message}**");
                return new List<OrderExtendedProperty>();
            }
        }


        private List<OrderDetails> GetOrdersDetails(List<Guid> orders)
        {
            try
            {
                return LinnWorks.Api.Orders.GetOrdersById(orders);
            }
            catch (Exception e)
            {
                _logger.LogError($"**Failed while GetOrdersById, with message {e.Message}**");
                return new List<OrderDetails>();
            }
        }

        private CustomScriptResult ExecuteCustomScriptQuery(string query)
        {
            try
            {
                var request = new ExecuteCustomScriptQueryRequest
                {
                    Script = query
                };

                var result = LinnWorks.Api.Dashboards.ExecuteCustomScriptQuery(request);

                if (!result.IsError)
                {
                    return result;
                }

                _logger.LogError($"**Error while ExecuteCustomScriptQuery in response {result.ErrorMessage}**");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"**Failed while ExecuteCustomScriptQuery with error:{ex.Message}**");
                return null;
            }
        }

        private string GetManifestInfos(List<OrderDetails> orders)
        {
            var totalInfo = new StringBuilder();

            foreach (var order in orders)
                totalInfo.Append(CreateInfoFromOrder(order))
                    .AppendLine();

            return totalInfo.ToString();
        }

        private string CreateInfoFromOrder(OrderDetails order)
        {
            var info = SupplierCode
                       + CompanyCode
                       + order.CustomerInfo.ChannelBuyerName
                       + order.GeneralInfo.ReferenceNum
                       + order.GeneralInfo.ReceivedDate.ToString("dd.MM.yyyy")
                       + order.Items.FirstOrDefault()?.ItemNumber
                       + order.GeneralInfo.ExternalReferenceNum
                       + ParcelDespatchOutcome
                       + order.GeneralInfo.DespatchByDate.ToString("dd.MM.yyyy")
                       + order.GeneralInfo.DespatchByDate.ToString("dd.MM.yyyy").PadRight(20)
                       + order.GeneralInfo.SubSource
                       + _orderDelayDate
                       + _orderDelayReason;

            return info;
        }
    }
}