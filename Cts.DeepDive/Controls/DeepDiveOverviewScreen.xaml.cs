using Gemelo.Applications.Cts.DeepDive.Code.Events;
using Gemelo.Applications.Cts.DeepDive.Controls.Common;
using Gemelo.Components.Common.Wpf.Localization;
using Gemelo.Components.Cts.Code.Data.Stations;
using Gemelo.Components.Cts.Code.Data.Survey;
using Gemelo.Components.Cts.Code.Data.Users;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Gemelo.Applications.Cts.DeepDive.Controls
{
    /// <summary>
    /// Zeigt die Übersicht mit DeepDive-Fragen
    /// </summary>
    public partial class DeepDiveOverviewScreen : UserControl
    {
        #region Felder und Eigenschaften

        private bool m_IsExpandedInstruction;
        private double m_InstructionTextExpandedHeight = 500.0;

        private Style m_StyleQuestionPreview;

        private Storyboard m_StoryInstructionExpand;
        private DoubleKeyFrame m_StoryInstructionExpandMaxHeightKeyFrame;
        private Storyboard m_StoryInstructionCollapse;

        #endregion Felder und Eigenschaften

        #region Ereignisse

        public event EventHandler<SurveyQuestionEventArgs> QuestionClick;

        protected void OnQuestionClick(SurveyQuestion question)
        {
            QuestionClick?.Invoke(this, new SurveyQuestionEventArgs(question));
        }

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

        public DeepDiveOverviewScreen()
        {
            InitializeComponent();
            InitializeStyles();
            InitializeStoryboards();
            SetIsInstructionExpandedDirectly(true);
            if (App.IsAvailable) App.Current.StationDefinitionChanged += App_StationDefinitionChanged;
        }

        private void InitializeStyles()
        {
            m_StyleQuestionPreview = (Style)FindResource("StyleQuestionPreview");
        }

        private void InitializeStoryboards()
        {
            m_StoryInstructionExpand = (Storyboard)FindResource("StoryInstructionExpand");
            m_StoryInstructionExpandMaxHeightKeyFrame = 
                ((DoubleAnimationUsingKeyFrames)m_StoryInstructionExpand.Children[0]).KeyFrames[0];
            m_StoryInstructionCollapse = (Storyboard)FindResource("StoryInstructionCollapse");
        }

        public void SetIsInstructionExpandedDirectly(bool isExpanded)
        {
            m_IsExpandedInstruction = isExpanded;
            m_StoryInstructionExpand.Stop(this);
            m_StoryInstructionCollapse.Stop(this);
            if (isExpanded)
            {
                m_ScrollMain.ScrollToHome();
                m_BorderInstructionText.MaxHeight = m_InstructionTextExpandedHeight;
                m_RotateExpand.Angle = 0.0;
            }
            else
            {
                m_BorderInstructionText.MaxHeight = 0;
                m_RotateExpand.Angle = -90.0;
            }
        }

        public void ShowFor(CtsUserInformation currentUser)
        {
            m_WrapPanelPreviews.Children.Clear();
            foreach (SurveyQuestion question in App.Current.StationDefinition.GetDeepDiveSurveyQuestions())
            {
                AddPreviewFor(question, currentUser);
            }
            m_ScrollMain.ScrollToVerticalOffset(0.0);
        }

        private void AddPreviewFor(SurveyQuestion question, CtsUserInformation user)
        {
            QuestionPreview preview = new QuestionPreview
            {
                Style = m_StyleQuestionPreview,
                IsForDeepDive = true,
                HasDeepDiveVisited = user.HasDeepDiveBeenVisited(question.QuestionID),
                Question = question,
                User = user
            };
            preview.Click += QuestionPreview_Click;
            m_WrapPanelPreviews.Children.Add(preview);
        }

        private void ToggleIsInstructionExpanded()
        {
            if (m_IsExpandedInstruction) CollapseInstructionTextAnimated();
            else ExpandInstructionTextAnimated();
        }

        private void ExpandInstructionTextAnimated()
        {
            m_IsExpandedInstruction = true;
            m_StoryInstructionExpand.Begin(this, true);
        }

        private void CollapseInstructionTextAnimated()
        {
            m_IsExpandedInstruction = false;
            m_StoryInstructionCollapse.Begin(this, true);
        }

        private void App_StationDefinitionChanged(object sender, EventArgs e)
        {
            DeepDiveStationDefinition stationDefinition = App.Current.StationDefinition;
            m_TxtHeadline.SetLocalizedText(stationDefinition.DeepDiveOverviewHeadline);
            m_TxtText.SetLocalizedText(stationDefinition.DeepDiveOverviewText);
            m_TxtInstructionTitle.SetLocalizedText(stationDefinition.DeepDiveInstructionHeadline);
            m_TxtInstructionText.SetLocalizedText(stationDefinition.DeepDiveInstructionText);
            m_Footer.NextButtonText = stationDefinition.DeepDiveOverviewButtonText;
            UpdateInstructionTextMaxHeight();
            SetIsInstructionExpandedDirectly(m_IsExpandedInstruction);
        }

        private void UpdateInstructionTextMaxHeight()
        {
            Size size = new Size(m_TxtInstructionText.Width, 10000.0);
            m_TxtInstructionText.Measure(size);
            m_InstructionTextExpandedHeight = m_TxtInstructionText.DesiredSize.Height;
            m_StoryInstructionExpandMaxHeightKeyFrame.Value = m_InstructionTextExpandedHeight;
        }

        private void StackInstructionTitle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.StylusDevice == null) ToggleIsInstructionExpanded();
        }

        private void StackInstructionTitle_TouchDown(object sender, TouchEventArgs e)
        {
            ToggleIsInstructionExpanded();
        }

        private void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            OnUserInteraction();
        }

        private void QuestionPreview_Click(object sender, EventArgs e)
        {
            QuestionPreview typedSender = (QuestionPreview)sender;
            OnQuestionClick(typedSender.Question);
        }

        private void Footer_NextClick(object sender, EventArgs e)
        {
            OnNextClick();
        }
    }
}
