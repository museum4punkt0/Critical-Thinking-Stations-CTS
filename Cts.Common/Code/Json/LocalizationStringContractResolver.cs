using Gemelo.Components.Common.Localization;
using Newtonsoft.Json.Serialization;
using System;

namespace Gemelo.Components.Cts.Common.Code.Json
{
    public class LocalizationStringContractResolver : DefaultContractResolver
    {
        private static LocalizationStringContractResolver s_Instance = new LocalizationStringContractResolver();

        public static LocalizationStringContractResolver Instance => s_Instance;

        public override JsonContract ResolveContract(Type type)
        {
            JsonContract contract = base.ResolveContract(type);
            if ((type == typeof(LocalizationString)))
            {
                contract.Converter = new LocalizationStringConverter();
            }
            return contract;
        }
    }
}
