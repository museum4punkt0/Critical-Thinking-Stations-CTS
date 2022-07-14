using Gemelo.Components.Common.Localization;
using Gemelo.Components.Cts.Common.Code.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Gemelo.Components.Cts.Code.Data.Survey
{
    public class SurveyQuestion
    {
        public string QuestionID { get; set; }

        public string StationID { get; set; }

        public string GroupID { get; set; }

        public LocalizationString Title { get; set; }

        public LocalizationString QuestionText { get; set; }

        public LocalizationString HintText { get; set; }

        public SurveyQuestionType QuestionType { get; set; } = SurveyQuestionType.Default;

        public List<SurveyAnswer> Answers { get; set; } = new List<SurveyAnswer>();

        public int MaximumAnswerCount { get; set; } = int.MaxValue;

        public SurveyQuestionWallInfo WallInfo { get; set; }

        public static bool TryFromJson(string json, out SurveyQuestion result)
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

        public static SurveyQuestion FromJson(string json)
        {
            SurveyQuestion result;
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = LocalizationStringContractResolver.Instance
            };
            result = JsonConvert.DeserializeObject<SurveyQuestion>(json, settings: settings);
            return result;
        }

        public static SurveyQuestion FromJsonOrNull(string json)
        {
            try
            {
                return FromJson(json);
            }
            catch
            {
                return null;
            }
        }

        public string ToJson()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = LocalizationStringContractResolver.Instance
            };
            return JsonConvert.SerializeObject(value: this, formatting: Formatting.None, settings: settings);
        }

        public string MapAnswerID(string answerID, DateTime answerTimeStamp)
        {
            if (QuestionType == SurveyQuestionType.Default) return answerID;
            else if (QuestionType == SurveyQuestionType.YearOfBirth)
            {
                if (int.TryParse(answerID, out int yearOfBirth))
                {
                    int age = answerTimeStamp.Year - yearOfBirth;
                    return MapAgeToAgeIntervalAnswers(age);
                }
                else return null;
            }
            else return null;
        }

        private string MapAgeToAgeIntervalAnswers(int age)
        {
            foreach (SurveyAnswer answer in Answers)
            {
                if (TryParseAgeIntervalFromAnswerID(answer.AnswerID, out int from, out int to) && 
                    age >= from && age <= to)
                {
                    return answer.AnswerID;
                }
            }
            return null;
        }

        private bool TryParseAgeIntervalFromAnswerID(string answerID, out int from, out int to)
        {
            from = 0;
            to = int.MaxValue;
            if (answerID.Contains('-'))
            {
                string[] parts = answerID.Split('-');
                if (parts.Length != 2) return false;
                parts[0] = parts[0].Trim();
                if (parts[0] != string.Empty && !int.TryParse(parts[0], out from)) return false;
                if (parts[1] != string.Empty && !int.TryParse(parts[1], out to)) return false;
                return true;
            }
            else if (int.TryParse(answerID.Trim(), out from))
            {
                to = from;
                return true;
            }
            else return false;
        }
    }
}
