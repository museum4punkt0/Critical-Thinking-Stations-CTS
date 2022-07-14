using Gemelo.Components.Common.Localization;
using Gemelo.Components.Common.Wpf.Localization;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Gemelo.Applications.Cts.DeepDive.Controls.Common
{
    /// <summary>
    /// Zeigt den Footer für die DeepDive-Station mit DAH-Logo und Weiter-Schaltfläche(n) an
    /// </summary>
    public partial class DeepDiveFooter : UserControl
    {
        #region Felder und Eigenschaften

        private Style m_StyleNextButtonDefault;
        private Style m_StyleNextButtonDeepDive;
        private Style m_StyleNextButtonDefaultDisabled;
        private Style m_StyleNextButtonDeepDiveDisabled;

        private bool m_UseDeepDiveColors = false;

        public bool UseDeepDiveColors
        {
            get => m_UseDeepDiveColors;
            set
            {
                m_UseDeepDiveColors = value;
                UpdateNextButtonStyle();
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
            set
            {
                m_DoesNextButtonLooksLikeEnabled = value;
                UpdateNextButtonStyle();
            }
        }

        private LocalizationString m_NextButtonText;

        public LocalizationString NextButtonText
        {
            get => m_NextButtonText;
            set => SetNextButtonText(value);
        }

        private LocalizationString m_SkipButtonText;

        public LocalizationString SkipButtonText
        {
            get => m_SkipButtonText;
            set => SetSkipButtonText(value);
        }

        private LocalizationString m_HintText;

        public LocalizationString HintText
        {
            get => m_HintText;
            set => SetHintText(value);
        }

        #endregion Felder und Eigenschaften

        #region Ereignisse

        public event EventHandler NextClick;

        protected void OnNextClick()
        {
            NextClick?.Invoke(this, null);
        }

        public event EventHandler SkipClick;

        protected void OnSkipClick()
        {
            SkipClick?.Invoke(this, null);
        }

        #endregion Ereignisse

        #region Konstruktor und Initialisierung

        public DeepDiveFooter()
        {
            InitializeComponent();
            InitializeStyles();
            UpdateNextButtonStyle();
        }

        private void InitializeStyles()
        {
            m_StyleNextButtonDefault = (Style)FindResource("StyleNextButtonDefault");
            m_StyleNextButtonDeepDive = (Style)FindResource("StyleNextButtonDeepDive");
            m_StyleNextButtonDefaultDisabled = (Style)FindResource("StyleNextButtonDefaultDisabled");
            m_StyleNextButtonDeepDiveDisabled = (Style)FindResource("StyleNextButtonDeepDiveDisabled");
        }

        #endregion Konstruktor und Initialisierung

        #region Private Methoden

        private void UpdateNextButtonStyle()
        {
            if (m_DoesNextButtonLooksLikeEnabled)
            {
                m_BtnNext.Style = m_UseDeepDiveColors ? m_StyleNextButtonDeepDive : m_StyleNextButtonDefault;
            }
            else
            {
                m_BtnNext.Style = m_UseDeepDiveColors ?
                    m_StyleNextButtonDeepDiveDisabled : m_StyleNextButtonDefaultDisabled;
            }
            m_BtnSkip.Style = m_UseDeepDiveColors ? 
                m_StyleNextButtonDeepDiveDisabled : m_StyleNextButtonDefaultDisabled;
        }

        private void SetNextButtonText(LocalizationString value)
        {
            m_NextButtonText = value;
            m_TxtNextButton.SetLocalizedText(value);
        }

        private void SetSkipButtonText(LocalizationString value)
        {
            m_SkipButtonText = value;
            if (LocalizationString.IsNotNullOrEmpty(value))
            {
                m_BtnSkip.Visibility = Visibility.Visible;
                m_TxtSkipButton.SetLocalizedText(value);
            }
            else m_BtnSkip.Visibility = Visibility.Collapsed;
        }

        private void SetHintText(LocalizationString value)
        {
            m_HintText = value;
            if (LocalizationString.IsNotNullOrEmpty(value))
            {
                m_TxtHint.Visibility = Visibility.Visible;
                m_TxtHint.SetLocalizedText(value);
            }
            else m_TxtHint.Visibility = Visibility.Collapsed;
        }

        #endregion Private Methoden

        #region Ereignishandler

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            OnNextClick();
        }

        private void BtnSkip_Click(object sender, RoutedEventArgs e)
        {
            OnSkipClick();
        }

        #endregion Ereignishandler
    }
}
