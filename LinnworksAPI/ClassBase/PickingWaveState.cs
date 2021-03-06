using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace LinnworksAPI
{ 
    [JsonConverter(typeof(StringEnumConverter))]
	public enum PickingWaveState
	{
		Unallocated,
		Allocated,
		InProgress,
		Paused,
		Complete,
		Abandoned,
		Packing,
		Shipped,
	}
}