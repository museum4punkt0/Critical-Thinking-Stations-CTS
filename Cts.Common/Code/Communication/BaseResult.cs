using Gemelo.Components.Cts.Common.Code.Json;
using Newtonsoft.Json;

namespace Gemelo.Components.Cts.Code.Communication
{
    public class BaseResult
    {
        public bool IsSuccessful { get; set; }

        public string Message { get; set; }

        public virtual string ToJson()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = LocalizationStringContractResolver.Instance
            };
            return JsonConvert.SerializeObject(value: this, formatting: Formatting.None, settings: settings);
        }
    }
}
