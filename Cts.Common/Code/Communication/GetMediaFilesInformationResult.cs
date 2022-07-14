using Gemelo.Components.Cts.Code.Media;
using Gemelo.Components.Cts.Common.Code.Json;
using Newtonsoft.Json;

namespace Gemelo.Components.Cts.Code.Communication
{
    public class GetMediaFilesInformationResult : BaseResult
    {
        public MediaFilesInformation MediaFilesInformation { get; set; }

        public static bool TryFromJson(string json, out GetMediaFilesInformationResult result)
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    MissingMemberHandling = MissingMemberHandling.Error,
                    ContractResolver = LocalizationStringContractResolver.Instance
                };
                result = JsonConvert.DeserializeObject<GetMediaFilesInformationResult>(value: json, settings: settings);
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
