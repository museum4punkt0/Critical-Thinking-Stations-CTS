using Gemelo.Components.Common.Localization;
using Gemelo.Components.Common.Wpf.Text;
using Gemelo.Components.Common.Wpf.UI;
using Gemelo.Components.Cts.Code.Data.Layout;
using System.Windows;
using System.Windows.Controls;

namespace Gemelo.Components.Cts.Controls.Common
{
    /// <summary>
    /// Startbildschirm für die interaktiven CTS-Stationen
    /// </summary>
    public partial class CtsStartScreen : UserControl
    {
        private LocalizationString m_Title = new LocalizationString();

        public LocalizationString Title
        {
            get => m_Title;
            set
            {
                m_Title = value;
                m_TxtTitleDe.SetFormattedText(value.GetFor(Languages.German));
                m_TxtTitleEn.SetFormattedText(value.GetFor(Languages.English));
            }
        }

        private LocalizationString m_RfidHintText = new LocalizationString();

        public LocalizationString RfidHintText
        {
            get => m_RfidHintText;
            set
            {
                m_RfidHintText = value;
                m_TxtRfidHintDe.SetFormattedText(value.GetFor(Languages.German));
                m_TxtRfidHintEn.SetFormattedText(value.GetFor(Languages.English, useFallback: false));
            }
        }

        private RfidHintPosition m_RfidHintPosition;

        public RfidHintPosition RfidHintPosition
        {
            get => m_RfidHintPosition;
            set => SetRfidHintPosition(value);
        }

        public bool IsRfidHandTopRightVisible
        {
            get => m_ViewboxRfidHandTopRight.Visibility == Visibility.Visible;
            set => m_ViewboxRfidHandTopRight.Visibility = value.ToVisibility();
        }

        public CtsStartScreen()
        {
            InitializeComponent();
        }


        public void StartAnimation()
        {
            m_CtsLogo.StartAnimation();
        }

        public void StopAnimation(bool showFirst = true)
        {
            m_CtsLogo.StopAnimation(showFirst);
        }

        private void SetRfidHintPosition(RfidHintPosition value)
        {
            m_RfidHintPosition = value;
            m_DahLogo.HorizontalAlignment =
                value == RfidHintPosition.Left || value == RfidHintPosition.BottomLeft ?
                HorizontalAlignment.Right : HorizontalAlignment.Left;
            m_StackRfidHint.HorizontalAlignment =
                value == RfidHintPosition.Left || value == RfidHintPosition.BottomLeft ?
                HorizontalAlignment.Left : HorizontalAlignment.Right;
            m_PathArrowLeft.Visibility = (value == RfidHintPosition.Left).ToVisibility();
            m_PathArrowRight.Visibility = (value == RfidHintPosition.Right).ToVisibility();
            m_PathArrowBottom.Visibility =
                (value == RfidHintPosition.BottomLeft || value == RfidHintPosition.BottomRight).ToVisibility();
            m_PathArrowTop.Visibility = (value == RfidHintPosition.TopRight).ToVisibility();
            m_TxtRfidHintDe.TextAlignment = m_TxtRfidHintEn.TextAlignment = value switch
            {
                RfidHintPosition.Left => TextAlignment.Left,
                RfidHintPosition.Right => TextAlignment.Right,
                _ => TextAlignment.Center
            };
        }
    }
}
