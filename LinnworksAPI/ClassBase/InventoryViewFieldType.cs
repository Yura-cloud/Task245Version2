using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace LinnworksAPI
{ 
    [JsonConverter(typeof(StringEnumConverter))]
	public enum InventoryViewFieldType
	{
		Int,
		Double,
		String,
		Boolean,
		Select,
		Date,
		Datetime2,
	}
}