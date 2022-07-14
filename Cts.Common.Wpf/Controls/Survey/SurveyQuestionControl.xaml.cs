using Gemelo.Components.Common.Collections;
using Gemelo.Components.Common.Localization;
using Gemelo.Components.Common.Wpf.Controls.Buttons;
using Gemelo.Components.Common.Wpf.Localization;
using Gemelo.Components.Common.Wpf.Text.Formatting;
using Gemelo.Components.Common.Wpf.UI;
using Gemelo.Components.Cts.Code.Data.Survey;
using Gemelo.Components.Cts.Code.Data.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Gemelo.Components.Cts.Controls.Survey
{
    /// <summary>
    /// Umfrage-Control
    /// </summary>
    public partial class SurveyQuestionControl : UserControl
    {
        #region Konstanten
        
        private static readonly CombinedFormattingHandler MultipleAnswersTextStyleHandler 
            = new CombinedFormattingHandler
            {
                TagName = "multi",
                Children = new List<SpanBasedFormattingHandler>()
                {
                    new FontSizeHandler
                    {
                        FontSizeOrFactor = 35
                    }
                }
            };

        private static readonly LocalizationString ConstQuestionWithHintFormat =
            LocalizationString.Create("{0} [multi]{1}[/multi]");

        #endregion Konstanten

        private List<BorderedButton> m_AnswerButtons;
        private List<TextBlock> m_AnswerTexts;

        private SurveyQuestion m_CurrentQuestion;

        public bool IsAnswered => GetAnswers().Any();

        public Style ButtonStyle
        {
            get => m_AnswerButtons[0].Style;
            set => m_AnswerButtons.ForEach(answerButton => answerButton.Style = value);
        }

        public event EventHandler AnswerChanged;

        protected void OnAnswerChanged()
        {
            AnswerChanged?.Invoke(this, null);
        }

        static SurveyQuestionControl()
        {
            TextFormattingEngine.AllHandlers.Add(MultipleAnswersTextStyleHandler);
        }

        public SurveyQuestionControl()
        {
            InitializeComponent();
            InitializeButtons();
            InitializeComboYearOfBirth();
        }

        private void InitializeButtons()
        {
            m_AnswerButtons = m_GridAnswers.Children.Cast<BorderedButton>().ToList();
            m_AnswerButtons.ForEach(button => button.Click += AnswerButton_Click);
            m_AnswerTexts = m_AnswerButtons.Select(button => button.Content as TextBlock).ToList();
        }

        private void InitializeComboYearOfBirth()
        {
            var yearAnswers = Enumerable.Range(DateTime.Now.Year - 101, 101)
                .OrderByDescending(year => year)
                .Select(year => new SurveyAnswer
                {
                    AnswerID = year.ToString(),
                    IsExclusive = true,
                    Text = LocalizationString.Create(year.ToString())
                });
            m_ComboYearOfBirth.ItemsSource = yearAnswers;
            LocalizationExtensions.UseVisualTree = true;
            m_ComboYearOfBirth.SetLocalization(Languages.Current);
        }

        public void ShowQuestion(SurveyQuestion question, CtsUserAnswerInformation userAnswers)
        {
            m_CurrentQuestion = question;
            if (question.HintText.IsEmptyOrWhiteSpace)
            {
                m_TxtQuestion.SetLocalizedText(question.QuestionText);
            }
            else
            {
                m_TxtQuestion.SetLocalizedText(LocalizationString.Format(
                    ConstQuestionWithHintFormat, question.QuestionText, question.HintText));
            }
            m_ComboYearOfBirth.Visibility = (question.QuestionType == SurveyQuestionType.YearOfBirth).ToVisibility();
            m_GridAnswers.Visibility = (question.QuestionType == SurveyQuestionType.Default).ToVisibility();
            m_GridAnswers.Columns = question.Answers.Count <= 3 ? 1 : 2;
            if (question.QuestionType == SurveyQuestionType.YearOfBirth) UpdateComboSelection(userAnswers);
            if (question.QuestionType == SurveyQuestionType.Default)
            {
                UpdateAnswerButtons();
                PreselectAnswersFromUser(userAnswers);
            }
        }

        private void UpdateComboSelection(CtsUserAnswerInformation userAnswers)
        {
            IEnumerable<SurveyAnswer> answers = m_ComboYearOfBirth.ItemsSource as IEnumerable<SurveyAnswer>;
            m_ComboYearOfBirth.SelectedItem =
                answers.FirstOrDefault(answer => userAnswers?.AnswerIDs?.Contains(answer.AnswerID) == true);
        }

        private void UpdateAnswerButtons()
        {
            for (int i = 0; i < m_AnswerButtons.Count; i++)
            {
                m_AnswerButtons[i].IsActive = false;
                if (i < m_CurrentQuestion.Answers.Count)
                {
                    m_AnswerButtons[i].Visibility = Visibility.Visible;
                    m_AnswerButtons[i].Tag = m_CurrentQuestion.Answers[i];
                    m_AnswerTexts[i].SetLocalizedText(m_CurrentQuestion.Answers[i].Text);
                }
                else m_AnswerButtons[i].Visibility = Visibility.Collapsed;
            }
        }

        private void PreselectAnswersFromUser(CtsUserAnswerInformation userAnswers)
        {
            if (userAnswers != null)
            {
                foreach (string answerID in userAnswers.AnswerIDs)
                {
                    SurveyAnswer answer = m_CurrentQuestion.Answers.FirstOrDefault(a => a.AnswerID == answerID);
                    if (answer != null)
                    {
                        BorderedButton answerButton = m_AnswerButtons.First(button => button.Tag == answer);
                        ToggleAnswer(answerButton, answer);
                    }
                }
            }
        }

        public IEnumerable<SurveyAnswer> GetAnswers()
        {
            if (m_CurrentQuestion.QuestionType == SurveyQuestionType.YearOfBirth)
            {
                if (m_ComboYearOfBirth.SelectedItem is SurveyAnswer answer) return new SurveyAnswer[] { answer };
                else return new SurveyAnswer[0];
            }
            else
            {
                return m_AnswerButtons
                    .Where(button => button.Visibility == Visibility.Visible && button.IsActive)
                    .Select(button => (SurveyAnswer)button.Tag);
            }
        }

        private void ToggleAnswer(BorderedButton answerButton, SurveyAnswer answer)
        {
            if (answerButton.IsActive) answerButton.IsActive = false;
            else
            {
                // Alle eingeloggten Antworten, die exklusiv sind ausloggen
                m_AnswerButtons.Where(answerButton => answerButton.IsActive).ForEach(answerButton =>
                {
                    SurveyAnswer otherAnswer = (SurveyAnswer)answerButton.Tag;
                    if (otherAnswer.IsExclusive) answerButton.IsActive = false;
                });
                // Prüfen, ob die nur eine Antwort erlaubt ist oder die aktuelle Antwort exklusiv ist,
                // dann alle ausloggen
                if (m_CurrentQuestion.MaximumAnswerCount <= 1 || answer.IsExclusive)
                {
                    m_AnswerButtons.ForEach(button => button.IsActive = false);
                }
                // Maximale Anzahl der Antworten einhalten
                else if (m_CurrentQuestion.MaximumAnswerCount < m_CurrentQuestion.Answers.Count)
                {
                    while (GetAnswers().Count() >= m_CurrentQuestion.MaximumAnswerCount)
                    {
                        m_AnswerButtons.FirstOrDefault(button => button.IsActive).IsActive = false;
                    }
                }
                answerButton.IsActive = true;
            }
            OnAnswerChanged();
        }

        private void AnswerButton_Click(object sender, RoutedEventArgs e)
        {
            BorderedButton typedSender = (BorderedButton)sender;
            SurveyAnswer answer = (SurveyAnswer)typedSender.Tag;
            ToggleAnswer(typedSender, answer);
        }

        private void ComboYearOfBirth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_CurrentQuestion.QuestionType == SurveyQuestionType.YearOfBirth) OnAnswerChanged();
        }
    }
}
