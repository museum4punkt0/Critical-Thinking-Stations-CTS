using Gemelo.Components.Common.Localization;
using System.Collections.Generic;

namespace Gemelo.Components.Cts.Code.Data.Survey
{
    public class SurveyQuestionWallInfo
    {
        public ChartType ChartType { get; set; } = ChartType.None;

        public List<int> ColorIndices { get; set; }

        public int PageIndex { get; set; }

        public int ColumnIndex { get; set; }

        public int Position { get; set; }

        public bool? AllowSingleLine { get; set; }

        public LocalizationString ChartTitle { get; set; }

        public LocalizationString GroupQuestion { get; set; }
    }

    public enum ChartType
    {
        None,
        Pie,
        Bar
    }
}
