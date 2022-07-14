using Gemelo.Applications.Cts.DeepDive.Controls.Common;
using Gemelo.Components.Common.Localization;
using Gemelo.Components.Common.Wpf.Localization;
using Gemelo.Components.Cts.Code.Data.Stations;
using Gemelo.Components.Cts.Code.Data.Survey;
using Gemelo.Components.Cts.Code.Data.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Gemelo.Applications.Cts.DeepDive.Controls
{
    /// <summary>
    /// Zeigt die finale Übersicht über alle Fragen und Antworten an.
    /// </summary>
    public partial class FinalOverviewScreen : UserControl
    {
        #region Konstanten

        private readonly static int[] ConstFourColumnQuestionCounts = { 4, 7, 8 };

        #endregion Konstanten

        #region Felder und Eigenschaften

        private Style m_TextStyleStationHeadline;
        private Style m_StyleGridQuestionPreviewsThreeColumns;
        private Style m_StyleGridQuestionPreviewsFourColumns;
        private Style m_StyleQuestionPreviewForThreeColumns;
        private Style m_StyleQuestionPreviewForFourColumns;

        private Dictionary<string, QuestionPreview> m_QuestionPreviews = new Dictionary<string, QuestionPreview>();

        #endregion Felder und Eigenschaften

        #region Ereignisse

        public event EventHandler NextClick;

        protected void OnNextClick()
        {
            NextClick?.Invoke(this, null);
        }

        public event EventHandler UserInteraction;

        protected void OnUserInteraction()
        {
            UserInteraction?.Invoke(this, null);
        }

        #endregion Ereignisse

        #region Konstruktor und Initialisierung

        public FinalOverviewScreen()
        {
            InitializeComponent();
            InitializeStyles();
            if (App.IsAvailable) App.Current.StationDefinitionChanged += App_StationDefinitionChanged;
        }

        private void InitializeStyles()
        {
            m_TextStyleStationHeadline = (Style)FindResource("TextStyleStationHeadline");
            m_StyleGridQuestionPreviewsThreeColumns = (Style)FindResource("StyleGridQuestionPreviewsThreeColumns");
            m_StyleGridQuestionPreviewsFourColumns = (Style)FindResource("StyleGridQuestionPreviewsFourColumns");
            m_StyleQuestionPreviewForThreeColumns = (Style)FindResource("StyleQuestionPreviewForThreeColumns");
            m_StyleQuestionPreviewForFourColumns = (Style)FindResource("StyleQuestionPreviewForFourColumns");
        }

        private void App_StationDefinitionChanged(object sender, EventArgs e)
        {
            DeepDiveStationDefinition stationDefinition = App.Current.StationDefinition;
            m_TxtHeadline.SetLocalizedText(stationDefinition.FinalOverviewHeadline);
            m_Footer.NextButtonText = stationDefinition.FinalOverviewButtonText;
            m_StackQuestions.Children.Clear();
            m_QuestionPreviews.Clear();
            foreach (TouchBeamerSurveyQuestions touchBeamerQuestions in stationDefinition.TouchBeamerSurveyQuestions)
            {
                AddTouchBeamerQuestions(touchBeamerQuestions);
            }
        }

        private void AddTouchBeamerQuestions(TouchBeamerSurveyQuestions touchBeamerQuestions)
        {
            TextBlock stationTitleTextBlock = new TextBlock
            {
                Style = m_TextStyleStationHeadline
            };
            stationTitleTextBlock.SetLocalizedText(touchBeamerQuestions.StationTitle);
            m_StackQuestions.Children.Add(stationTitleTextBlock);
            bool useFourColumns = ConstFourColumnQuestionCounts.Contains(touchBeamerQuestions.SurveyQuestions.Count);
            UniformGrid questionsGrid = new UniformGrid
            {
                Style = useFourColumns ?
                    m_StyleGridQuestionPreviewsFourColumns : m_StyleGridQuestionPreviewsThreeColumns
            };
            int index = 0;
            foreach (SurveyQuestion question in touchBeamerQuestions.SurveyQuestions)
            {
                questionsGrid.Children.Add(CreateQuestionPreviewFor(
                    question: question,
                    useFourColumns: useFourColumns,
                    index: index,
                    count: touchBeamerQuestions.SurveyQuestions.Count));
                index++;
            }
            m_StackQuestions.Children.Add(questionsGrid);
        }

        private QuestionPreview CreateQuestionPreviewFor(SurveyQuestion question, bool useFourColumns,
            int index, int count)
        {
            QuestionPreview preview = new QuestionPreview
            {
                Style = useFourColumns ? m_StyleQuestionPreviewForFourColumns : m_StyleQuestionPreviewForThreeColumns,
                IsForDeepDive = false,
                IsHitTestVisible = false,
                QuestionIndexText = LocalizationString.Create($"{index + 1}/{count}"),
                Question = question
            };
            m_QuestionPreviews.Add(question.QuestionID, preview);
            return preview;
        }

        #endregion Konstruktor und Initialisierung

        #region Öffentliche Methoden

        public void ShowFor(CtsUserInformation user)
        {
            foreach (string questionID in m_QuestionPreviews.Keys) m_QuestionPreviews[questionID].User = user;
            m_ScrollMain.ScrollToHome();
        }

        #endregion Öffentliche Methoden

        #region Ereignishandler

        private void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            OnUserInteraction();
        }

        private void Footer_NextClick(object sender, EventArgs e)
        {
            OnNextClick();
        }

        #endregion Ereignishandler
    }
}
