using System.Collections.Generic;
using System;

namespace LinnworksAPI
{ 
    public class AddItemsToTransferRequest
	{
		public Guid TransferId { get; set; }

		public List<WarehouseTransferItemQuantity> TransferItems { get; set; }
	} 
}