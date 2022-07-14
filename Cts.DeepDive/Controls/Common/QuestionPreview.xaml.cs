using Gemelo.Components.Common.Collections;
using Gemelo.Components.Common.Localization;
using Gemelo.Components.Common.Wpf.Localization;
using Gemelo.Components.Common.Wpf.UI;
using Gemelo.Components.Cts.Code.Data.Stations;
using Gemelo.Components.Cts.Code.Data.Survey;
using Gemelo.Components.Cts.Code.Data.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gemelo.Applications.Cts.DeepDive.Controls.Common
{
    /// <summary>
    /// Zeigt eine Vorschau der Frage und der Besucherantwort an
    /// </summary>
    public partial class QuestionPreview : UserControl
    {
        #region Konstanten

        private const string ConstConcatenateSeparator = "[br/]+[br/]";

        #endregion Konstanten

        #region Felder und Eigenschaften

        private Style m_StyleBorderDefault;
        private Style m_StyleBorderDeepDive;

        private Style m_StyleBorderDeepDiveIconOpen;
        private Style m_StyleBorderDeepDiveIconVisited;

        private bool m_IsPressed;

        public bool IsForDeepDive
        {
            get => m_BorderMain.Style == m_StyleBorderDeepDive;
            set
            {
                m_BorderDeepDiveIcon.Visibility = value.ToVisibility();
                m_BorderQuestionIndex.Visibility = (!value).ToVisibility();
                m_BorderMain.Style = value ? m_StyleBorderDeepDive : m_StyleBorderDefault;
            }
        }

        public bool HasDeepDiveVisited
        {
            get => m_PathDeepDiveVisited.Visibility == Visibility.Visible;
            set
            {
                m_PathDeepDiveVisited.Visibility = value.ToVisibility();
                m_PathDeepDiveOpen.Visibility = (!value).ToVisibility();
                m_BorderDeepDiveIcon.Style = value ? m_StyleBorderDeepDiveIconVisited : m_StyleBorderDeepDiveIconOpen;
            }
        }

        private LocalizationString m_QuestionIndexText;

        public LocalizationString QuestionIndexText
        {
            get => m_QuestionIndexText;
            set
            {
                m_QuestionIndexText = value;
                m_TxtQuestionIndex.SetLocalizedText(value);
            }
        }

        private SurveyQuestion m_Question;

        public SurveyQuestion Question 
        { 
            get => m_Question;
            set
            {
                m_Question = value;
                UpdateQuestionAndUser();
            }
        }

        private CtsUserInformation m_User;

        public CtsUserInformation User
        {
            get => m_User;
            set
            {
                m_User = value;
                UpdateQuestionAndUser();
            }
        }

        #endregion Felder und Eigenschaften

        #region Ereignisse

        public event EventHandler Click;

        protected void OnClick()
        {
            Click?.Invoke(this, null);
        }

        #endregion Ereignisse

        #region Konstruktur und Initialisierung

        public QuestionPreview()
        {
            InitializeComponent();
            InitializeStyles();
            IsForDeepDive = false;
            HasDeepDiveVisited = false;
        }

        private void InitializeStyles()
        {
            m_StyleBorderDefault = (Style)FindResource("StyleBorderDefault");
            m_StyleBorderDeepDive = (Style)FindResource("StyleBorderDeepDive");
            m_StyleBorderDeepDiveIconOpen = (Style)FindResource("StyleBorderDeepDiveIconOpen");
            m_StyleBorderDeepDiveIconVisited = (Style)FindResource("StyleBorderDeepDiveIconVisited");
        }

        #endregion Konstruktur und Initialisierung

        public void UpdateQuestionAndUser()
        {
            if (m_Question != null && m_User != null)
            {
                DeepDiveStationDefinition stationDefinition = App.Current.StationDefinition;
                m_TxtQuestionHeadline.SetLocalizedText(stationDefinition.QuestionHeadline);
                m_TxtQuestionText.SetLocalizedText(m_Question.QuestionText);
                SurveyAnswerType answerType = m_User.GetAnswerTypeFor(m_Question.QuestionID);
                switch (answerType)
                {
                    case SurveyAnswerType.NoAnswer:
                        m_TxtAnswerHeadline.SetLocalizedText(stationDefinition.NoAnswerText);
                        break;
                    case SurveyAnswerType.Skipped:
                        m_TxtAnswerHeadline.SetLocalizedText(stationDefinition.SkippedQuestionText);
                        break;
                    case SurveyAnswerType.Answer:
                        m_TxtAnswerHeadline.SetLocalizedText(stationDefinition.AnswerHeadline);
                        m_TxtAnswerText.SetLocalizedText(GetAnswerTextFor(m_Question, m_User));
                        break;
                }
                m_TxtAnswerText.Visibility = (answerType == SurveyAnswerType.Answer).ToVisibility();
            }
        }

        private static LocalizationString GetAnswerTextFor(SurveyQuestion question, CtsUserInformation user)
        {
            CtsUserAnswerInformation answerInformation = user.GetLatestAnswersFor(question.QuestionID);
            var answerTexts = question.Answers
                .Where(answer => answerInformation.AnswerIDs.Contains(answer.AnswerID))
                .Select(answer => answer.Text);
            var answerConcatenatedText = new LocalizationString();
            foreach (string language in Languages.Available)
            {
                IEnumerable<string> localizedAnswers = answerTexts.Select(answerText => answerText.GetFor(language));
                string concatenated = localizedAnswers.ToConcatenatedString(ConstConcatenateSeparator);
                answerConcatenatedText.SetFor(language, concatenated);
            }
            return answerConcatenatedText;
        }

        private void GridMain_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.StylusDevice == null) m_IsPressed = true;
            //if (e.StylusDevice == null) OnClick();
        }

        private void GridMain_TouchDown(object sender, TouchEventArgs e)
        {
            //m_IsPressed = true;
            //OnClick();
        }

        private void GridMain_MouseLeave(object sender, MouseEventArgs e)
        {
            if (e.StylusDevice == null) m_IsPressed = false;
        }

        private void GridMain_TouchLeave(object sender, TouchEventArgs e)
        {
            //m_IsPressed = false;
        }

        private void GridMain_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.StylusDevice == null && m_IsPressed) OnClick();
        }

        private void GridMain_TouchUp(object sender, TouchEventArgs e)
        {
            //if (m_IsPressed) OnClick();
        }

        private void GridMain_StylusSystemGesture(object sender, StylusSystemGestureEventArgs e)
        {
            if (e.SystemGesture == SystemGesture.Tap) OnClick();
        }
    }
}
