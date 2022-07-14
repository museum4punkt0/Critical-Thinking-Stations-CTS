using Gemelo.Components.Common.Collections;
using Gemelo.Components.Common.Wpf.Localization;
using Gemelo.Components.Cts.Code.Data.DeepDive;
using Gemelo.Components.Cts.Code.Data.Stations;
using Gemelo.Components.Cts.Code.Data.Survey;
using Gemelo.Components.Cts.Code.Media;
using Gemelo.Components.Cts.Controls.Media;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gemelo.Applications.Cts.DeepDive.Controls
{
    /// <summary>
    /// Zeigt die Vertiefungsinhalte an
    /// </summary>
    public partial class DeepDiveContentScreen : UserControl
    {
        #region Konstanten

        private const string ConstTextStyleFormat = "TextStyleDeepDiveContent{0}";

        #endregion Konstanten

        private Style m_StyleMediaPlayer;
        private Style m_StyleMediaPlayerImageOnly;

        public DeepDiveContentDefinition DeepDiveContent { get; private set; }

        public SurveyQuestion Question { get; private set; }

        public event EventHandler UserInteraction;

        protected void OnUserInteraction()
        {
            UserInteraction?.Invoke(this, null);
        }

        public event EventHandler BackClick;

        protected void OnBackClick()
        {
            BackClick?.Invoke(this, null);
        }

        public DeepDiveContentScreen()
        {
            InitializeComponent();
            InitializeStyles();
            if (App.IsAvailable) App.Current.StationDefinitionChanged += App_StationDefinitionChanged;
        }

        private void InitializeStyles()
        {
            m_StyleMediaPlayer = (Style)FindResource("StyleMediaPlayer");
            m_StyleMediaPlayerImageOnly = (Style)FindResource("StyleMediaPlayerImageOnly");
        }

        public void ShowFor(SurveyQuestion question)
        {
            DeepDiveContent = App.Current.StationDefinition.DeepDiveContents[question.QuestionID];
            Question = question;
            m_TxtQuestion.SetLocalizedText(question.QuestionText);
            m_StackContentElements.Children.Clear();
            foreach (DeepDiveContentElement contentElement in DeepDiveContent.ContentElements)
            {
                bool show = contentElement.AnswerFilters == null || contentElement.AnswerFilters.Count == 0;
                if (!show)
                {
                    var userAnswers = App.Current.CurrentUser.GetLatestAnswersFor(question.QuestionID);
                    if (!(userAnswers?.AnswerIDs.IsNullOrEmpty() == true))
                    {
                        show = contentElement.AnswerFilters.Intersect(userAnswers.AnswerIDs).Any();
                    }
                }
                if (show) m_StackContentElements.Children.Add(CreateContentElementFor(contentElement));
            }
            m_ScrollViewerMain.ScrollToHome();
        }

        public bool IsMediaPlaying()
        {
            foreach (MediaPlayer mediaPlayer in m_StackContentElements.Children.Cast<UIElement>()
                .Where(element => element is MediaPlayer))
            {
                if (mediaPlayer.IsPlaying) return true;
            }
            return false;
        }

        private UIElement CreateContentElementFor(DeepDiveContentElement contentElement)
        {
            return contentElement.ElementType switch
            {
                DeepDiveContentElementType.Media => CreateMediaElementFor(contentElement),
                _ => CreateTextElementFor(contentElement)
            };
        }

        private UIElement CreateMediaElementFor(DeepDiveContentElement contentElement)
        {
            MediaType mediaType = MediaFileHelper.GetTypeFromFilename(contentElement.Filename);
            MediaPlayer result = new MediaPlayer
            {
                Style = mediaType == MediaType.Image ? m_StyleMediaPlayerImageOnly : m_StyleMediaPlayer
            };
            result.PlayClick += MediaPlayer_PlayClick;
            result.PauseClick += MediaPlayer_PauseClick;
            result.SeekUpdate += MediaPlayer_SeekUpdate;
            result.Completed += MediaPlayer_Completed;
            result.PrepareFor(contentElement.Filename, useVideoPoster: true, autoAdjustHeight: true);
            return result;
        }

        private UIElement CreateTextElementFor(DeepDiveContentElement contentElement)
        {
            TextBlock result = new TextBlock
            {
                Style = (Style)FindResource(string.Format(ConstTextStyleFormat, contentElement.ElementType))
            };
            if (contentElement.Width >= 0) result.Width = contentElement.Width;
            if (contentElement.TopMargin != int.MinValue)
            {
                Thickness margin = result.Margin;
                margin.Top = contentElement.TopMargin;
                result.Margin = margin;
            }
            result.SetLocalizedText(contentElement.Text);
            return result;
        }

        private void PauseAllMediaPlayersExceptFor(MediaPlayer exceptFor)
        {
            m_StackContentElements.Children
                .Cast<UIElement>()
                .Where(element => element is MediaPlayer && element != exceptFor)
                .Cast<MediaPlayer>()
                .ForEach(mediaPlayer => mediaPlayer.Pause());
        }

        private void App_StationDefinitionChanged(object sender, EventArgs e)
        {
            DeepDiveStationDefinition stationDefinition = App.Current.StationDefinition;
            m_TxtHeadline.SetLocalizedText(stationDefinition.DeepDiveContentHeadline);
            m_Footer.NextButtonText = stationDefinition.DeepDiveContentButtonText;
        }

        private void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            OnUserInteraction();
        }

        private void MediaPlayer_PlayClick(object sender, EventArgs e)
        {
            MediaPlayer typedSender = (MediaPlayer)sender;
            PauseAllMediaPlayersExceptFor(typedSender);
            OnUserInteraction();
        }

        private void MediaPlayer_PauseClick(object sender, EventArgs e)
        {
            OnUserInteraction();
        }

        private void MediaPlayer_SeekUpdate(object sender, EventArgs e)
        {
            OnUserInteraction();
        }

        private void MediaPlayer_Completed(object sender, EventArgs e)
        {
            OnUserInteraction();
        }

        private void Footer_NextClick(object sender, EventArgs e)
        {
            OnBackClick();
        }
    }
}
