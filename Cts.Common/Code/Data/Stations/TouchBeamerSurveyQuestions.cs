using Gemelo.Components.Common.Localization;
using Gemelo.Components.Cts.Code.Data.Survey;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gemelo.Components.Cts.Code.Data.Stations
{
    public class TouchBeamerSurveyQuestions
    {
        public LocalizationString StationTitle { get; set; }

        public List<SurveyQuestion> SurveyQuestions { get; set; }
    }
}
