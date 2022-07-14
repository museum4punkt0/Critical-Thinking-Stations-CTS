using Gemelo.Components.Common.Localization;
using Gemelo.Components.Common.Wpf.Localization;
using Gemelo.Components.Common.Wpf.UI.Transitions.Appearance;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Gemelo.Applications.Cts.DeepDive.Controls.Common
{
    /// <summary>
    /// Zeigt die Hintergrundelement mit optionalem Weiter-Button und optionalem Logo an
    /// </summary>
    public partial class DeepDiveBackground : UserControl
    { 
        private Storyboard m_StoryToDefaultColor;
        private Storyboard m_StoryToDeepDiveColor;

        private LocalizationString m_ProgressText;

        public LocalizationString ProgressText
        {
            get => m_ProgressText;
            set
            {
                m_ProgressText = value;
                m_TxtProgress.SetLocalizedText(value);
            }
        }

        public DeepDiveBackground()
        {
            InitializeComponent();
            InitializeStoryboards();
        }

        private void InitializeStoryboards()
        {
            m_StoryToDefaultColor = (Storyboard)FindResource("StoryToDefaultColor");
            m_StoryToDeepDiveColor = (Storyboard)FindResource("StoryToDeepDiveColor");
        }

        public void ToDefaultColor()
        {
            m_StoryToDefaultColor.Begin(this, true);
        }

        public void ToDeepDiveColor()
        {
            m_StoryToDeepDiveColor.Begin(this, true);
        }

        public void FadeOutElements()
        {
            m_GridElements.FadeOutIfVisible();
        }

        public void FadeInElements()
        {
            m_GridElements.FadeInIfNotVisible();
        }
    }
}

