using Gemelo.Components.Common.Localization;
using Gemelo.Components.Common.Wpf.Localization;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Gemelo.Applications.Cts.TouchBeamer.Controls
{
    /// <summary>
    /// Zeigt die Hintergrundelement mit Weiter-Button an
    /// </summary>
    public partial class TouchBeamerBackground : UserControl
    {
        private Style m_StyleNextButtonDefault;
        private Style m_StyleNextButtonDisabled;

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

        private LocalizationString m_ButtonText;

        public LocalizationString ButtonText
        {
            get => m_ButtonText;
            set
            {
                m_ButtonText = value;
                m_TxtButton.SetLocalizedText(value);
            }
        }

        public bool IsNextButtonEnabled
        {
            get => m_BtnNext.IsEnabled;
            set => m_BtnNext.IsEnabled = value;
        }

        private bool m_DoesNextButtonLooksLikeEnabled = true;

        public bool DoesNextButtonLooksLikeEnabled
        {
            get => m_DoesNextButtonLooksLikeEnabled;
            set => SetDoesNextButtonLooksLikeEnabled(value);
        }
        
        public event EventHandler NextClick;

        protected void OnNextClick()
        {
            NextClick?.Invoke(this, null);
        }

        public TouchBeamerBackground()
        {
            InitializeComponent();
            InitializeStyles();
        }

        private void InitializeStyles()
        {
            m_StyleNextButtonDefault = (Style)FindResource("StyleNextButtonDefault");
            m_StyleNextButtonDisabled = (Style)FindResource("StyleNextButtonDisabled");
        }

        private void SetDoesNextButtonLooksLikeEnabled(bool value)
        {
            m_DoesNextButtonLooksLikeEnabled = value;
            m_BtnNext.Style = value ? m_StyleNextButtonDefault : m_StyleNextButtonDisabled;
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            OnNextClick();
        }
    }
}
