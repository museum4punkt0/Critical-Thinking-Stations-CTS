using System;
using System.Collections.Generic;

namespace Gemelo.Components.Cts.Code.Data.Users
{
    public class CtsUserAnswerInformation
    {
        public string QuestionID { get; set; }

        public string StationID { get; set; }

        public DateTime Timestamp { get; set; }

        public List<string> AnswerIDs { get; set; } = new List<string>();
    }
}
