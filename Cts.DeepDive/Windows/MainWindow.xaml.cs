using Gemelo.Applications.Cts.DeepDive.Code.Events;
using Gemelo.Applications.Cts.DeepDive.Controls;
using Gemelo.Components.Common.Exhibits.Settings;
using Gemelo.Components.Common.Localization;
using Gemelo.Components.Common.Tracing;
using Gemelo.Components.Cts.Code.Data.Stations;
using Gemelo.Components.Cts.Code.Data.Survey;
using Gemelo.Components.Cts.Code.Data.Users;
using Gemelo.Components.Cts.Windows;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace Gemelo.Applications.Cts.DeepDive
{
    /// <summary>
    /// Hauptfenster CTS DeepDive
    /// </summary>
    public partial class MainWindow : CtsMainWindow
    {
        #region Eingebettete Typen

        private enum State
        {
            Startup,
            DeepDiveOverview,
            DeepDiveContent,
            ReanswerQuestion,
            FinalOverview,
            DemographyIntro,
            DemographyQuestions,
            Goodbye
        }

        #endregion Eingebettete Typen

        #region Felder und Eigenschaften

        private Style m_StyleMessageBoxDefault;
        private Style m_StyleMessageBoxDeepDive;

        private State m_State;

        private List<SurveyQuestion> m_CurrentDemographyQuestions;

        private int m_CurrentDemographyQuestionIndex;

        #endregion Felder und Eigenschaften

        #region Konstruktor und Initialisierung

        public MainWindow()
        {
            InitializeComponent();
            CheckForRatio16to10AndRfidHand();
            InitializeStyles();
            if (App.IsAvailable) BindApplicationEvents();
        }

        private void CheckForRatio16to10AndRfidHand()
        {
            if (App.IsAvailable && App.Current.UseRatio16to10)
            {
                m_GridMain.Height = m_GridMain.Width / 16.0 * 10.0;
                m_StartScreen.IsRfidHandTopRightVisible = App.Current.IsRfidHandTopRightVisible;
            }
        }

        private void InitializeStyles()
        {
            m_StyleMessageBoxDefault = (Style)FindResource("StyleMessageBoxDefault");
            m_StyleMessageBoxDeepDive = (Style)FindResource("StyleMessageBoxDeepDive");
        }

        private void BindApplicationEvents()
        {
            App.Current.StationDefinitionChanged += App_StationDefinitionChanged;
        }

        #endregion Konstruktor und Initialisierung

        #region Öffentliche Methoden

        public override void ProcessNewUser()
        {
            base.ProcessNewUser();
            m_MessageBox.HideDirectly();
            if (App.Current.CurrentUser?.State.HasFlag(CtsUserState.SurveyCompleted) == true)
            {
                TraceX.WriteInformational(
                    message: "User had already completed survey. DeepDiveOverview will be skipped!");
                ToFinalOverview();
            }
            else ToDeepDiveOverview(isInstructionExpanded: true);
        }

        public override void TryRestartRestartTimer()
        {
            if (!App.Current.RfidController.IsRfidDetected && m_State != State.Startup)
            {
                if (m_State >= State.Goodbye)
                {
                    RestartTimer.StartVaryingInterval(App.Current.StationDefinition.GoodbyeTimeout);
                }
                else if (m_State != State.DeepDiveContent || !m_DeepDiveContentScreen.IsMediaPlaying())
                {
                    RestartTimer.Restart();
                }
            }
            else RestartTimer.Stop();
        }

        #endregion Öffentliche Methoden

        #region Private Methoden

        public override void ShowStartup()
        {
            base.ShowStartup();
            m_State = State.Startup;
            App.Current.ResetUser();
            m_OfflineWarningOverlay.Start();
            m_Background.FadeOutElements();
            m_Background.ToDefaultColor();
            m_StartScreen.StartAnimation();
            ShowMainControl(m_StartScreen);
            TryRestartRestartTimer();
        }

        private void ToDeepDiveOverview(bool isInstructionExpanded)
        {
            TraceX.WriteInformational(nameof(ToDeepDiveOverview));
            m_State = State.DeepDiveOverview;
            m_Background.FadeOutElements();
            m_Background.ProgressText = LocalizationString.Empty;
            m_Background.ToDefaultColor();
            m_StartScreen.StopAnimation();
            m_DeepDiveOverviewScreen.ShowFor(App.Current.CurrentUser);
            m_DeepDiveOverviewScreen.SetIsInstructionExpandedDirectly(isInstructionExpanded);
            ShowMainControl(m_DeepDiveOverviewScreen);
            TryRestartRestartTimer();
        }

        private void FromDeepDiveOverviewToDeepDiveContentFor(SurveyQuestion question)
        {
            TraceX.WriteInformational(
                $"{nameof(FromDeepDiveOverviewToDeepDiveContentFor)}, question: {question?.QuestionID}");
            m_State = State.DeepDiveContent;
            _ = TryToSaveVisit(question);
            m_Background.ProgressText = LocalizationString.Empty;
            m_Background.ToDeepDiveColor();
            m_DeepDiveContentScreen.ShowFor(question);
            ShowMainControl(m_DeepDiveContentScreen);
            TryRestartRestartTimer();
        }

        private async void AskForFromDeepDiveOverviewToFinalOverview()
        {
            TraceX.WriteInformational(nameof(AskForFromDeepDiveOverviewToFinalOverview));
            if (await m_MessageBox.Show(
                title: App.Current.StationDefinition.CompleteSurveyMessageTitle,
                text: App.Current.StationDefinition.CompleteSurveyMessageText,
                buttonTexts: new LocalizationString[]
                {
                    App.Current.StationDefinition.CompleteSurveyMessageButtonConfirmText,
                    App.Current.StationDefinition.CompleteSurveyMessageButtonCancelText,
                }) == 0)
            {
                await App.Current.AddUserState(CtsUserState.SurveyCompleted);
                ToFinalOverview();
            }
        }

        private void ToFinalOverview()
        {
            TraceX.WriteInformational(nameof(ToFinalOverview));
            m_State = State.FinalOverview;
            m_Background.ToDefaultColor();
            m_FinalOverviewScreen.ShowFor(App.Current.CurrentUser);
            ShowMainControl(m_FinalOverviewScreen);
            TryRestartRestartTimer();
        }

        private async Task FromDeepDiveContentTo()
        {
            TraceX.WriteInformational(nameof(FromDeepDiveContentTo));
            bool isAlreadyAnswered = App.Current.CurrentUser
                .GetAnswerTypeFor(m_DeepDiveContentScreen.Question.QuestionID) == SurveyAnswerType.Answer;
            DeepDiveStationDefinition stationDefinition = App.Current.StationDefinition;
            LocalizationString title = isAlreadyAnswered ?
                stationDefinition.ReanswerQuestionMessageTitle : stationDefinition.FirstAnswerQuestionMessageTitle;
            LocalizationString text = isAlreadyAnswered ?
                stationDefinition.ReanswerQuestionMessageText : stationDefinition.FirstAnswerQuestionMessageText;
            LocalizationString confirmButtonText = isAlreadyAnswered ?
                stationDefinition.ReanswerQuestionButtonConfirmText :
                stationDefinition.FirstAnswerQuestionButtonConfirmText;
            LocalizationString cancelButtonText = isAlreadyAnswered ?
                stationDefinition.ReanswerQuestionButtonCancelText :
                stationDefinition.FirstAnswerQuestionButtonCancelText;
            m_MessageBox.Style = m_StyleMessageBoxDeepDive;
            int result = await m_MessageBox.Show(title, text, cancelButtonText, confirmButtonText);
            if (result == 0) ToDeepDiveOverview(isInstructionExpanded: false);
            else if (result == 1) FromDeepDiveContentToReanswerQuestion(m_DeepDiveContentScreen.Question);
        }

        private void FromDeepDiveContentToReanswerQuestion(SurveyQuestion question)
        {
            TraceX.WriteInformational(
                $"{nameof(FromDeepDiveContentToReanswerQuestion)}, question: {question?.QuestionID}");
            m_State = State.ReanswerQuestion;
            m_Background.FadeInElements();
            m_ReanswerQuestionScreen.ShowQuestion(question,
                App.Current.CurrentUser.GetLatestAnswersFor(question.QuestionID));
            m_ReanswerQuestionScreen.IsHitTestVisible = true;
            m_Background.ProgressText = LocalizationString.Empty;
            m_ReanswerQuestionScreen.ButtonText = App.Current.StationDefinition.QuestionButtonText;
            ShowMainControl(m_ReanswerQuestionScreen);
            TryRestartRestartTimer();
        }

        private async Task FromReanswerQuestionToNext()
        {
            TraceX.WriteInformational(nameof(FromReanswerQuestionToNext));
            TryRestartRestartTimer();
            DeepDiveStationDefinition stationDefinition = App.Current.StationDefinition;
            m_MessageBox.Style = m_StyleMessageBoxDeepDive;
            if (m_ReanswerQuestionScreen.IsAnswered || await m_MessageBox.Show(
                title: stationDefinition.SkipQuestionMessageText,
                text: null,
                buttonTexts: stationDefinition.SkipQuestionMessageButtonText) == 0)
            {
                if (await TrySaveAnswers(m_ReanswerQuestionScreen))
                {
                    ToDeepDiveOverview(isInstructionExpanded: false);
                }
                else ReactivateReanswerQuestion();
            }
        }

        private void ReactivateReanswerQuestion()
        {
            m_ReanswerQuestionScreen.IsHitTestVisible = true;
            m_ReanswerQuestionScreen.UpdateNextButton();
            m_ReanswerQuestionScreen.ButtonText = App.Current.StationDefinition.QuestionButtonText;
            TryRestartRestartTimer();
        }

        private void FromFinalOverviewTo()
        {
            TraceX.WriteInformational(nameof(FromFinalOverviewTo));
            if (App.Current.CurrentUser?.State.HasFlag(CtsUserState.DemographySkipped) == true)
            {
                TraceX.WriteInformational(
                    message: "User had already skipped demography questions. Directly go to goodbye!");
                ToGoodbye();
            }
            else
            {
                UpdateCurrentDemographyQuestions();
                if (m_CurrentDemographyQuestions.Count == App.Current.StationDefinition.DemographyQuestions.Count)
                {
                    FromFinalOverviewToDemographyIntro();
                }
                else if (m_CurrentDemographyQuestions.Count > 0)
                {
                    FromFinalOverviewToDemographyQuestions();
                }
                else ToGoodbye();
            }
        }

        private void FromFinalOverviewToDemographyIntro()
        {
            TraceX.WriteInformational(nameof(FromFinalOverviewToDemographyIntro));
            m_State = State.DemographyIntro;
            m_Background.FadeInElements();
            m_Background.ToDefaultColor();
            ShowMainControl(m_DemographyIntroScreen);
            TryRestartRestartTimer();
        }

        private void FromFinalOverviewToDemographyQuestions()
        {
            TraceX.WriteInformational(nameof(FromFinalOverviewToDemographyQuestions));
            m_State = State.DemographyQuestions;
            m_Background.FadeInElements();
            m_Background.ToDefaultColor();
            m_CurrentDemographyQuestionIndex = 0;
            ShowDemographyQuestion();
        }

        private void FromDemographyIntroToDemographyQuestions()
        {
            TraceX.WriteInformational(nameof(FromDemographyIntroToDemographyQuestions));
            m_State = State.DemographyQuestions;
            m_Background.FadeInElements();
            m_Background.ToDefaultColor();
            m_CurrentDemographyQuestionIndex = 0;
            ShowDemographyQuestion();
        }

        private async Task FromDemographyQuestionToNext()
        {
            TraceX.WriteInformational(nameof(FromDemographyQuestionToNext));
            TryRestartRestartTimer();
            DeepDiveStationDefinition stationDefinition = App.Current.StationDefinition;
            QuestionScreen currentDemographyQuestionScreen =
                m_DemographyQuestionScreenA.Visibility == Visibility.Visible ?
                m_DemographyQuestionScreenA : m_DemographyQuestionScreenB;
            m_MessageBox.Style = m_StyleMessageBoxDefault;
            if (currentDemographyQuestionScreen.IsAnswered || await m_MessageBox.Show(
                title: stationDefinition.SkipQuestionMessageText,
                text: null,
                buttonTexts: stationDefinition.SkipQuestionMessageButtonText) == 0)
            {
                if (await TrySaveAnswers(currentDemographyQuestionScreen))
                {
                    m_CurrentDemographyQuestionIndex++;
                    if (m_CurrentDemographyQuestionIndex < m_CurrentDemographyQuestions.Count)
                    {
                        ShowDemographyQuestion();
                    }
                    else FromDemographyQuestionsToGoodbye();
                }
                else ReactivateDemographyQuestion();
            }
        }

        private void UpdateCurrentDemographyQuestions()
        {
            CtsUserInformation user = App.Current.CurrentUser;
            m_CurrentDemographyQuestions =
                user.GetUnansweredQuestionsFor(App.Current.StationDefinition.DemographyQuestions);
        }

        private void ShowDemographyQuestion()
        {
            QuestionScreen nextQuestionScreen = m_DemographyQuestionScreenA.Visibility == Visibility.Visible ?
                m_DemographyQuestionScreenB : m_DemographyQuestionScreenA;
            DeepDiveStationDefinition stationDefinition = App.Current.StationDefinition;
            SurveyQuestion question = m_CurrentDemographyQuestions[m_CurrentDemographyQuestionIndex];
            nextQuestionScreen.ShowQuestion(question,
                App.Current.CurrentUser.GetLatestAnswersFor(question.QuestionID));
            nextQuestionScreen.IsHitTestVisible = true;
            m_Background.ProgressText = LocalizationString.Format(stationDefinition.QuestionProgressTextFormat,
                m_CurrentDemographyQuestionIndex + 1, m_CurrentDemographyQuestions.Count);
            nextQuestionScreen.ButtonText = App.Current.StationDefinition.QuestionButtonText;
            nextQuestionScreen.UpdateNextButton();
            ShowMainControl(nextQuestionScreen);
            TryRestartRestartTimer();
        }

        private void ReactivateDemographyQuestion()
        {
            QuestionScreen currentDemographyQuestionScreen =
                m_DemographyQuestionScreenA.Visibility == Visibility.Visible ?
                m_DemographyQuestionScreenA : m_DemographyQuestionScreenB;
            currentDemographyQuestionScreen.IsHitTestVisible = true;
            currentDemographyQuestionScreen.ButtonText = App.Current.StationDefinition.QuestionButtonText;
            currentDemographyQuestionScreen.UpdateNextButton();
            TryRestartRestartTimer();
        }

        private void FromDemographyQuestionsToGoodbye()
        {
            TraceX.WriteInformational(nameof(FromDemographyQuestionsToGoodbye));
            ToGoodbye();
        }

        private async void FromDemographyIntroToGoodbye()
        {
            TraceX.WriteInformational(nameof(FromDemographyIntroToGoodbye));
            await App.Current.AddUserState(CtsUserState.DemographySkipped);
            ToGoodbye();
        }

        private void ToGoodbye()
        {
            TraceX.WriteInformational(nameof(ToGoodbye));
            m_State = State.Goodbye;
            m_Background.ToDefaultColor();
            m_Background.ProgressText = LocalizationString.Empty;
            ShowMainControl(m_GoodbyeScreen);
            TryRestartRestartTimer();
        }

        private async Task TryToSaveVisit(SurveyQuestion question)
        {
            App.Current.CurrentUser.DeepDiveVisits.Add(new DeepDiveVisit
            {
                QuestionID = question.QuestionID,
                Timestamp = DateTime.Now
            });
            await App.Current.SaveDeepDiveVisit(question.QuestionID);
        }

        private async Task<bool> TrySaveAnswers(QuestionScreen questionScreen)
        {
            DeepDiveStationDefinition stationDefinition = App.Current.StationDefinition;
            questionScreen.SetWaitForSaving(stationDefinition.WaitForSavingText);
            questionScreen.IsHitTestVisible = false;
            if (await App.Current.SaveAnswers(questionScreen.Question.QuestionID,
                questionScreen.GetAnswers()))
            {
                return true;
            }
            else
            {
                m_MessageBox.Style = questionScreen.UseDeepDiveColors ?
                    m_StyleMessageBoxDeepDive : m_StyleMessageBoxDefault;
                await m_MessageBox.Show(
                    title: stationDefinition.SaveErrorTitle,
                    text: null,
                    buttonTexts: stationDefinition.SaveErrorButtonText);
                return false;
            }
        }

        protected override bool IsRestartAllowed() => false;

        protected override async void RestartTimer_Expired(object sender, EventArgs e)
        {
            base.RestartTimer_Expired(sender, e);
            if (m_State != State.DeepDiveContent || !m_DeepDiveContentScreen.IsMediaPlaying() &&
                m_State < State.Goodbye)
            {
                RestartTimer.Stop();
                DeepDiveStationDefinition stationDefinition = App.Current.StationDefinition;
                m_MessageBox.Style = m_State == State.DeepDiveContent || m_State == State.ReanswerQuestion ?
                    m_StyleMessageBoxDeepDive : m_StyleMessageBoxDefault;
                if (await m_MessageBox.ShowTimeout(
                    titleFormat: stationDefinition.TimeoutMessageTextFormat,
                    textFormat: null,
                    buttonText: stationDefinition.TimeoutMessageButtonText,
                    timeout: stationDefinition.TimeoutMessageDuration) > 0)
                {
                    ShowStartup();
                }
                else RestartTimer.Restart();
            }
            else if (m_State == State.Goodbye) ShowStartup();
        }

        #endregion Private Methoden

        #region Ereignishandler

        private void App_StationDefinitionChanged(object sender, EventArgs e)
        {
            DeepDiveStationDefinition stationDefinition = App.Current.StationDefinition;
            m_StartScreen.Title = stationDefinition.Title;
            m_StartScreen.RfidHintText = stationDefinition.RfidHintText;
            m_StartScreen.RfidHintPosition =
                App.Current.OverrideRfidHintPosition ?? stationDefinition.RfidHintPosition;
            RestartTimer.Interval = stationDefinition.RestartTimeout;
        }

        private void DeepDiveOverviewScreen_UserInteraction(object sender, EventArgs e)
        {
            if (m_State == State.DeepDiveOverview) TryRestartRestartTimer();
        }

        private void DeepDiveOverviewScreen_QuestionClick(object sender, SurveyQuestionEventArgs e)
        {
            if (m_State == State.DeepDiveOverview) FromDeepDiveOverviewToDeepDiveContentFor(e.Question);
        }

        private void DeepDiveOverviewScreen_NextClick(object sender, EventArgs e)
        {
            if (m_State == State.DeepDiveOverview) AskForFromDeepDiveOverviewToFinalOverview();
        }

        private void DeepDiveContentScreen_UserInteraction(object sender, EventArgs e)
        {
            if (m_State == State.DeepDiveContent) TryRestartRestartTimer();
        }

        private void FinalOverviewScreen_UserInteraction(object sender, EventArgs e)
        {
            if (m_State == State.FinalOverview) TryRestartRestartTimer();
        }

        private void FinalOverviewScreen_NextClick(object sender, EventArgs e)
        {
            if (m_State == State.FinalOverview) FromFinalOverviewTo();
        }

        private async void DeepDiveContentScreen_BackClick(object sender, EventArgs e)
        {
            if (m_State == State.DeepDiveContent) await FromDeepDiveContentTo();
        }

        private void ReanswerQuestionScreen_AnswerChanged(object sender, EventArgs e)
        {
            if (m_State == State.ReanswerQuestion) TryRestartRestartTimer();
        }

        private async void ReanswerQuestionScreen_NextClick(object sender, EventArgs e)
        {
            if (m_State == State.ReanswerQuestion) await FromReanswerQuestionToNext();
        }

        private void DemographyIntroScreen_SkipClick(object sender, EventArgs e)
        {
            if (m_State == State.DemographyIntro) FromDemographyIntroToGoodbye();
        }

        private void DemographyIntroScreen_ConfirmClick(object sender, EventArgs e)
        {
            if (m_State == State.DemographyIntro) FromDemographyIntroToDemographyQuestions();
        }

        private void DemographyQuestionScreen_AnswerChanged(object sender, EventArgs e)
        {
            if (m_State == State.DemographyQuestions) TryRestartRestartTimer();
        }

        private async void DemographyQuestionScreen_NextClick(object sender, EventArgs e)
        {
            if (m_State == State.DemographyQuestions) await FromDemographyQuestionToNext();
        }

        private void GoodbyeScreen_CompletedClick(object sender, EventArgs e)
        {
            if (m_State == State.Goodbye) ShowStartup();
        }

        #endregion Ereignishandler
    }
}
