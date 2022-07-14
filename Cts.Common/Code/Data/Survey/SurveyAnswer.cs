using Gemelo.Components.Common.Localization;
using System.Text.Json.Serialization;

namespace Gemelo.Components.Cts.Code.Data.Survey
{
    public class SurveyAnswer
    {
        public string AnswerID { get; set; }

        public LocalizationString Text { get; set; }

        public bool IsExclusive { get; set; }

        [JsonIgnore]
        public string TextDe => Text.GetFor(Languages.German);

        [JsonIgnore]
        public string TextEn => Text.GetFor(Languages.English);

        [JsonIgnore]
        public string TextCurrent => Text.GetFor(Languages.Current);
    }
}
