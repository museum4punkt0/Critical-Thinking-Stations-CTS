using Gemelo.Components.Common.Collections;
using Gemelo.Components.Common.Localization;
using Gemelo.Components.Common.Text;
using Gemelo.Components.Cts.Code.Data.Survey;
using Gemelo.Components.Cts.Common.Code.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gemelo.Components.Cts.Code.Data.Stations
{
    public class WallStationDefinition : StationDefinitionBase
    {
        #region Konstanten

        public static readonly string StationID = "Wall";

        #endregion Konstanten

        public LocalizationString Title { get; set; }

        public LocalizationString Description { get; set; }

        public LocalizationString PageProgressTextFormat { get; set; }

        public TimeSpan PageInterval { get; set; } = TimeSpan.FromMinutes(5.0);

        public TimeSpan QuestionShowUpDuration { get; set; } = TimeSpan.FromSeconds(4.0);

        public TimeSpan QuestionShowUpInterval { get; set; } = TimeSpan.FromSeconds(8.0);

        public string BackgroundLink { get; set; } 

        public LocalizationString AnswersCountText { get; set; }

        public List<SurveyQuestion> SurveyQuestions { get; set; } = new List<SurveyQuestion>();

        public static WallStationDefinition FromJson(string json)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Error,
                ContractResolver = LocalizationStringContractResolver.Instance
            };
            return JsonConvert.DeserializeObject<WallStationDefinition>(json, settings: settings);
        }

        public int GetPageCount()
        {
            if (SurveyQuestions.IsNullOrEmpty()) return 0;
            return SurveyQuestions.Select(question => question.WallInfo?.PageIndex ?? 0).Max() + 1;
        }

        public IEnumerable<List<SurveyQuestion>> GetQuestionGroupsForPage(int pageIndex)
        {
            return SurveyQuestions.Where(question => question.WallInfo?.PageIndex == pageIndex)
                .GroupBy(question => question.GroupID.IsNullOrWhiteSpace() ? question.QuestionID : question.GroupID)
                .Select(questionGroup => questionGroup.ToList());                
        }
    }
}
