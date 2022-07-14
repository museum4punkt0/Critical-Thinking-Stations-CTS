using Gemelo.Components.Cts.Common.Code.Json;
using Newtonsoft.Json;

namespace Gemelo.Components.Cts.Code.Data.Stations
{
    public class StationDefinitionBase
    {
        public string ToJson()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = LocalizationStringContractResolver.Instance
            };
            return JsonConvert.SerializeObject(value: this, formatting: Formatting.None, settings: settings);
        }
    }
}
