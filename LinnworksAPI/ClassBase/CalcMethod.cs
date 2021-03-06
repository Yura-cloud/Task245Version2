using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace LinnworksAPI
{ 
    [JsonConverter(typeof(StringEnumConverter))]
	public enum CalcMethod
	{
		LEGACY,
		MAX,
		STACKED,
		WEIGHTFIT,
		THREEDIM_ALL,
		THREEDIM_ITEMS,
	}
}