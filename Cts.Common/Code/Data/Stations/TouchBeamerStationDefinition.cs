using Gemelo.Components.Common.Localization;
using Gemelo.Components.Cts.Code.Data.Layout;
using Gemelo.Components.Cts.Code.Data.Survey;
using Gemelo.Components.Cts.Common.Code.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Gemelo.Components.Cts.Code.Data.Stations
{
    public class TouchBeamerStationDefinition : StationDefinitionBase
    {
        #region Konstanten
        
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

        public LocalizationString WelcomeFirstHeadline { get; set; }

        public LocalizationString WelcomeFirstText { get; set; }

        public LocalizationString WelcomeFirstButtonText { get; set; }

        public LocalizationString WelcomeAgainHeadline { get; set; }

        public LocalizationString WelcomeAgainText { get; set; }

        public LocalizationString WelcomeAgainButtonText { get; set; }

        public LocalizationString WelcomeBackUnansweredQuestionsHeadline { get; set; }

        public LocalizationString WelcomeBackUnansweredQuestionsText { get; set; }

        public LocalizationString WelcomeBackUnansweredQuestionsButtonText { get; set; }

        public LocalizationString ReturnAlreadyCompletedHeadline { get; set; }

        public LocalizationString ReturnAlreadyCompletedText { get; set; }

        public LocalizationString ReturnAlreadyCompletedButtonText { get; set; }

        public LocalizationString PrivacyHeadline { get; set; }

        public LocalizationString PrivacyText { get; set; }

        public LocalizationString PrivacyConfirmationText { get; set; }

        public LocalizationString PrivacyButtonText { get; set; }

        public LocalizationString GoodbyeHeadline { get; set; }

        public LocalizationString GoodbyeText { get; set; }

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

        public List<SurveyQuestion> SurveyQuestions { get; set; }

        public static TouchBeamerStationDefinition FromJson(string json)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Error,
                ContractResolver = LocalizationStringContractResolver.Instance
            };
            return JsonConvert.DeserializeObject<TouchBeamerStationDefinition>(json, settings: settings);
        }
    }
}
