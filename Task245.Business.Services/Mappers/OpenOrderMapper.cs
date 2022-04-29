using System;
using LinnworksAPI;
using LinnworksMacroHelpers.Helpers;

namespace WaspIntegration.Business.Services.Mappers
{
    public class OpenOrderMapper
    {
        private const string Source = "Wasp";

        private const string SubSource = "Studio";

        public static void MapToOpenOrder(string[] order, OpenOrder openOrder)
        {
            openOrder.CustomerInfo.ChannelBuyerName = order[2];
            openOrder.CustomerInfo.Address.FullName = $"{order[16]} {order[17]}";
            openOrder.CustomerInfo.Address.Company = order[37];
            openOrder.CustomerInfo.Address.Address1 = order[29];
            openOrder.CustomerInfo.Address.Address2 = $"{order[30]} {order[31]}";
            openOrder.CustomerInfo.Address.Address3 = $"{order[32]} {order[33]}";
            openOrder.CustomerInfo.Address.Town = order[46];
            openOrder.CustomerInfo.Address.Region = order[14];
            openOrder.CustomerInfo.Address.PostCode = order[34];
            openOrder.CustomerInfo.Address.PhoneNumber = order[13];

            openOrder.GeneralInfo.ReferenceNum = order[3];
            openOrder.GeneralInfo.ReceivedDate =
                Convert.ToDateTime(ConvertorDateTimeHelper.ParseToCurrentCulture(order[4]));
            openOrder.GeneralInfo.SecondaryReference = order[5];
            openOrder.GeneralInfo.ExternalReferenceNum = order[6];
            openOrder.GeneralInfo.DespatchByDate =
                Convert.ToDateTime(ConvertorDateTimeHelper.ParseToCurrentCulture(order[28]));
            openOrder.GeneralInfo.Source = Source;
            openOrder.GeneralInfo.SubSource = SubSource;
            openOrder.GeneralInfo.Status = 0;

            openOrder.ShippingInfo.PostalServiceName = order[11];
            openOrder.ShippingInfo.TrackingNumber = order[26];
            openOrder.ShippingInfo.TotalWeight = Convert.ToDecimal(order[75]);
            openOrder.ShippingInfo.Vendor = order[73];

            openOrder.Items.Add(new OrderItem {ItemNumber = order[7], Title = order[8]});
        }
    }
}