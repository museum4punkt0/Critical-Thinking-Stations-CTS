using System.Collections.Generic;

namespace Gemelo.Components.Cts.Code.Communication
{
    public class SaveAnswersRequest
    {
        public int CtsUserID { get; set; }

        public string StationID { get; set; }

        public string QuestionID { get; set; }

        public List<string> AnswerIDs { get; set; }
    }
}
