using Gemelo.Components.Common.Localization;
using System;
using System.Collections.Generic;

namespace Gemelo.Components.Cts.Code.Data.DeepDive
{
    public class DeepDiveContentElement
    {
        public DeepDiveContentElementType ElementType { get; set; } = DeepDiveContentElementType.Paragraph;

        public LocalizationString Text { get; set; }

        public string Filename { get; set; }

        public int TopMargin { get; set; } = int.MinValue;

        public int Width { get; set; } = int.MinValue;

        public List<string> AnswerFilters = null;
    }

    public enum DeepDiveContentElementType
    {
        Headline,
        SubHeadline,
        ParagraphHeadline,
        Paragraph,
        Media
    }
}
