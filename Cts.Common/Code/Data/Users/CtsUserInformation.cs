using Gemelo.Components.Cts.Code.Data.Survey;
using Gemelo.Components.Cts.Common.Code.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gemelo.Components.Cts.Code.Data.Users
{
    public class CtsUserInformation
    {
        public int CtsUserID { get; set; }

        public string Rfid { get; set; }

        public string Language { get; set; }

        public CtsUserState State { get; set; } = CtsUserState.None;

        public Dictionary<string, List<CtsUserAnswerInformation>> AnswersForQuestionIDs { get; set; } =
            new Dictionary<string, List<CtsUserAnswerInformation>>();

        public bool IsNew => AnswersForQuestionIDs == null || AnswersForQuestionIDs.Count == 0;

        public List<DeepDiveVisit> DeepDiveVisits { get; set; } = new List<DeepDiveVisit>();

        public static bool TryFromJson(string json, out CtsUserInformation result)
        {
            try
            {
                result = FromJson(json);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public static CtsUserInformation FromJson(string json)
        {
            CtsUserInformation result;
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = LocalizationStringContractResolver.Instance
            };
            result = JsonConvert.DeserializeObject<CtsUserInformation>(json, settings: settings);
            return result;
        }

        public string ToJson()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = LocalizationStringContractResolver.Instance
            };
            return JsonConvert.SerializeObject(value: this, formatting: Formatting.None, settings: settings);
        }

        public CtsUserAnswerInformation GetLatestAnswersFor(string questionID)
        {
            if (AnswersForQuestionIDs.ContainsKey(questionID))
            {
                return AnswersForQuestionIDs[questionID]
                    .OrderByDescending(answers => answers.Timestamp)
                    .FirstOrDefault();
            }
            else return null;
        }

        public SurveyAnswerType GetAnswerTypeFor(string questionID)
        {
            if (AnswersForQuestionIDs.ContainsKey(questionID))
            {
                return AnswersForQuestionIDs[questionID]
                    .OrderByDescending(answers => answers.Timestamp)
                    .FirstOrDefault().AnswerIDs.Count > 0 ? SurveyAnswerType.Answer : SurveyAnswerType.Skipped;
            }
            else return SurveyAnswerType.NoAnswer;
        }

        public List<SurveyQuestion> GetUnansweredQuestionsFor(List<SurveyQuestion> surveyQuestions)
        {
            List<SurveyQuestion> result = new List<SurveyQuestion>(surveyQuestions.Count);
            result.AddRange(surveyQuestions);
            foreach (string questionID in AnswersForQuestionIDs.Keys)
            {
                SurveyQuestion question = result.FirstOrDefault(q => q.QuestionID == questionID);
                if (question != null && GetLatestAnswersFor(questionID).AnswerIDs.Count > 0)
                {
                    result.Remove(question);
                }
            }
            return result;
        }

        public bool HasDeepDiveBeenVisited(string questionID)
        {
            return DeepDiveVisits.Exists(visit => visit.QuestionID == questionID );
        }
    }
}
