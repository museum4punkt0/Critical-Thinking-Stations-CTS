using Gemelo.Components.Cts.Code.Data.Users;
using Gemelo.Components.Cts.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gemelo.Components.Cts.WebApiHost.Code
{
    public static class CtsUserExtensions
    {
        public static CtsUserInformation GetDetails(this CtsUser user)
        {
            if (CtsUserInformation.TryFromJson(user.DetailsAsJson, out CtsUserInformation result)) return result;
            else return null;
        }

        public static Dictionary<string, List<CtsUserAnswerInformation>> GetAnswersForQuestionIDs(this CtsUser user)
        {
            return GetDetails(user)?.AnswersForQuestionIDs;
        }
    }
}
