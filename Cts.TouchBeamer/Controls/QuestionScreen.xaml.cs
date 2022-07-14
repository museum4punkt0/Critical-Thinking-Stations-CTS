using Gemelo.Components.Cts.Code.Data.Survey;
using Gemelo.Components.Cts.Code.Data.Users;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Gemelo.Applications.Cts.TouchBeamer.Controls
{
    /// <summary>
    /// Screen für die Umfrage
    /// </summary>
    public partial class QuestionScreen : UserControl
    {
        public bool IsAnswered => m_QuestionControl.IsAnswered;

        public event EventHandler AnswerChanged;

        protected void OnAnswerChanged()
        {
            AnswerChanged?.Invoke(this, null);
        }

        public QuestionScreen()
        {
            InitializeComponent();
        }

        public void ShowQuestion(SurveyQuestion question, CtsUserAnswerInformation userAnswers)
        {
            m_QuestionControl.ShowQuestion(question, userAnswers);
        }

        public IEnumerable<SurveyAnswer> GetAnswers() => m_QuestionControl.GetAnswers();

        private void QuestionControl_AnswerChanged(object sender, EventArgs e)
        {
            OnAnswerChanged();
        }
    }
}
