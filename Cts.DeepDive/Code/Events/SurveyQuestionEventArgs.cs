using Gemelo.Components.Cts.Code.Data.Survey;
using System;

namespace Gemelo.Applications.Cts.DeepDive.Code.Events
{
    public class SurveyQuestionEventArgs : EventArgs
    {
        public SurveyQuestion Question { get; set; }

        public SurveyQuestionEventArgs(SurveyQuestion question)
        {
            Question = question;
        }
    }
}
