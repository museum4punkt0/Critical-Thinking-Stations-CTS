using Gemelo.Components.Common.Localization;
using Gemelo.Components.Cts.Code.Data.Survey;
using Gemelo.Components.Cts.Code.Data.Users;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Gemelo.Applications.Cts.DeepDive.Controls
{
    /// <summary>
    /// Screen für eine Frage
    /// </summary>
    public partial class QuestionScreen : UserControl
    {
        private Style m_StyleSurveyDeepDiveReanswerAnswerButton;
        private Style m_StyleSurveyDeepDiveDemographyAnswerButton;

        public SurveyQuestion Question { get; private set; }

        public bool IsAnswered => m_QuestionControl.IsAnswered;

        public bool UseDeepDiveColors
        {
            get => m_Footer.UseDeepDiveColors;
            set
            {
                m_Footer.UseDeepDiveColors = value;
                m_QuestionControl.ButtonStyle = value ?
                    m_StyleSurveyDeepDiveReanswerAnswerButton : m_StyleSurveyDeepDiveDemographyAnswerButton;
            }
        }

        public LocalizationString ButtonText
        {
            get => m_Footer.NextButtonText;
            set => m_Footer.NextButtonText = value;
        }

        public event EventHandler AnswerChanged;

        protected void OnAnswerChanged()
        {
            AnswerChanged?.Invoke(this, null);
        }

        public event EventHandler NextClick;

        protected void OnNextClick()
        {
            NextClick?.Invoke(this, null);
        }

        public QuestionScreen()
        {
            InitializeComponent();
            InitializeStyles();
        }

        private void InitializeStyles()
        {
            m_StyleSurveyDeepDiveReanswerAnswerButton =
                (Style)FindResource("StyleSurveyDeepDiveReanswerAnswerButton");
            m_StyleSurveyDeepDiveDemographyAnswerButton =
                (Style)FindResource("StyleSurveyDeepDiveDemographyAnswerButton");
        }

        public void ShowQuestion(SurveyQuestion question, CtsUserAnswerInformation userAnswers)
        {
            Question = question;
            // m_QuestionControl.ShowQuestion(question, userAnswers);
            m_QuestionControl.ShowQuestion(question, null);
            UpdateNextButton();
        }

        public IEnumerable<SurveyAnswer> GetAnswers() => m_QuestionControl.GetAnswers();

        public void UpdateNextButton()
        {
            m_Footer.IsNextButtonEnabled = true;
            m_Footer.DoesNextButtonLooksLikeEnabled = m_QuestionControl.IsAnswered;
        }

        public void SetWaitForSaving(LocalizationString waitForSavingText)
        {
            m_Footer.DoesNextButtonLooksLikeEnabled = false;
            m_Footer.IsNextButtonEnabled = false;
            m_Footer.NextButtonText = waitForSavingText;
        }

        private void QuestionControl_AnswerChanged(object sender, EventArgs e)
        {
            UpdateNextButton();
            OnAnswerChanged();
        }

        private void Footer_NextClick(object sender, EventArgs e)
        {
            OnNextClick();
        }
    }
}
