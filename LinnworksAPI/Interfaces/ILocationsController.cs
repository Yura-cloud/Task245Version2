using System;

namespace LinnworksAPI
{ 
    public interface ILocationsController
	{
		void AddLocation(StockLocation location);
		void DeleteLocation(Guid pkStockLocationId);
		DeleteWarehouseToteResponse DeleteWarehouseTOTE(DeleteWarehouseToteRequest request);
		StockLocation GetLocation(Guid pkStockLocationId);
		GetWarehouseTotesResponse GetWarehouseTOTEs(GetWarehouseTotesRequest request);
		void UpdateLocation(StockLocation location);
	} 
}