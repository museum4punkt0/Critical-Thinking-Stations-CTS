using Gemelo.Components.Common.Base;
using Gemelo.Components.Common.Collections;
using Gemelo.Components.Common.Localization;
using Gemelo.Components.Common.Wpf.Controls.Buttons;
using Gemelo.Components.Common.Wpf.Localization;
using Gemelo.Components.Common.Wpf.UI;
using Gemelo.Components.Common.Wpf.UI.Transitions;
using Gemelo.Components.Common.Wpf.UI.Transitions.Appearance;
using Gemelo.Components.Cts.Applications;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Gemelo.Components.Cts.Controls.Common
{
    /// <summary>
    /// MessageBox
    /// </summary>
    public partial class MessageBox : UserControl
    {
        #region Konstanten

        private static readonly Brush DefaultMessageBoxBackground =
            new SolidColorBrush(Color.FromRgb(r: 0, g: 29, 38));
        private static readonly Brush DefaultMessageBoxBorderBrush =
            new SolidColorBrush(Color.FromRgb(25, 63, 65)); //#193F41
        private static readonly Thickness DefaultMessageBoxBorderThickness = new Thickness(2.0);
        private static readonly Brush DefaultButtonBackground =
            new SolidColorBrush(Color.FromRgb(34, 88, 90)); //#22585A
        private static readonly Brush DefaultButtonForeground = Brushes.White;

        #endregion Konstanten

        #region Felder und Eigenschaften

        private TaskCompletionSource<int> m_MessageBoxCompletionSource;

        public Thickness ButtonsSensitiveMargin
        {
            get => m_BtnClose.SensitiveMargin;
            set
            {
                m_BtnClose.SensitiveMargin = value;
                m_StackButtons.Children.Cast<BorderedButton>().ForEach(button => button.SensitiveMargin = value);
            }
        }

        private DispatcherTimer m_TimeoutUpdateTimer;
        private LocalizationString m_TimeoutTitleFormat;
        private LocalizationString m_TimeoutTextFormat;
        private TimeSpan m_TimeoutEndTime;

        #endregion Felder und Eigenschaften

        #region DependencyProperties

        public static readonly DependencyProperty MessageBoxBackgroundProperty = DependencyProperty.Register(
           "MessageBoxBackground", typeof(Brush), typeof(MessageBox),
           new PropertyMetadata(DefaultMessageBoxBackground));

        [Description("Hintergrund für die MessageBox")]
        [Category("gemelo")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Brush MessageBoxBackground
        {
            get => (Brush)GetValue(MessageBoxBackgroundProperty);
            set => SetValue(MessageBoxBackgroundProperty, value);
        }

        public static readonly DependencyProperty MessageBoxBorderBrushProperty = DependencyProperty.Register(
           "MessageBoxBorderBrush", typeof(Brush), typeof(MessageBox),
           new PropertyMetadata(DefaultMessageBoxBorderBrush));

        [Description("Farbe für die Umrandung der MessageBox")]
        [Category("gemelo")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Brush MessageBoxBorderBrush
        {
            get => (Brush)GetValue(MessageBoxBorderBrushProperty);
            set => SetValue(MessageBoxBorderBrushProperty, value);
        }

        public static readonly DependencyProperty MessageBoxBorderThicknessProperty = DependencyProperty.Register(
           "MessageBoxBorderThickness", typeof(Thickness), typeof(MessageBox),
           new PropertyMetadata(DefaultMessageBoxBorderThickness));

        [Description("Breite der Umrandung der MessageBox")]
        [Category("gemelo")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Thickness MessageBoxBorderThickness
        {
            get => (Thickness)GetValue(MessageBoxBorderThicknessProperty);
            set => SetValue(MessageBoxBorderThicknessProperty, value);
        }

        public static readonly DependencyProperty ButtonBackgroundProperty = DependencyProperty.Register(
           "ButtonBackground", typeof(Brush), typeof(MessageBox),
           new PropertyMetadata(DefaultButtonBackground));

        [Description("Farbe für die Umrandung der MessageBox")]
        [Category("gemelo")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Brush ButtonBackground
        {
            get => (Brush)GetValue(ButtonBackgroundProperty);
            set => SetValue(ButtonBackgroundProperty, value);
        }

        public static readonly DependencyProperty ButtonForegroundProperty = DependencyProperty.Register(
           "ButtonForeground", typeof(Brush), typeof(MessageBox),
           new PropertyMetadata(DefaultButtonForeground));

        [Description("Farbe für die Umrandung der MessageBox")]
        [Category("gemelo")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Brush ButtonForeground
        {
            get => (Brush)GetValue(ButtonForegroundProperty);
            set => SetValue(ButtonForegroundProperty, value);
        }

        #endregion DependencyProperties

        public MessageBox()
        {
            InitializeComponent();
            InitializeTimeoutUpdateTimer();
        }

        private void InitializeTimeoutUpdateTimer()
        {
            m_TimeoutUpdateTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(100.0) };
            m_TimeoutUpdateTimer.Tick += TimeoutUpdateTimer_Tick;
        }

        public async Task<int> Show(LocalizationString title, LocalizationString text,
            params LocalizationString[] buttonTexts)
        {
            m_MessageBoxCompletionSource?.TrySetResult(int.MinValue);
            m_TxtTitle.Visibility = LocalizationString.IsNotNullOrEmpty(title).ToVisibility();
            m_TxtTitle.SetLocalizedText(title);
            m_TxtText.Visibility = LocalizationString.IsNotNullOrEmpty(text).ToVisibility();
            m_TxtText.SetLocalizedText(text);
            for (int i = 0; i < m_StackButtons.Children.Count; i++)
            {
                BorderedButton button = (BorderedButton)m_StackButtons.Children[i];
                TextBlock buttonTextBlock = (TextBlock)button.Content;
                if (i < buttonTexts.Length)
                {
                    button.Visibility = Visibility.Visible;
                    buttonTextBlock.SetLocalizedText(buttonTexts[i]);
                }
                else button.Visibility = Visibility.Collapsed;
            }
            m_MessageBoxCompletionSource = new TaskCompletionSource<int>();
            Visibility = Visibility.Visible;
            m_BorderBackground.FadeIn(150.0);
            m_BorderMain.Appear();
            int result = await m_MessageBoxCompletionSource.Task;
            if (result > int.MinValue)
            {
                m_BorderBackground.FadeOut();
                m_BorderMain.Disappear().Completed += (s, e) => Visibility = Visibility.Collapsed;
            }
            return result;
        }

        public async Task<int> ShowTimeout(LocalizationString titleFormat, LocalizationString textFormat,
            LocalizationString buttonText, TimeSpan timeout)
        {
            m_MessageBoxCompletionSource?.TrySetResult(int.MinValue);
            m_TimeoutEndTime = CtsApplication.Current.TimeElapsed + timeout;
            m_TimeoutTitleFormat = titleFormat;
            m_TimeoutTextFormat = textFormat;
            for (int i = 0; i < m_StackButtons.Children.Count; i++)
            {
                BorderedButton button = (BorderedButton)m_StackButtons.Children[i];
                TextBlock buttonTextBlock = (TextBlock)button.Content;
                if (i < 1)
                {
                    button.Visibility = Visibility.Visible;
                    buttonTextBlock.SetLocalizedText(buttonText);
                }
                else button.Visibility = Visibility.Collapsed;
            }
            m_TimeoutUpdateTimer.Start();
            UpdateTimeoutTexts();
            m_MessageBoxCompletionSource = new TaskCompletionSource<int>();
            Visibility = Visibility.Visible;
            m_BorderBackground.FadeIn(150.0);
            m_BorderMain.Appear();
            int result = await m_MessageBoxCompletionSource.Task;
            if (result > int.MinValue)
            {
                m_BorderBackground.FadeOut();
                m_BorderMain.Disappear().Completed += (s, e) => Visibility = Visibility.Collapsed;
            }
            return result;
        }

        private void UpdateTimeoutTexts()
        {
            TimeSpan remaining = m_TimeoutEndTime - CtsApplication.Current.TimeElapsed;
            int remainingSeconds = (int)Math.Ceiling(remaining.TotalSeconds);
            if (LocalizationString.IsNotNullOrEmpty(m_TimeoutTitleFormat))
            {
                m_TxtTitle.Visibility = Visibility.Visible;
                m_TxtTitle.SetLocalizedText(LocalizationString.Format(m_TimeoutTitleFormat, remainingSeconds));
            }
            else m_TxtTitle.Visibility = Visibility.Collapsed;
            if (LocalizationString.IsNotNullOrEmpty(m_TimeoutTextFormat))
            {
                m_TxtText.Visibility = Visibility.Visible;
                m_TxtText.SetLocalizedText(LocalizationString.Format(m_TimeoutTextFormat, remainingSeconds));
            }
            else m_TxtText.Visibility = Visibility.Collapsed;
        }

        public void HideDirectly()
        {
            m_TimeoutUpdateTimer.Stop();
            m_MessageBoxCompletionSource?.TrySetResult(int.MinValue);
            Visibility = Visibility.Collapsed;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.StylusDevice == null && !e.Handled) m_MessageBoxCompletionSource?.TrySetResult(-2);
        }

        protected override void OnTouchDown(TouchEventArgs e)
        {
            base.OnTouchDown(e);
            if (!e.Handled) m_MessageBoxCompletionSource?.TrySetResult(-2);
        }

        private void TimeoutUpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateTimeoutTexts();
            if (CtsApplication.Current.TimeElapsed >= m_TimeoutEndTime)
            {
                m_TimeoutUpdateTimer.Stop();
                m_MessageBoxCompletionSource?.TrySetResult(int.MaxValue);
            }
        }

        private void AnswerButton_Click(object sender, RoutedEventArgs e)
        {
            BorderedButton typedSender = (BorderedButton)sender;
            m_MessageBoxCompletionSource?.TrySetResult(m_StackButtons.Children.IndexOf(typedSender));
        }

        private void BntClose_Click(object sender, RoutedEventArgs e)
        {
            m_MessageBoxCompletionSource?.TrySetResult(-1);
        }
    }
}
