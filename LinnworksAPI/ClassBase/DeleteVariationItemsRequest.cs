using System.Collections.Generic;
using System;

namespace LinnworksAPI
{ 
    public class DeleteVariationItemsRequest
	{
		public Guid VariationItemId { get; set; }

		public List<Guid> StockItemIds { get; set; }
	} 
}