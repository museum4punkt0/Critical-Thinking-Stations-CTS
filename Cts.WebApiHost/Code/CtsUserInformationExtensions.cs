using Gemelo.Components.Cts.Code.Communication;
using Gemelo.Components.Cts.Code.Data.Users;
using System;
using System.Collections.Generic;

namespace Gemelo.Components.Cts.WebApiHost.Code
{
    public static class CtsUserInformationExtensions
    {
        public static void ProcessSaveAnswersRequest(this CtsUserInformation userInformation, 
            SaveAnswersRequest request)
        {
            if (!userInformation.AnswersForQuestionIDs.ContainsKey(request.QuestionID))
            {
                userInformation.AnswersForQuestionIDs.Add(request.QuestionID, new List<CtsUserAnswerInformation>());
            }
            List<CtsUserAnswerInformation> answerInformations =
                userInformation.AnswersForQuestionIDs[request.QuestionID];
            answerInformations.Add(new CtsUserAnswerInformation
            {
                QuestionID = request.QuestionID,
                StationID = request.StationID,
                Timestamp = DateTime.Now,
                AnswerIDs = request.AnswerIDs
            });
        }

        public static void ProcessSaveDeepDiveVisitRequest(this CtsUserInformation userInformation, 
            SaveDeepDiveVisitRequest request)
        {
            DeepDiveVisit visit = new DeepDiveVisit
            {
                QuestionID = request.QuestionID,
                Timestamp = DateTime.Now
            };
            userInformation.DeepDiveVisits.Add(visit);
        }

        public static void ProcessChangeUserState(this CtsUserInformation userInformation,
            ChangeUserStateRequest request)
        {
            userInformation.State |= request.AddStates;
            userInformation.State &= ~request.RemoveStates;
        }
    }
}
