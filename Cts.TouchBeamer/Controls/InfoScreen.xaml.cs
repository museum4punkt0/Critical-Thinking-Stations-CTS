using Gemelo.Components.Common.Localization;
using Gemelo.Components.Common.Wpf.Localization;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Gemelo.Applications.Cts.TouchBeamer.Controls
{
    /// <summary>
    /// Zeigt einen Textbildschirm an.
    /// </summary>
    public partial class InfoScreen : UserControl
    {
        private LocalizationString m_Headline;

        public LocalizationString Headline
        {
            get => m_Headline;
            set
            {
                m_Headline = value;
                m_TxtHeadline.SetLocalizedText(value);
            }
        }

        private LocalizationString m_Text;

        public LocalizationString Text
        {
            get => m_Text;
            set
            {
                m_Text = value;
                m_TxtText.SetLocalizedText(value);
            }
        }

        private LocalizationString m_Hint;

        public LocalizationString Hint
        {
            get => m_Hint;
            set
            {
                m_Hint = value;
                m_TxtHint.SetLocalizedText(value);
            }
        }

        public InfoScreen()
        {
            InitializeComponent();
        }
    }
}
