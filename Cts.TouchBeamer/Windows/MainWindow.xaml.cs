using Gemelo.Applications.Cts.TouchBeamer.Code.Settings;
using Gemelo.Applications.Cts.TouchBeamer.Controls;
using Gemelo.Components.Common.Exhibits.Settings;
using Gemelo.Components.Common.Localization;
using Gemelo.Components.Common.Settings;
using Gemelo.Components.Common.Tracing;
using Gemelo.Components.Common.Wpf.Threading;
using Gemelo.Components.Common.Wpf.UI;
using Gemelo.Components.Common.Wpf.UI.Transitions.Appearance;
using Gemelo.Components.Cts.Code.Data.Stations;
using Gemelo.Components.Cts.Code.Data.Survey;
using Gemelo.Components.Cts.Code.Data.Users;
using Gemelo.Components.Cts.Windows;
using Gemelo.Components.LidarTouch.Code;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Gemelo.Applications.Cts.TouchBeamer
{
    /// <summary>
    /// Hauptfenster CTS TouchBeamer
    /// </summary>
    public partial class MainWindow : CtsMainWindow
    {
        #region Konstanten

        private const string ConstViewportSettingsFilename = @"Settings\Viewport.json";

        private static readonly TimeSpan ConstTemporarilyLockNextButtonDuration = TimeSpan.FromMilliseconds(800.0);

        #endregion Konstanten

        #region Eingebettete Typen

        private enum State
        {
            Startup,
            WelcomeFirst,
            Privacy,
            WelcomeAgain,
            WelcomeBackUnansweredQuestions,
            Questions,
            Goodbye,
            ReturnAlreadyCompleted
        }

        #endregion Eingebettete Typen

        #region Felder und Eigenschaften

        private ViewportSettings m_Viewport;

        private State m_State;

        private List<SurveyQuestion> m_CurrentQuestions;

        private int m_CurrentQuestionIndex;

        private DelayedCall m_DelayedCallTemporarilyLockNextButton = null;

        #endregion Felder und Eigenschaften

        #region Konstruktor und Initialisierung

        public MainWindow()
        {
            InitializeComponent();
            InitializeViewport();
            if (App.IsAvailable)
            {
                BindApplicationEvents();
            }
        }

        private void InitializeViewport()
        {
            string viewportFilePath = GetViewportFilePath();
            if (!ViewportSettings.TryFromJsonFile(viewportFilePath, out m_Viewport)) m_Viewport = new();
            m_Viewport.ApplyTo(m_ViewboxMain);
        }

        private void BindApplicationEvents()
        {
            App.Current.StationDefinitionChanged += App_StationDefinitionChanged;
        }

        #endregion Konstruktor und Initialisierung

        #region Öffentliche Methoden

        public void Restart()
        {
            ShowStartup();
        }

        public override void ProcessNewUser()
        {
            base.ProcessNewUser();
            m_MessageBox.HideDirectly();
            ToWelcome();
        }

        public override void TryRestartRestartTimer()
        {
            if (!App.Current.RfidController.IsRfidDetected && m_State != State.Startup)
            {
                if (m_State >= State.Goodbye)
                {
                    RestartTimer.StartVaryingInterval(App.Current.StationDefinition.GoodbyeTimeout);
                }
                else RestartTimer.Restart();
            }
            else RestartTimer.Stop();
        }

        public void ShowNoLidarMessage()
        {
            m_BorderNoLidarMessage.Visibility = Visibility.Visible;
        }

        #endregion Öffentliche Methoden

        #region Private Methoden

        public override void ShowStartup()
        {
            base.ShowStartup();
            m_State = State.Startup;
            App.Current.ResetUser();
            m_OfflineWarningOverlay.Start();
            m_Background.ProgressText = LocalizationString.Empty;
            m_Background.FadeOutIfVisible();
            m_StartScreen.StartAnimation();
            ShowMainControl(m_StartScreen);
            TryRestartRestartTimer();
        }

        private void ToWelcome()
        {
            CtsUserInformation user = App.Current.CurrentUser;
            TraceX.WriteInformational(nameof(ToWelcome));
            m_Background.FadeInIfNotVisible();
            m_Background.ProgressText = LocalizationString.Empty;
            m_StartScreen.StopAnimation();
            m_CurrentQuestions = App.Current.StationDefinition.SurveyQuestions;
            if (user.IsNew)
            {
                ToWelcomeFirst();
            }
            else
            {
                m_CurrentQuestions = user.GetUnansweredQuestionsFor(m_CurrentQuestions);
                if (m_CurrentQuestions.Count == 0) ToReturnAlreadyCompleted();
                else if (m_CurrentQuestions.Count != App.Current.StationDefinition.SurveyQuestions.Count)
                {
                    ToWelcomeBackUnansweredQuestions();
                }
                else ToWelcomeAgain();
            }
            UpdateNextButton();
            TryRestartRestartTimer();
        }

        private void ToWelcomeFirst()
        {
            TraceX.WriteInformational(nameof(ToWelcomeFirst));
            m_State = State.WelcomeFirst;
            m_Background.ButtonText = App.Current.StationDefinition.WelcomeFirstButtonText;
            ShowMainControl(m_WelcomeFirstScreen);
            TryRestartRestartTimer();
        }

        private void ToWelcomeAgain()
        {
            TraceX.WriteInformational(nameof(ToWelcomeAgain));
            m_State = State.WelcomeAgain;
            m_Background.ButtonText = App.Current.StationDefinition.WelcomeAgainButtonText;
            ShowMainControl(m_WelcomeAgainScreen);
            TryRestartRestartTimer();
        }

        private void ToWelcomeBackUnansweredQuestions()
        {
            TraceX.WriteInformational(nameof(ToWelcomeBackUnansweredQuestions));
            m_State = State.WelcomeBackUnansweredQuestions;
            m_Background.ButtonText = App.Current.StationDefinition.WelcomeBackUnansweredQuestionsButtonText;
            ShowMainControl(m_WelcomeBackUnansweredQuestionsScreen);
            TryRestartRestartTimer();
        }

        private void FromWelcomeFirstToPrivacy()
        {
            TraceX.WriteInformational(nameof(FromWelcomeFirstToPrivacy));
            m_State = State.Privacy;
            m_Background.ButtonText = App.Current.StationDefinition.PrivacyButtonText;
            ShowMainControl(m_PrivacyScreen);
            UpdateNextButton(lockButtonTemporarily: true);
            TryRestartRestartTimer();
        }

        private void ToQuestions()
        {
            TraceX.WriteInformational(nameof(ToQuestions));
            m_State = State.Questions;
            m_CurrentQuestionIndex = 0;
            ShowQuestion();
        }

        private async Task FromQuestionToNext()
        {
            TraceX.WriteInformational(nameof(FromQuestionToNext));
            TryRestartRestartTimer();
            TouchBeamerStationDefinition stationDefinition = App.Current.StationDefinition;
            if (m_Background.DoesNextButtonLooksLikeEnabled || await m_MessageBox.Show(
                title: stationDefinition.SkipQuestionMessageText,
                text: null,
                buttonTexts: stationDefinition.SkipQuestionMessageButtonText) == 0)
            {
                if (await TrySaveAnswers())
                {
                    m_CurrentQuestionIndex++;
                    if (m_CurrentQuestionIndex < m_CurrentQuestions.Count) ShowQuestion();
                    else FromQuestionsToGoodbye();
                }
                else ReactivateQuestion();
            }
        }

        private void ShowQuestion()
        {
            QuestionScreen nextQuestionScreen = m_QuestionScreenA.Visibility == Visibility.Visible ?
                m_QuestionScreenB : m_QuestionScreenA;
            TouchBeamerStationDefinition stationDefinition = App.Current.StationDefinition;
            SurveyQuestion question = m_CurrentQuestions[m_CurrentQuestionIndex];
            nextQuestionScreen.ShowQuestion(question,
                App.Current.CurrentUser.GetLatestAnswersFor(question.QuestionID));
            nextQuestionScreen.IsHitTestVisible = true;
            m_Background.ProgressText = LocalizationString.Format(stationDefinition.QuestionProgressTextFormat,
                m_CurrentQuestionIndex + 1, m_CurrentQuestions.Count);
            m_Background.ButtonText = App.Current.StationDefinition.QuestionButtonText;
            UpdateNextButton(nextQuestionScreen, lockButtonTemporarily: true);
            ShowMainControl(nextQuestionScreen);
            TryRestartRestartTimer();
        }

        private void ReactivateQuestion()
        {
            QuestionScreen currentQuestionScreen = m_QuestionScreenA.Visibility == Visibility.Visible ?
                m_QuestionScreenA : m_QuestionScreenB;
            currentQuestionScreen.IsHitTestVisible = true;
            m_Background.ButtonText = App.Current.StationDefinition.QuestionButtonText;
            UpdateNextButton(currentQuestionScreen);
            TryRestartRestartTimer();
        }

        private void FromQuestionsToGoodbye()
        {
            TraceX.WriteInformational(nameof(FromQuestionsToGoodbye));
            m_State = State.Goodbye;
            m_Background.ProgressText = LocalizationString.Empty;
            m_Background.ButtonText = App.Current.StationDefinition.GoodbyeButtonText;
            UpdateNextButton(lockButtonTemporarily: true);
            ShowMainControl(m_GoodbyeScreen);
            TryRestartRestartTimer();
        }

        private void ToReturnAlreadyCompleted()
        {
            TraceX.WriteInformational(nameof(ToReturnAlreadyCompleted));
            m_State = State.ReturnAlreadyCompleted;
            m_Background.ProgressText = LocalizationString.Empty;
            m_Background.ButtonText = App.Current.StationDefinition.ReturnAlreadyCompletedButtonText;
            UpdateNextButton();
            ShowMainControl(m_ReturnAlreadyCompletedScreen);
            TryRestartRestartTimer();
        }

        private async Task<bool> TrySaveAnswers()
        {
            m_Background.DoesNextButtonLooksLikeEnabled = false;
            m_Background.IsNextButtonEnabled = false;
            TouchBeamerStationDefinition stationDefinition = App.Current.StationDefinition;
            m_Background.ButtonText = stationDefinition.WaitForSavingText;
            QuestionScreen currentQuestionScreen = m_QuestionScreenA.Visibility == Visibility.Visible ?
                m_QuestionScreenA : m_QuestionScreenB;
            currentQuestionScreen.IsHitTestVisible = false;
            SurveyQuestion question = m_CurrentQuestions[m_CurrentQuestionIndex];
            if (await App.Current.SaveAnswers(question.QuestionID, currentQuestionScreen.GetAnswers())) return true;
            else
            {
                await m_MessageBox.Show(
                    title: stationDefinition.SaveErrorTitle,
                    text: null,
                    buttonTexts: stationDefinition.SaveErrorButtonText);
                return false;
            }
        }

        private void UpdateNextButton(QuestionScreen questionScreen = null, bool lockButtonTemporarily = false)
        {
            m_Background.IsNextButtonEnabled = true;
            if (m_State == State.Questions)
            {
                questionScreen ??= m_QuestionScreenA.Visibility == Visibility.Visible ?
                    m_QuestionScreenA : m_QuestionScreenB;
                m_Background.DoesNextButtonLooksLikeEnabled = questionScreen.IsAnswered;
            }
            else m_Background.DoesNextButtonLooksLikeEnabled = true;
            if (lockButtonTemporarily)
            {
                m_Background.IsHitTestVisible = false;
                DelayedCall.CancelAndNull(ref m_DelayedCallTemporarilyLockNextButton);
                m_DelayedCallTemporarilyLockNextButton =
                    DelayedCall.Start(ConstTemporarilyLockNextButtonDuration, () =>
                {
                    m_Background.IsHitTestVisible = true;
                });
            }
        }

        protected override bool IsRestartAllowed() => false;

        protected override async void RestartTimer_Expired(object sender, EventArgs e)
        {
            base.RestartTimer_Expired(sender, e);
            if (m_State < State.Goodbye)
            {
                RestartTimer.Stop();
                TouchBeamerStationDefinition stationDefinition = App.Current.StationDefinition;
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
            else ShowStartup();
        }

        private string GetViewportFilePath()
        {
            return Directories.GetPathInDataDirectory(ConstViewportSettingsFilename);
        }

        private void UpdateViewportHorizontal(bool affectsLeft, int delta)
        {
            if (affectsLeft) m_Viewport.MarginLeft += delta;
            else m_Viewport.MarginRight += delta;
            ApplyAndSaveViewport();
        }

        private void UpdateViewportVertical(bool affectsTop, int delta)
        {
            if (affectsTop) m_Viewport.MarginTop += delta;
            else m_Viewport.MarginBottom += delta;
            ApplyAndSaveViewport();
        }

        private void ApplyAndSaveViewport()
        {
            m_Viewport.ApplyTo(m_ViewboxMain);
            string viewportFilePath = GetViewportFilePath();
            Directory.CreateDirectory(Path.GetDirectoryName(viewportFilePath));
            m_Viewport.ToJsonFile(viewportFilePath);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.F4) m_BorderVisualization.ToggleVisibility();
            else if (e.Key == Key.F5)
            {
                m_LidarSensorVisualization.ToggleVisibility();
                if (m_LidarSensorVisualization.Visibility == Visibility.Collapsed)
                {
                    m_LidarTouchCalibrationControl.Visibility = Visibility.Collapsed;
                    m_LidarSensorVisualization.Sensor = App.Current.LidarSensor;
                }
            }
            else if (e.Key == Key.F6)
            {
                m_LidarTouchCalibrationControl.ToggleVisibility();
                if (m_LidarTouchCalibrationControl.Visibility == Visibility.Visible)
                {
                    m_LidarSensorVisualization.Visibility = Visibility.Collapsed;
                    m_LidarTouchVisualization.Visibility = Visibility.Collapsed;
                    m_LidarTouchCalibrationControl.Sensor = App.Current.LidarSensor;
                    m_LidarTouchCalibrationControl.Start();
                }
            }
            else if (e.Key == Key.F7)
            {
                m_LidarTouchVisualization.ToggleVisibility();
            }
            //else if (e.Key == Key.N)
            //{
            //    m_LidarTouchCalibrationControl.Next();
            //}
            else if ((e.Key == Key.Left || e.Key == Key.Right) &&
                m_BorderVisualization.Visibility == Visibility.Visible)
            {
                int delta = e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) ? -1 : 1;
                delta *= e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift) ? 10 : 1;
                bool affectsLeft = (e.Key == Key.Right && delta > 0) || (e.Key == Key.Left && delta < 0);
                UpdateViewportHorizontal(affectsLeft: affectsLeft, delta: delta);
            }
            else if ((e.Key == Key.Up || e.Key == Key.Down) && m_BorderVisualization.Visibility == Visibility.Visible)
            {
                int delta = e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) ? -1 : 1;
                delta *= e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift) ? 10 : 1;
                bool affectsTop = (e.Key == Key.Down && delta > 0) || (e.Key == Key.Up && delta < 0); ;
                UpdateViewportVertical(affectsTop: affectsTop, delta: delta);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            App.Current.LidarSensor?.Stop();
        }

        #endregion Private Methoden

        #region Ereignishandler

        private void App_StationDefinitionChanged(object sender, EventArgs e)
        {
            TouchBeamerStationDefinition stationDefinition = App.Current.StationDefinition;
            m_StartScreen.Title = stationDefinition.Title;
            m_StartScreen.RfidHintText = stationDefinition.RfidHintText;
            m_StartScreen.RfidHintPosition =
                App.Current.OverrideRfidHintPosition ?? stationDefinition.RfidHintPosition;
            m_WelcomeFirstScreen.Headline = stationDefinition.WelcomeFirstHeadline;
            m_WelcomeFirstScreen.Text = stationDefinition.WelcomeFirstText;
            m_WelcomeAgainScreen.Headline = stationDefinition.WelcomeAgainHeadline;
            m_WelcomeAgainScreen.Text = stationDefinition.WelcomeAgainText;
            m_WelcomeBackUnansweredQuestionsScreen.Headline = stationDefinition.WelcomeBackUnansweredQuestionsHeadline;
            m_WelcomeBackUnansweredQuestionsScreen.Text = stationDefinition.WelcomeBackUnansweredQuestionsText;
            m_PrivacyScreen.Headline = stationDefinition.PrivacyHeadline;
            m_PrivacyScreen.Text = stationDefinition.PrivacyText;
            m_PrivacyScreen.Hint = stationDefinition.PrivacyConfirmationText;
            m_GoodbyeScreen.Headline = stationDefinition.GoodbyeHeadline;
            m_GoodbyeScreen.Text = stationDefinition.GoodbyeText;
            m_ReturnAlreadyCompletedScreen.Headline = stationDefinition.ReturnAlreadyCompletedHeadline;
            m_ReturnAlreadyCompletedScreen.Text = stationDefinition.ReturnAlreadyCompletedText;
            RestartTimer.Interval = stationDefinition.RestartTimeout;
        }

        private async void Background_NextClick(object sender, EventArgs e)
        {
            switch (m_State)
            {
                case State.WelcomeFirst:
                    FromWelcomeFirstToPrivacy();
                    break;
                case State.Privacy:
                case State.WelcomeAgain:
                case State.WelcomeBackUnansweredQuestions:
                    ToQuestions();
                    break;
                case State.Questions:
                    await FromQuestionToNext();
                    break;
                case State.Goodbye:
                case State.ReturnAlreadyCompleted:
                    ShowStartup();
                    break;
            }

        }

        private void QuestionScreen_AnswerChanged(object sender, EventArgs e)
        {
            UpdateNextButton(lockButtonTemporarily: true);
            TryRestartRestartTimer();
        }

        private void LidarTouchCalibrationControl_Completed(object sender, EventArgs e)
        {
            m_LidarTouchCalibrationControl.Visibility = Visibility.Collapsed;
            LidarTouchDevice.SetCalibrationFor(App.Current.LidarSensor, 
                m_LidarTouchCalibrationControl.Calibration);
            App.Current.SaveLidarSettingsAndCalibration();
        }

        #endregion Ereignishandler
    }
}
