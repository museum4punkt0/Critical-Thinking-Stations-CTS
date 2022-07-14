using System;

namespace Gemelo.Components.Cts.Database.Models
{
    public class SurveyQuestionVersion
    {
        public int SurveyQuestionVersionID { get; set; }

        public string QuestionID { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateTime { get; set; }

        public string DetailsAsJson { get; set; }
    }
}
