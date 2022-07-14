using Gemelo.Components.Cts.Common.Code.Json;
using Newtonsoft.Json;

namespace Gemelo.Components.Cts.Code.Communication
{
    public class SaveDeepDiveVisitResult : BaseResult
    {
        public static bool TryFromJson(string json, out SaveDeepDiveVisitResult result)
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    MissingMemberHandling = MissingMemberHandling.Error,
                    ContractResolver = LocalizationStringContractResolver.Instance
                };
                result = JsonConvert.DeserializeObject<SaveDeepDiveVisitResult>(value: json, settings: settings);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}
