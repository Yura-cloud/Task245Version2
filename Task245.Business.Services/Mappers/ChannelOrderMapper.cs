using System;
using System.Collections.Generic;
using LinnworksAPI;
using LinnworksMacroHelpers.Helpers;

namespace WaspIntegration.Business.Services.Mappers
{
    public class ChannelOrderMapper
    {
        private const string Source = "Wasp";
        private const string SubSource = "Studio";

        public static ChannelOrder MapOrdersRowsToChannelOrders(string orderLine)
        {
            var channelOrder = new ChannelOrder();
            var splitProperties = orderLine.Split(';');

            channelOrder.ChannelBuyerName = splitProperties[2];
            channelOrder.OrderIdentifierTags = new HashSet<string>();

            channelOrder.DeliveryAddress = new ChannelAddress
            {
                FullName = $"{splitProperties[16]} {splitProperties[17]}",
                Company = splitProperties[37],
                Address1 = splitProperties[29],
                Address2 = $"{splitProperties[30]} {splitProperties[31]}",
                Address3 = $"{splitProperties[32]} {splitProperties[33]}",
                Town = splitProperties[46],
                Region = splitProperties[14],
                PostCode = splitProperties[34],
                PhoneNumber = splitProperties[13]
            };

            channelOrder.ReferenceNumber = splitProperties[3];
            channelOrder.ReceivedDate =
                Convert.ToDateTime(ConvertorDateTimeHelper.ParseToCurrentCulture(splitProperties[4]));
            channelOrder.SecondaryReferenceNumber = splitProperties[5];
            channelOrder.ExternalReference = splitProperties[6];
            channelOrder.DispatchBy =
                Convert.ToDateTime(ConvertorDateTimeHelper.ParseToCurrentCulture(splitProperties[28]));
            channelOrder.Source = Source;
            channelOrder.SubSource = SubSource;
            channelOrder.PaymentStatus = PaymentStatus.Paid;
            channelOrder.PostalServiceName = splitProperties[11];

            channelOrder.ExtendedProperties = new List<ChannelOrderExtendedProperty>()
            {
                new ChannelOrderExtendedProperty
                {
                    Name = "Dispatched",
                    Type = "Attribute",
                    Value = "NoAction"
                },
                new ChannelOrderExtendedProperty
                {
                    Name = "HaulierCode",
                    Type = "Attribute",
                    Value = splitProperties[10]
                }
            };

            channelOrder.OrderItems = new List<ChannelOrderItem>
            {
                new ChannelOrderItem
                {
                    TaxCostInclusive = true,
                    UseChannelTax = true,
                    Qty = 1,
                    ItemNumber = splitProperties[7],
                    ChannelSKU = splitProperties[7],
                    IsService = false,
                    ItemTitle = splitProperties[8]
                }
            };

            return channelOrder;
        }
    }
}