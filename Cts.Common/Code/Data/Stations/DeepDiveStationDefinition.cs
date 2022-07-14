using Gemelo.Components.Common.Localization;
using Gemelo.Components.Cts.Code.Data.DeepDive;
using Gemelo.Components.Cts.Code.Data.Layout;
using Gemelo.Components.Cts.Code.Data.Survey;
using Gemelo.Components.Cts.Common.Code.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gemelo.Components.Cts.Code.Data.Stations
{
    public class DeepDiveStationDefinition : StationDefinitionBase
    {
        #region Konstanten

        public static readonly string StationID = "DeepDive";

        public static readonly string DemographyQuestionsID = "Demographie";

        private static readonly TimeSpan DefaultRestartTimeout = TimeSpan.FromSeconds(60.0);
        private static readonly TimeSpan DefaultGoodbyeTimeout = TimeSpan.FromSeconds(20.0);
        private static readonly TimeSpan DefaultTimeoutMessageDuration = TimeSpan.FromSeconds(10.0);

        #endregion Konstanten

        public LocalizationString Title { get; set; }

        public TimeSpan RestartTimeout { get; set; } = DefaultRestartTimeout;

        public TimeSpan GoodbyeTimeout { get; set; } = DefaultGoodbyeTimeout;

        public TimeSpan TimeoutMessageDuration { get; set; } = DefaultTimeoutMessageDuration;

        public LocalizationString RfidHintText { get; set; }

        public RfidHintPosition RfidHintPosition { get; set; }

        public LocalizationString DeepDiveOverviewHeadline { get; set; }

        public LocalizationString DeepDiveOverviewText { get; set; }

        public LocalizationString DeepDiveInstructionHeadline { get; set; }

        public LocalizationString DeepDiveInstructionText { get; set; }

        public LocalizationString DeepDiveOverviewButtonText { get; set; }

        public LocalizationString QuestionHeadline { get; set; }

        public LocalizationString AnswerHeadline { get; set; }

        public LocalizationString NoAnswerText { get; set; }

        public LocalizationString SkippedQuestionText { get; set; }

        public LocalizationString DeepDiveContentHeadline { get; set; }
        public LocalizationString DeepDiveContentButtonText { get; set; }

        public LocalizationString ReanswerQuestionMessageTitle { get; set; }
        public LocalizationString ReanswerQuestionMessageText { get; set; }
        public LocalizationString ReanswerQuestionButtonConfirmText { get; set; }
        public LocalizationString ReanswerQuestionButtonCancelText { get; set; }

        public LocalizationString FirstAnswerQuestionMessageTitle { get; set; }
        public LocalizationString FirstAnswerQuestionMessageText { get; set; }
        public LocalizationString FirstAnswerQuestionButtonConfirmText { get; set; }
        public LocalizationString FirstAnswerQuestionButtonCancelText { get; set; }

        public LocalizationString CompleteSurveyMessageTitle { get; set; }
        public LocalizationString CompleteSurveyMessageText { get; set; }
        public LocalizationString CompleteSurveyMessageButtonConfirmText { get; set; }
        public LocalizationString CompleteSurveyMessageButtonCancelText { get; set; }

        public LocalizationString FinalOverviewHeadline { get; set; }
        public LocalizationString FinalOverviewButtonText { get; set; }

        public LocalizationString DemographyIntroHeadline { get; set; }
        public LocalizationString DemographyIntroText { get; set; }
        public LocalizationString DemographyIntroPrivacyHint { get; set; }
        public LocalizationString DemographyIntroConfirmButtonText { get; set; }
        public LocalizationString DemographyIntroSkipButtonText { get; set; }

        public LocalizationString GoodbyeHeadline { get; set; }
        public LocalizationString GoodbyeText { get; set; }
        public LocalizationString GoodbyeLink { get; set; }
        public LocalizationString GoodbyeButtonText { get; set; }

        public LocalizationString QuestionButtonText { get; set; }

        public LocalizationString LastQuestionButtonText { get; set; }

        public LocalizationString QuestionProgressTextFormat { get; set; }

        public LocalizationString SkipQuestionMessageText { get; set; }

        public LocalizationString SkipQuestionMessageButtonText { get; set; }

        public LocalizationString TimeoutMessageTextFormat { get; set; }

        public LocalizationString TimeoutMessageButtonText { get; set; }

        public LocalizationString WaitForSavingText { get; set; }

        public LocalizationString SaveErrorTitle { get; set; }

        public LocalizationString SaveErrorButtonText { get; set; }

        public List<TouchBeamerSurveyQuestions> TouchBeamerSurveyQuestions { get; set; } = 
            new List<TouchBeamerSurveyQuestions>();

        public Dictionary<string, DeepDiveContentDefinition> DeepDiveContents { get; set; } = 
            new Dictionary<string, DeepDiveContentDefinition>();

        public List<SurveyQuestion> DemographyQuestions { get; set; } = new List<SurveyQuestion>();

        public static DeepDiveStationDefinition FromJson(string json)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Error,
                ContractResolver = LocalizationStringContractResolver.Instance
            };
            return JsonConvert.DeserializeObject<DeepDiveStationDefinition>(json, settings: settings);
        }

        public IEnumerable<SurveyQuestion> GetDeepDiveSurveyQuestions()
        {
            List<SurveyQuestion> result = new List<SurveyQuestion>();
            TouchBeamerSurveyQuestions.ForEach(questions =>
            {
                result.AddRange(
                    questions.SurveyQuestions.Where(question => DeepDiveContents.ContainsKey(question.QuestionID)));
            });
            return result;
        }
    }
}
