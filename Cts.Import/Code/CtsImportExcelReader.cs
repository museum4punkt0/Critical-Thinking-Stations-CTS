using Gemelo.Components.ClosedXml.Code.ExcelReader;
using Gemelo.Components.Common.Base;
using Gemelo.Components.Common.Collections;
using Gemelo.Components.Common.Localization;
using Gemelo.Components.Common.Text;
using Gemelo.Components.Common.Tracing;
using Gemelo.Components.Cts.Code.Data.DeepDive;
using Gemelo.Components.Cts.Code.Data.Layout;
using Gemelo.Components.Cts.Code.Data.Stations;
using Gemelo.Components.Cts.Code.Data.Survey;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Gemelo.Applications.Cts.Import.Code
{
    public class CtsImportExcelReader : SimpleExcelReader
    {
        #region Konstanten

        private const string ConstWorksheetQuesions = "Questions";
        private const string ConstWorksheetTouchBeamer = "TouchBeamer";
        private const string ConstWorksheetDeepDive = "DeepDive";
        private const string ConstWorksheetWall = "Wall";

        private const string ConstDeepDiveWorksheetPrefix = "DD-";

        private const int ConstFirstRowWithData = 6;

        private const int ConstMaxAnswersPerQuestions = 8;

        private const char ConstSeparatorChar = ',';
        private static readonly char[] ConstSeparatorChars = { ConstSeparatorChar };

        #endregion Konstanten

        #region Felder und Eigenschaften

        public Dictionary<string, List<SurveyQuestion>> Questions { get; } =
            new Dictionary<string, List<SurveyQuestion>>();

        public Dictionary<string, TouchBeamerStationDefinition> TouchBeamerStationDefinitions { get; } =
            new Dictionary<string, TouchBeamerStationDefinition>();

        public DeepDiveStationDefinition DeepDiveStationDefinition { get; } = new DeepDiveStationDefinition();

        public WallStationDefinition WallStationDefinition { get; } = new WallStationDefinition();

        #endregion Felder und Eigenschaften

        protected override void DocumentOpened()
        {
            ProcessQuestions();
            ProcessTouchBeamerStations();
            ProcessDeepDiveStation();
            ProcessWallStation();
        }

        private void ProcessQuestions()
        {
            // alle Fragen
            if (ActivateWorksheet(ConstWorksheetQuesions)) ParseRows(ConstFirstRowWithData);
        }

        private void ProcessTouchBeamerStations()
        {
            // TouchBeamer-Stationen anlegen und die Fragen setzen
            Questions.Keys.ForEach(stationID =>
            {
                if (stationID != DeepDiveStationDefinition.DemographyQuestionsID)
                {
                    var stationDefintion = new TouchBeamerStationDefinition
                    {
                        SurveyQuestions = Questions[stationID]
                    };
                    TouchBeamerStationDefinitions.Add(stationID, stationDefintion);
                }
            });
            // TouchBeamer-Stationstexte etc. auslesen
            if (ActivateWorksheet(ConstWorksheetTouchBeamer)) ParseRows(ConstFirstRowWithData);
        }

        private void ProcessDeepDiveStation()
        {
            // DeepDive-Fragen von den TouchBeamern setzen
            TouchBeamerStationDefinitions.Keys.ForEach(stationID =>
            {
                DeepDiveStationDefinition.TouchBeamerSurveyQuestions.Add(new TouchBeamerSurveyQuestions
                {
                    StationTitle = TouchBeamerStationDefinitions[stationID].Title,
                    SurveyQuestions = TouchBeamerStationDefinitions[stationID].SurveyQuestions
                });
            });
            // DeepDive-Fragen für Demographie setzen
            DeepDiveStationDefinition.DemographyQuestions = Questions[DeepDiveStationDefinition.DemographyQuestionsID];

            // DeepDive-Contents auslesen
            foreach (string deepDiveWorksheetName in AvailableWorksheetNames
                .Where(name => name.StartsWith(ConstDeepDiveWorksheetPrefix)))
            {
                if (ActivateWorksheet(deepDiveWorksheetName)) ParseRows(ConstFirstRowWithData);
            }
            // DeepDive-Stationstexte etc. auslesen
            if (ActivateWorksheet(ConstWorksheetDeepDive)) ParseRows(ConstFirstRowWithData);
        }

        private void ProcessWallStation()
        {
            // Wall-Stationstexte etc. auslesen
            if (ActivateWorksheet(ConstWorksheetWall)) ParseRows(ConstFirstRowWithData);
            Questions.Keys.ForEach(stationID => WallStationDefinition.SurveyQuestions.AddRange(Questions[stationID]));
        }

        protected override void RowParsed(int currentRowNumber, string workSheetName, object idTag)
        {
            if (workSheetName.StartsWith(ConstDeepDiveWorksheetPrefix))
            {
                string deepDiveName = workSheetName[ConstDeepDiveWorksheetPrefix.Length..];
                if (!DeepDiveStationDefinition.DeepDiveContents.ContainsKey(deepDiveName))
                {
                    DeepDiveStationDefinition.DeepDiveContents.Add(deepDiveName, new DeepDiveContentDefinition());
                }
                DeepDiveContentRowParsed(DeepDiveStationDefinition.DeepDiveContents[deepDiveName]);
            }
            else
            {
                switch (workSheetName)
                {
                    case ConstWorksheetQuesions:
                        QuestionsRowParsed();
                        break;
                    case ConstWorksheetTouchBeamer:
                        TouchBeamerRowParsed();
                        break;
                    case ConstWorksheetDeepDive:
                        DeepDiveRowParsed();
                        break;
                    case ConstWorksheetWall:
                        WallRowParsed();
                        break;
                }
            }
        }

        private void QuestionsRowParsed()
        {
            string stationIdString = GetStringOfCurrentRow("A");
            if (string.IsNullOrWhiteSpace(stationIdString)) StopCurrentParsingRows();
            else
            {
                if (!Questions.ContainsKey(stationIdString))
                {
                    Questions.Add(stationIdString, new List<SurveyQuestion>());
                }
                Questions[stationIdString].Add(ReadQuestionFromCurrentRow(stationIdString));
            }
        }

        private SurveyQuestion ReadQuestionFromCurrentRow(string stationId)
        {
            string groupID = GetStringOfCurrentRow("C");
            if (groupID.Trim() == string.Empty) groupID = null;
            string typeAsString = GetStringOfCurrentRow("J");
            SurveyQuestionType type = SurveyQuestionType.Default;
            if (typeAsString.IsNotNullOrEmpty() && !Enum.TryParse(typeAsString, out type))
            {
                TraceX.WriteWarning(
                    message: $"Cannot parse '{typeAsString}' as SurveyQuestionType",
                    arguments: $"typeAsString: {typeAsString}",
                    category: nameof(CtsImportExcelReader));
            }
            string maxAnswerCountAsString = GetStringOfCurrentRow("K");
            int maxAnswerCount = int.MaxValue;
            if (maxAnswerCountAsString.IsNotNullOrEmpty() && !int.TryParse(maxAnswerCountAsString, out maxAnswerCount))
            {
                TraceX.WriteWarning(
                    message: $"Cannot parse '{maxAnswerCountAsString}' as MaximumAnswerCount",
                    arguments: $"maxAnswerCountAsString: {maxAnswerCountAsString}",
                    category: nameof(CtsImportExcelReader));
            }

            SurveyQuestion question = new SurveyQuestion
            {
                QuestionID = GetStringOfCurrentRow("B"),
                StationID = stationId,
                GroupID = groupID,
                Title = GetLocalizationStringInCurrentRow("D", "E"),
                QuestionText = GetLocalizationStringInCurrentRow("F", "G"),
                HintText = GetLocalizationStringInCurrentRow("H", "I"),
                QuestionType = type,
                MaximumAnswerCount = maxAnswerCount,
                WallInfo = ReadQuestionWallInfoFromCurrentRowAt('L' - 'A' + 1)
            };
            for (int i = 0; i < ConstMaxAnswersPerQuestions; i++)
            {
                SurveyAnswer answer = ReadAnswerFromCurrentRowAt((char)('V' + i * 4 + 1) - 'A');
                if (answer != null) question.Answers.Add(answer);
            }
            return question;
        }

        private SurveyQuestionWallInfo ReadQuestionWallInfoFromCurrentRowAt(int column)
        {
            string chartTypeString = GetStringOfCurrentRow(column);
            if (chartTypeString.IsNullOrWhiteSpace()) return null;
            List<int> colorIndices = GetStringOfCurrentRow(column + 1)
                .Split(ConstSeparatorChars, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.Parse(s))
                .ToList();
            string allowSingleLineString = GetStringOfCurrentRow(column + 5);
            return new SurveyQuestionWallInfo
            {
                ChartType = Enum.Parse<ChartType>(chartTypeString),
                ColorIndices = colorIndices,
                PageIndex = GetIntOfCurrentRow(column + 2),
                ColumnIndex = GetIntOfCurrentRow(column + 3),
                Position = GetIntOfCurrentRow(column + 4),
                AllowSingleLine = allowSingleLineString.IsNullOrWhiteSpace() ? null : allowSingleLineString != "no",
                ChartTitle = GetLocalizationStringInCurrentRow(column + 6, column + 7),
                GroupQuestion = GetLocalizationStringInCurrentRow(column + 8, column + 9)
            };
        }

        private SurveyAnswer ReadAnswerFromCurrentRowAt(int column)
        {
            string id = GetStringOfCurrentRow(column);
            if (id.IsNullOrEmpty()) return null;
            else
            {
                return new SurveyAnswer
                {
                    AnswerID = id,
                    IsExclusive = GetStringOfCurrentRow(column + 1).IsNotNullOrWhiteSpace(),
                    Text = GetLocalizationStringInCurrentRow(column + 2, column + 3)
                };
            }
        }

        private void TouchBeamerRowParsed()
        {
            string propertyName = GetStringOfCurrentRow("A");
            if (string.IsNullOrWhiteSpace(propertyName)) StopCurrentParsingRows();
            else
            {
                string[] stations = GetStringOfCurrentRow("B")?.Split(ConstSeparatorChar);
                if (stations == null || (stations.Length == 1 && stations[0].IsNullOrWhiteSpace()))
                {
                    stations = TouchBeamerStationDefinitions.Keys.ToArray();
                }
                ReadCurrentTouchBeamerPropertyValuesFor(propertyName, stations);
            }
        }

        private void ReadCurrentTouchBeamerPropertyValuesFor(string propertyName, string[] stations)
        {
            GetPropertyInfoAndValueFor(
                destinationType: typeof(TouchBeamerStationDefinition),
                propertyName: propertyName,
                col1: "C",
                col2: "D",
                propertyInfo: out PropertyInfo propertyInfo,
                value: out object value);
            stations.ForEach(stationID =>
            {
                if (!TouchBeamerStationDefinitions.ContainsKey(stationID))
                {
                    throw new ArgumentOutOfRangeException(stationID);
                }
                propertyInfo.SetValue(TouchBeamerStationDefinitions[stationID], value);
            });
        }

        private void DeepDiveRowParsed()
        {
            string propertyName = GetStringOfCurrentRow("A");
            if (string.IsNullOrWhiteSpace(propertyName)) StopCurrentParsingRows();
            else
            {
                GetPropertyInfoAndValueFor(
                    destinationType: typeof(DeepDiveStationDefinition),
                    propertyName: propertyName,
                    col1: "B",
                    col2: "C",
                    propertyInfo: out PropertyInfo propertyInfo,
                    value: out object value);
                propertyInfo.SetValue(DeepDiveStationDefinition, value);
            }
        }

        private void WallRowParsed()
        {
            string propertyName = GetStringOfCurrentRow("A");
            if (string.IsNullOrWhiteSpace(propertyName)) StopCurrentParsingRows();
            else
            {
                GetPropertyInfoAndValueFor(
                    destinationType: typeof(WallStationDefinition),
                    propertyName: propertyName,
                    col1: "B",
                    col2: "C",
                    propertyInfo: out PropertyInfo propertyInfo,
                    value: out object value);
                propertyInfo.SetValue(WallStationDefinition, value);
            }
        }

        private void GetPropertyInfoAndValueFor(Type destinationType, string propertyName, string col1, string col2,
            out PropertyInfo propertyInfo, out object value)
        {
            propertyInfo = destinationType.GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new ArgumentOutOfRangeException(nameof(propertyInfo),
                    $"Unknown property name: '{propertyInfo.Name}'");
            }
            Type propertyType = propertyInfo.PropertyType;
            if (propertyType == typeof(LocalizationString))
            {
                value = GetLocalizationStringInCurrentRow(col1, col2);
            }
            else if (propertyType == typeof(string))
            {
                value = GetStringOfCurrentRow(col1);
            }
            else if (propertyType == typeof(TimeSpan))
            {
                string valueAsString = GetStringOfCurrentRow(col1);
                if (TimeSpan.TryParse(valueAsString, out TimeSpan timeSpan))
                {
                    value = timeSpan;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(propertyInfo),
                        $"Cannot parse TimeSpan value: '{valueAsString}'");
                }
            }
            else if (propertyType == typeof(RfidHintPosition))
            {
                string valueAsString = GetStringOfCurrentRow(col1);
                if (Enum.TryParse(valueAsString, out RfidHintPosition position))
                {
                    value = position;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(propertyInfo),
                        $"Cannot parse RfidHintPosition value: '{valueAsString}'");
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(propertyInfo),
                    $"Unsupported type of property: '{propertyInfo.Name}'");
            }
        }

        private void DeepDiveContentRowParsed(DeepDiveContentDefinition contentDefinition)
        {
            string elementTypeString = GetStringOfCurrentRow("A");
            if (elementTypeString.IsNullOrWhiteSpace() || GetStringOfCurrentRow("B").IsNullOrEmpty())
            {
                StopCurrentParsingRows();
            }
            else
            {
                DeepDiveContentElement contentElement = new DeepDiveContentElement();
                DeepDiveContentElementType elementType = EnumEx.Parse<DeepDiveContentElementType>(elementTypeString);
                contentElement.ElementType = elementType;
                if (elementType == DeepDiveContentElementType.Media)
                {
                    contentElement.Filename = GetStringOfCurrentRow("B");
                }
                else contentElement.Text = GetLocalizationStringInCurrentRow("B", "C");
                string topMarginString = GetStringOfCurrentRow("D");
                if (topMarginString.IsNotNullOrWhiteSpace())
                {
                    contentElement.TopMargin = int.Parse(topMarginString);
                }
                string widthString = GetStringOfCurrentRow("E");
                if (widthString.IsNotNullOrWhiteSpace())
                {
                    contentElement.Width = int.Parse(widthString);
                }
                string answerFiltersString = GetStringOfCurrentRow("F");
                if (answerFiltersString.IsNotNullOrWhiteSpace())
                {
                    contentElement.AnswerFilters = answerFiltersString
                        .Split(ConstSeparatorChar)
                        .Where(answerID => answerID.IsNotNullOrWhiteSpace())
                        .ToList();
                }
                contentDefinition.ContentElements.Add(contentElement);
            }
        }

        private LocalizationString GetLocalizationStringInCurrentRow(string colDE, string colEN, 
            bool allowEmptyEnglish = true)
        {
            string valueDE = GetStringOfCurrentRow(colDE);
            string valueEN = GetStringOfCurrentRow(colEN);
            return !allowEmptyEnglish && string.IsNullOrWhiteSpace(valueEN) ?
                LocalizationString.Create(valueDE) :
                LocalizationString.Create(de: valueDE, en: valueEN);
        }

        private LocalizationString GetLocalizationStringInCurrentRow(int colDE, int colEN, 
            bool allowEmptyEnglish = true)
        {
            string valueDE = GetStringOfCurrentRow(colDE);
            string valueEN = GetStringOfCurrentRow(colEN);
            return !allowEmptyEnglish && string.IsNullOrWhiteSpace(valueEN) ?
                LocalizationString.Create(valueDE) :
                LocalizationString.Create(de: valueDE, en: valueEN);
        }
    }
}
