using Gemelo.Components.Common.Base;
using Gemelo.Components.Common.Collections;
using Gemelo.Components.Common.Localization;
using Gemelo.Components.Common.Tracing;
using Gemelo.Components.Common.Wpf.Controls.Charts;
using Gemelo.Components.Common.Wpf.Graphics;
using Gemelo.Components.Common.Wpf.UI;
using Gemelo.Components.Common.Wpf.UI.Transitions;
using Gemelo.Components.Common.Wpf.UI.Transitions.Appearance;
using Gemelo.Components.Cts.Code.Data.Survey;
using Gemelo.Components.Dah.Controls.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Gemelo.Applications.Cts.Wall.Controls
{
    /// <summary>
    /// Zeigt die Auswertung für eine Frage an.
    /// </summary>
    public partial class SurveyQuestionChart : UserControl
    {
        #region Konstanten

        private const string ConstPieChartSuffix = "ForPieChart";
        private const string ConstBarChartSuffix = "ForBarChart";
        private const string ConstLocalizedTextTitleStyleNameFormat = "LocalizedTextStyleWallQuestionTitle{0}";
        private const string ConstLocalizedTextQuestionStyleNameFormat = "LocalizedTextStyleWallQuestionText{0}";
        private const string ConstGridColumnAnswerTextStyleNameFormat = "StyleAnswerTextColumn{0}";

        private static readonly TimeSpan ConstTitleAndQuestionFadeInDuration = TimeSpan.FromMilliseconds(100.0);
        private static readonly TimeSpan ConstAnswerCountFadeInDuration = TimeSpan.FromMilliseconds(100.0);

        #endregion Konstanten

        #region Felder und Eigenschaften

        private Style m_StyleAnswerText;
        private Style m_StylePieChartLegendRectangle;

        private BrushCollection m_DefaultChartBrushes;

        private List<SurveyQuestion> m_Questions;

        public List<SurveyQuestion> Questions
        {
            get => m_Questions;
            set => SetQuestions(value);
        }

        public SurveyQuestion FirstQuestion => m_Questions != null && m_Questions.Count > 0 ? m_Questions[0] : null;

        public Grid CurrentChartGrid
        {
            get
            {
                ChartType? chartType = FirstQuestion?.WallInfo?.ChartType;
                if (chartType == ChartType.Pie)
                {
                    return m_Questions.Count > 1 ? m_GridPieChartDouble : m_GridPieChartSingle;
                }
                else if (chartType == ChartType.Bar) return m_GridBarChart;
                return null;
            }
        }

        private bool HasSecondChart => FirstQuestion?.WallInfo?.ChartType == ChartType.Pie && m_Questions.Count > 1;

        private BaseChartControl CurrentChartControl
        {
            get
            {
                ChartType? chartType = FirstQuestion?.WallInfo?.ChartType;
                if (chartType == ChartType.Pie)
                {
                    return m_Questions.Count > 1 ? m_PieChartOuter : m_PieChartSingle;
                }
                else if (chartType == ChartType.Bar) return m_BarChart;
                return null;
            }
        }

        private LocalizedTextBlock CurrentChartTitle
        {
            get
            {
                ChartType? chartType = FirstQuestion?.WallInfo?.ChartType;
                if (chartType == ChartType.Pie)
                {
                    return m_Questions.Count > 1 ? m_LocalizedTextOuterChartTitle : m_LocalizedTextSingleChartTitle;
                }
                else if (chartType == ChartType.Bar) return m_LocalizedTextBarChartTitle;
                return null;
            }
        }

        public bool IsActiveForSettings
        {
            get => Background == null;
            set => Background = value ? Brushes.Red.WithOpacity(0.3) : null;
        }

        #endregion Felder und Eigenschaften

        #region Konstruktor und Initialisierung

        public SurveyQuestionChart()
        {
            InitializeComponent();
            InitializeStyles();
            if (App.IsAvailable)
            {
                BindApplicationEvents();
                UpdateTexts();
            }
        }

        private void InitializeStyles()
        {
            m_StyleAnswerText = (Style)FindResource("StyleAnswerText");
            m_StylePieChartLegendRectangle = (Style)FindResource("StylePieChartLegendRectangle");
            m_DefaultChartBrushes = (BrushCollection)FindResource("ChartBrushes");
        }

        private void BindApplicationEvents()
        {
            App.Current.StationDefinitionChanged += App_StationDefinitionChanged;
        }

        #endregion Konstruktor und Initialisierung

        #region Öffentliche Methoden

        public void UpdateResultsFrom(SurveyResultSet resultSet)
        {
            int total = UpdateChartForAndReturnTotal(
                chartControl: CurrentChartControl,
                question: FirstQuestion,
                resultSet: resultSet);
            int? secondTotal = null;
            if (HasSecondChart)
            {
                secondTotal = UpdateChartForAndReturnTotal(chartControl: m_PieChartInner,
                    question: Questions[1],
                    resultSet: resultSet);
            }
            UpdateAnswerCount(total, secondTotal);
        }

        public void ShowUp(TimeSpan delay, TimeSpan duration)
        {
            m_LocalizedTextTitle.StopTransitionAndHide();
            m_LocalizedTextTitle.DelayedFadeIn(delay: delay, duration: ConstTitleAndQuestionFadeInDuration);
            m_LocalizedTextQuestion.StopTransitionAndHide();
            m_LocalizedTextQuestion.DelayedFadeIn(delay: delay, duration: ConstTitleAndQuestionFadeInDuration);
            m_LocalizedTextOuterChartTitle.StopTransitionAndHide();
            m_LocalizedTextOuterChartTitle.DelayedFadeIn(delay: delay, duration: ConstTitleAndQuestionFadeInDuration);
            m_LocalizedTextInnerChartTitle.StopTransitionAndHide();
            m_LocalizedTextInnerChartTitle.DelayedFadeIn(delay: delay + duration * 0.5,
                duration: ConstTitleAndQuestionFadeInDuration);
            foreach (UIElement uIElement in m_GridAnswers.Children)
            {
                uIElement.Opacity = 0.0;
            }
            CurrentChartControl.ShowUp(duration: duration, delay: delay);
            if (HasSecondChart)
            {
                m_PieChartInner.ShowUp(duration: duration, delay: delay + duration * 0.5);
            }
            m_StackAnswerCount.StopTransitionAndHide();
            m_StackAnswerCount.DelayedFadeIn(delay: delay + duration, duration: ConstAnswerCountFadeInDuration);
        }

        #endregion Öffentliche Methoden

        #region Private Methoden

        private void SetQuestions(List<SurveyQuestion> questions)
        {
            if (questions == null || questions.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(questions),
                    "Questions must be set and must contain at least one question!");
            }
            m_Questions = questions;
            m_LocalizedTextTitle.Text = FirstQuestion.Title.ToUpperInvariant();
            LocalizationString groupQuestion = FirstQuestion.WallInfo?.GroupQuestion;
            m_LocalizedTextQuestion.Text = LocalizationString.IsNullOrEmpty(groupQuestion) ?
                FirstQuestion.QuestionText.RemoveLineBreaks() : groupQuestion;
            bool allowSingleLine = FirstQuestion.WallInfo?.AllowSingleLine ??
                FirstQuestion.WallInfo?.ChartType == ChartType.Pie;
            UpdateEntryFills();
            SetAnswers(FirstQuestion.Answers, allowSingleLine: allowSingleLine);
            UpdateStyles();
            Grid currentChartGrid = CurrentChartGrid;
            foreach (Grid chartGrid in m_GridCharts.Children)
            {
                chartGrid.Visibility = (chartGrid == currentChartGrid).ToVisibility();
            }
            if (CurrentChartTitle != null) CurrentChartTitle.Text = FirstQuestion.WallInfo.ChartTitle;
            if (HasSecondChart) m_LocalizedTextInnerChartTitle.Text = questions[1].WallInfo.ChartTitle;
        }

        private void UpdateEntryFills()
        {
            UpdateEntryFillsFor(CurrentChartControl, FirstQuestion.WallInfo.ColorIndices);
            if (HasSecondChart)
            {
                UpdateEntryFillsFor(m_PieChartInner, m_Questions[1].WallInfo.ColorIndices);
            }
        }

        private void UpdateEntryFillsFor(BaseChartControl chartControl, List<int> colorIndices)
        {
            if (!colorIndices.IsNullOrEmpty())
            {
                BrushCollection entryFills = new BrushCollection();
                foreach (int colorIndex in colorIndices)
                {
                    entryFills.Add(m_DefaultChartBrushes[colorIndex % m_DefaultChartBrushes.Count]);
                }
                chartControl.EntryFills = entryFills;
            }
            else chartControl.EntryFills = m_DefaultChartBrushes;
        }

        private void UpdateStyles()
        {
            bool isPieChart = FirstQuestion?.WallInfo?.ChartType == ChartType.Pie;
            string styleSuffix = isPieChart ? ConstPieChartSuffix : ConstBarChartSuffix;
            m_LocalizedTextTitle.Style =
                (Style)FindResource(string.Format(ConstLocalizedTextTitleStyleNameFormat, styleSuffix));
            m_LocalizedTextQuestion.Style =
                (Style)FindResource(string.Format(ConstLocalizedTextQuestionStyleNameFormat, styleSuffix));
            m_GridColumnAnswerText.Style =
                (Style)FindResource(string.Format(ConstGridColumnAnswerTextStyleNameFormat, styleSuffix));
        }

        private void SetAnswers(List<SurveyAnswer> answers, bool allowSingleLine)
        {
            m_GridAnswers.Children.Clear();
            m_GridAnswers.RowDefinitions.Clear();
            for (int answerIndex = 0; answerIndex < answers.Count; answerIndex++)
            {
                m_GridAnswers.RowDefinitions.Add(new RowDefinition());
                SurveyAnswer answer = answers[answerIndex];
                LocalizedTextBlock answerTextBlock = new LocalizedTextBlock
                {
                    Text = answer.Text.RemoveLineBreaks(),
                    AllowSingleLine = allowSingleLine,
                    Style = m_StyleAnswerText,
                    Tag = answer.AnswerID
                };
                Grid.SetRow(answerTextBlock, answerIndex);
                m_GridAnswers.Children.Add(answerTextBlock);
                if (FirstQuestion.WallInfo?.ChartType == ChartType.Pie)
                {
                    Rectangle answerLegend = new Rectangle
                    {
                        Fill = CurrentChartControl.GetFillForAnswerIndex(answerIndex),
                        Style = m_StylePieChartLegendRectangle,
                        Tag = answer.AnswerID
                    };
                    m_GridAnswers.Children.Add(answerLegend);
                    Grid.SetRow(answerLegend, answerIndex);
                }
            }
        }

        private void UpdateBarPositions()
        {
            if (FirstQuestion?.WallInfo?.ChartType == ChartType.Bar)
            {
                ObservableCollection<double> centerPositions = new ObservableCollection<double>();
                foreach (LocalizedTextBlock localizedText in m_GridAnswers.Children
                    .Cast<UIElement>()
                    .Where(element => element is LocalizedTextBlock)
                    .Cast<LocalizedTextBlock>())
                {
                    double height = localizedText.ActualHeight;
                    double centerPosition = localizedText.TranslatePoint(new Point(0, height * 0.5), m_GridMain).Y;
                    centerPositions.Add(centerPosition);
                }
                m_BarChart.BarPositions = centerPositions;
            }
        }

        private void UpdateAnswerCount(int total, int? secondTotal)
        {
            m_TxtAnswerCount.Text = secondTotal.HasValue ? $"{total} / {secondTotal.Value}" : total.ToString();
        }

        private int UpdateChartForAndReturnTotal(BaseChartControl chartControl, SurveyQuestion question,
            SurveyResultSet resultSet)
        {
            var questionResults = resultSet.GetCompleteResultForQuestion(question, out int total);
            ChartEntryCollection chartEntries = new ChartEntryCollection();
            questionResults.Keys
                .Select(answerID => new ChartEntry()
                {
                    Value = total > 0.0 ? Math.Round(questionResults[answerID] * 100.0 / (double)total) : 0.0,
                    Tag = answerID
                })
                .ForEach(entry => chartEntries.Add(entry));
            chartControl.Entries = chartEntries;
            return total;
        }

        private void UpdateTexts()
        {
            m_LocalizedTextAnswerCountLabel.Text = App.Current.StationDefinition.AnswersCountText;
        }

        #endregion Private Methoden

        #region Ereignishandler

        private void App_StationDefinitionChanged(object sender, EventArgs e)
        {
            UpdateTexts();
        }

        private void Chart_EntryProgressChanged(object sender, ChartEntryProgressEventArgs e)
        {
            m_GridAnswers.Children
                .Cast<FrameworkElement>()
                .Where(element => element.Tag == e.Entry.Tag)
                .ForEach(element =>
            {
                element.Opacity = MathEx.MinMax(e.Progress * 10.0, 0.0, 1.0);
            });
        }

        private void StackTexts_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (m_StackTexts.HasActualSize()) UpdateBarPositions();
        }

        #endregion Ereignishandler
    }
}
