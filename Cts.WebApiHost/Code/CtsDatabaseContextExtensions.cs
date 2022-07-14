using Gemelo.Components.Common.Collections;
using Gemelo.Components.Common.Tracing;
using Gemelo.Components.Cts.Code.Data.Survey;
using Gemelo.Components.Cts.Code.Data.Users;
using Gemelo.Components.Cts.Database.Databases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gemelo.Components.Cts.WebApiHost.Code
{
    public static class CtsDatabaseContextExtensions
    {
        public static async Task<SurveyResultSet> GetSurveyResultSet(this CtsDatabaseContext context, 
            List<string> questionIDs)
        {
            try
            {
                SurveyResultSet resultSet = new SurveyResultSet();
                await Task.Run(() =>
                {
                    IEnumerable<SurveyQuestion> questions = context.SurveyQuestionVersions
                        .Where(qv => qv.IsActive)
                        .ToList()
                        .Where(qv => questionIDs.Contains(qv.QuestionID))
                        .Select(qv => SurveyQuestion.FromJsonOrNull(qv.DetailsAsJson))
                        .Where(q => q != null);
                    IEnumerable<Dictionary<string, List<CtsUserAnswerInformation>>> userAnswersList = context.CtsUsers
                        .Where(user => user.IsActive)
                        .ToList()
                        .Select(user => user.GetAnswersForQuestionIDs())
                        .Where(answers => answers != null);
                    userAnswersList.ForEach(userAnswers => resultSet.AddUserAnswersForQuestions(userAnswers, questions));
                });
                return resultSet;
            }
            catch (Exception exception)
            {
                TraceX.WriteHandledException(
                    message: $"Handled exception while trying to fetch result set: {exception.Message}",
                    category: nameof(CtsDatabaseContextExtensions),
                    exception: exception);
                return null;
            }
        }
    }
}
