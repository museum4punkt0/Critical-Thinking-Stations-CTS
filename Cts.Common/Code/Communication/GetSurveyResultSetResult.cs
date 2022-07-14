using Gemelo.Components.Cts.Code.Data.Survey;
using Gemelo.Components.Cts.Common.Code.Json;
using Newtonsoft.Json;

namespace Gemelo.Components.Cts.Code.Communication
{
    public class GetSurveyResultSetResult : BaseResult
    {
        public SurveyResultSet ResultSet { get; set; }

        public static bool TryFromJson(string json, out GetSurveyResultSetResult result)
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    MissingMemberHandling = MissingMemberHandling.Error,
                    ContractResolver = LocalizationStringContractResolver.Instance
                };
                result = JsonConvert.DeserializeObject<GetSurveyResultSetResult>(value: json, settings: settings);
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
