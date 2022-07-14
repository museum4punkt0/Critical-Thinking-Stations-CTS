using Gemelo.Components.Cts.Code.Data.Users;
using Gemelo.Components.Cts.Common.Code.Json;
using Newtonsoft.Json;
using System;

namespace Gemelo.Components.Cts.Code.Communication
{
    public class GetUserForRfidResult : BaseResult
    {
        public CtsUserInformation CtsUserInformation { get; set; }

        public static bool TryFromJson(string json, out GetUserForRfidResult result)
        {
            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    MissingMemberHandling = MissingMemberHandling.Error,
                    ContractResolver = LocalizationStringContractResolver.Instance
                };
                result = JsonConvert.DeserializeObject<GetUserForRfidResult>(value: json, settings: settings);
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
