using Gemelo.Components.Common.Tracing;
using Gemelo.Components.Common.Wpf.Media.ImageSequences;
using Gemelo.Components.Common.Wpf.UI;
using Gemelo.Components.Cts.Applications;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Gemelo.Components.Cts.Controls.Logos
{
    /// <summary>
    /// Zeigt das CTS Logo an
    /// </summary>
    public partial class CtsLogo : UserControl
    {
        private const string ConstAnimationResourcePathFormat = 
            "Resources/Images/Sequences/CTS_Logo_anim_{0:00000}.png";

        private const int ConstAnimationFrameCount = 181;

        public const double ConstFramesPerSecond = 60.0;
        public const double ConstMillisecondsPerFrame = 1000.0 / ConstFramesPerSecond;

        public const double ConstPauseAfterPass = 2000.0;

        private static readonly PngImageSequence s_ImageSequence;

        private double m_AnimationStartTime = double.MaxValue;

        private WriteableBitmap m_BitmapForSequence;

        private int m_LastIndex = -1;

        public bool UseVectors
        {
            get => m_PathLogo.Visibility == Visibility.Visible;
            set => m_PathLogo.Visibility = value.ToVisibility();
        }

        static CtsLogo()
        {
            s_ImageSequence = PngImageSequence.CreateFromResources(
                name: "cts-logo",
                resourcePathFornat: ConstAnimationResourcePathFormat,
                count: ConstAnimationFrameCount,
                assembly: typeof(CtsLogo).Assembly);
        }

        public CtsLogo()
        {
            InitializeComponent();
            ShowImage(0);
            if (CtsApplication.IsAvailable) CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        public void StartAnimation()
        {
            m_AnimationStartTime = CtsApplication.Current.TimeInMs;
        }

        public void StopAnimation(bool showFirst = true)
        {
            m_AnimationStartTime = double.MaxValue;
            if (showFirst) ShowImage(0);
        }

        private void UpdateAnimation(double elapsed)
        {
            int index = (int)(elapsed / ConstMillisecondsPerFrame);
            if (index < s_ImageSequence.Count) ShowImage(index);
            else
            {
                m_AnimationStartTime = CtsApplication.Current.TimeInMs + ConstPauseAfterPass;
                ShowImage(s_ImageSequence.Count - 1);
            }
        }

        private void ShowImage(int index)
        {
            if (m_LastIndex != index && s_ImageSequence != null && index >= 0 && index < s_ImageSequence.Count)
            {
                try
                {
                    m_LastIndex = index;
                    s_ImageSequence.DecodeImageToWriteableBitmap(index, ref m_BitmapForSequence);
                    m_ImageLogo.Source = m_BitmapForSequence;
                }
                catch (Exception exception)
                {
                    TraceX.WriteHandledException(
                        message: $"Handled exception while show image sequence image at index" +
                            $" {index}: {exception.Message}",
                        category: nameof(CtsLogo),
                        exception: exception);
                }
            }
        }

        private void CompositionTarget_Rendering(object sender, System.EventArgs e)
        {
            double time = CtsApplication.Current.TimeInMs;
            if (IsVisible && time >= m_AnimationStartTime) UpdateAnimation(time - m_AnimationStartTime);
        }
    }
}
