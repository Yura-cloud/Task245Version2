using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace LinnworksAPI
{ 
    [JsonConverter(typeof(StringEnumConverter))]
	public enum SearchType
	{
		SKU,
		ITEMTITLE,
		ITEMBARCODE,
		TRANSFERREFERENCE,
		BINREFERENCE,
	}
}