using Gemelo.Components.Common.Collections;
using Gemelo.Components.Common.Settings;
using Gemelo.Components.Cts.Code.Data.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gemelo.Components.Cts.Code.Data.Survey
{
    public class SurveyResultSet
    {
        public Dictionary<string, Dictionary<string, int>> AnswersPerQuestion { get; set; } = new();

        public Dictionary<string, int> UsersWithAnswerPerQuestion { get; set; } = new();

        public static SurveyResultSet CreateDemoResultSetFor(List<SurveyQuestion> surveyQuestions)
        {
            SurveyResultSet resultSet = new SurveyResultSet();
            foreach (SurveyQuestion question in surveyQuestions)
            {
                string questionID = question.QuestionID;
                resultSet.AnswersPerQuestion.Add(questionID,
                    question.Answers.ToDictionary(q => q.AnswerID, q => RandomEx.GetInteger(400)));
                resultSet.UsersWithAnswerPerQuestion.Add(questionID,
                    resultSet.AnswersPerQuestion[questionID].Select(keyValue => keyValue.Value).Sum());
            }
            return resultSet;
        }

        public Dictionary<string, int> GetCompleteResultForQuestion(SurveyQuestion question, out int userCount)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            if (AnswersPerQuestion.ContainsKey(question.QuestionID))
            {
                Dictionary<string, int> answersInResultSet = AnswersPerQuestion[question.QuestionID];
                foreach (SurveyAnswer answer in question.Answers)
                {
                    int answerCount = answersInResultSet.ContainsKey(answer.AnswerID) ?
                        answersInResultSet[answer.AnswerID] : 0;
                    result.Add(answer.AnswerID, answerCount);
                }
                userCount = UsersWithAnswerPerQuestion.ContainsKey(question.QuestionID) ?
                    UsersWithAnswerPerQuestion[question.QuestionID] : 0;
            }
            else
            {
                question.Answers.ForEach(answer => result.Add(answer.AnswerID, 0));
                userCount = 0;
            }
            return result;
        }

        public void AddUserAnswersForQuestions(Dictionary<string, List<CtsUserAnswerInformation>> userAnswers,
            IEnumerable<SurveyQuestion> questionIDs)
        {
            questionIDs.Where(question => userAnswers.ContainsKey(question.QuestionID)).ForEach(question =>
            {
                string questionID = question.QuestionID;
                CtsUserAnswerInformation answerInformation = userAnswers[questionID]
                    .OrderByDescending(answers => answers.Timestamp)
                    .FirstOrDefault();

                if (!AnswersPerQuestion.ContainsKey(questionID))
                {
                    AnswersPerQuestion.Add(questionID, new Dictionary<string, int>());
                }
                Dictionary<string, int> answersPerQuestionDictionary = AnswersPerQuestion[questionID];
                answerInformation.AnswerIDs.ForEach(answerID =>
                {
                    if (question.QuestionType != SurveyQuestionType.Default)
                    {
                        answerID = question.MapAnswerID(answerID, answerInformation.Timestamp);
                    }
                    if (answerID != null)
                    {
                        if (!answersPerQuestionDictionary.ContainsKey(answerID))
                        {
                            answersPerQuestionDictionary.Add(answerID, 1);
                        }
                        else answersPerQuestionDictionary[answerID]++;
                    }
                });
                if (answerInformation.AnswerIDs.Count > 0)
                {
                    if (!UsersWithAnswerPerQuestion.ContainsKey(questionID))
                    {
                        UsersWithAnswerPerQuestion.Add(questionID, 1);
                    }
                    else UsersWithAnswerPerQuestion[questionID]++;
                }
            });
        }
    }
}
