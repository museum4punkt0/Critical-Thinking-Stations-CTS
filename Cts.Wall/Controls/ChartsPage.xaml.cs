using Gemelo.Components.Common.Collections;
using Gemelo.Components.Common.Exhibits.Controls;
using Gemelo.Components.Common.Wpf.Controls;
using Gemelo.Components.Common.Wpf.UI.Transitions;
using Gemelo.Components.Common.Wpf.UI.Transitions.Appearance;
using Gemelo.Components.Cts.Code.Data.Survey;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Gemelo.Applications.Cts.Wall.Controls
{
    /// <summary>
    /// Zeigt eine Seite mit Auswertungen an.
    /// </summary>
    public partial class ChartsPage : UserControl, IAppearanceControl
    {
        #region Konstanten

        private static readonly TimeSpan ConstFadeOutDuration = TimeSpan.FromMilliseconds(200.0);
        private static readonly TimeSpan ConstFadeInDelay = TimeSpan.FromMilliseconds(250.0);
        private static readonly TimeSpan ConstFadeInDuration = TimeSpan.FromMilliseconds(150.0);

        #endregion Konstanten

        private readonly List<StackPanel> m_ColumnStacks;

        private IEnumerable<List<SurveyQuestion>> m_QuestionGroups;
        private SurveyResultSet m_ResultSet;

        public ChartsPage()
        {
            InitializeComponent();
            m_ColumnStacks = m_GridMain.Children.Cast<StackPanel>().ToList();
        }

        public void ShowUpQuestions(IEnumerable<List<SurveyQuestion>> questionGroups, SurveyResultSet resultSet)
        {
            m_QuestionGroups = questionGroups;
            m_ResultSet = resultSet;
            UpdateChartControls();
            StartShowUp();
        }

        public SurveyQuestionChart GetChart(int index)
        {
            int stackIndex = 0;
            while (stackIndex < m_ColumnStacks.Count && index >= m_ColumnStacks[stackIndex].Children.Count)
            {
                index -= m_ColumnStacks[stackIndex].Children.Count;
                stackIndex++;
            }
            if (stackIndex < m_ColumnStacks.Count)
            {
                return (SurveyQuestionChart)m_ColumnStacks[stackIndex].Children[index];
            }
            else return null;
        }

        public void ResetAllActivatedForSettingsFlags()
        {
            m_ColumnStacks.ForEach(stack =>
                stack.Children.Cast<SurveyQuestionChart>().ForEach(chart => chart.IsActiveForSettings = false));
        }

        void IAppearanceControl.Hide(TimeSpan? duration, TimeSpan? delay)
        {
            this.StopTransition();
            this.FadeOutIfVisible(ConstFadeOutDuration);
        }

        void IAppearanceControl.ShowUp(TimeSpan? duration, TimeSpan? delay)
        {
            this.StopTransition();
            delay ??= TimeSpan.Zero;
            this.DelayedFadeIn(delay: delay.Value + ConstFadeInDelay, duration: ConstFadeInDuration);
        }

        private void StartShowUp()
        {
            TimeSpan duration = App.Current.StationDefinition.QuestionShowUpDuration;
            TimeSpan interval = App.Current.StationDefinition.QuestionShowUpInterval;
            TimeSpan delay = ConstFadeInDelay + ConstFadeInDuration;

            foreach (StackPanel stack in m_ColumnStacks)
            {
                foreach (SurveyQuestionChart chart in stack.Children)
                {
                    chart.ShowUp(delay, duration);
                    delay += interval;
                }
            }
        }

        private void UpdateChartControls()
        {
            for (int column = 0; column < m_ColumnStacks.Count; column++)
            {
                UpdateChartControlsFor(m_ColumnStacks[column],
                    m_QuestionGroups.Where(questionGroup => questionGroup != null &&
                    questionGroup.Count > 0 && questionGroup[0].WallInfo?.ColumnIndex == column));
            }
        }

        private void UpdateChartControlsFor(StackPanel stack, IEnumerable<List<SurveyQuestion>> questionGroups)
        {
            int chartIndex = 0;
            foreach (List<SurveyQuestion> questionGroup in questionGroups.OrderBy(qg => qg[0].WallInfo.Position))
            {
                SurveyQuestionChart chart = stack.Children.Count > chartIndex ?
                    (SurveyQuestionChart)stack.Children[chartIndex] :
                    CreateNewSurveyChartFor(stack);
                chart.Questions = questionGroup;
                chart.UpdateResultsFrom(m_ResultSet);
                UpdateTopMarginFromDisplaySettings(chart);
                chartIndex++;
            }
            while (stack.Children.Count > chartIndex) stack.Children.RemoveAt(chartIndex);
        }

        private static void UpdateTopMarginFromDisplaySettings(SurveyQuestionChart chart)
        {
            string questionID = chart.FirstQuestion.QuestionID;
            if (App.Current.DisplaySettings.ChartSettings.ContainsKey(questionID))
            {
                chart.Margin = new Thickness(
                    left: 0.0,
                    top: App.Current.DisplaySettings.ChartSettings[questionID].TopMartin,
                    right: 0.0,
                    bottom: 0.0);
            }
        }

        private SurveyQuestionChart CreateNewSurveyChartFor(StackPanel stack)
        {
            SurveyQuestionChart chart = new SurveyQuestionChart();
            stack.Children.Add(chart);
            return chart;
        }
    }
}
