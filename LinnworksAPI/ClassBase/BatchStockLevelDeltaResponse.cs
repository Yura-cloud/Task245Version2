using System.Collections.Generic;
using System;

namespace LinnworksAPI
{ 
    public class BatchStockLevelDeltaResponse
	{
		public List<BatchStockLevelDelta> ProcessedDeltas { get; set; }

		public Boolean ProcessedContainsErrors { get; set; }
	} 
}