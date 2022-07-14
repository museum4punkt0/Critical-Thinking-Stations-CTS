using Gemelo.Components.Dah.Applications;
using Gemelo.Components.Common.Base;
using Gemelo.Components.Common.Localization;
using Gemelo.Components.Common.Wpf.UI;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Gemelo.Components.Common.Text;
using Gemelo.Components.Cts.Code.Media;
using System.ComponentModel;
using Gemelo.Components.Cts.Applications;

namespace Gemelo.Components.Cts.Controls.Media
{
    /// <summary>
    /// Zeigt einen Player für unterschiedliche Medien an.
    /// </summary>
    public partial class MediaPlayer : UserControl
    {
        #region Konstanten

        private static readonly TimeSpan ConstStopAheadTimeSpan = TimeSpan.FromMilliseconds(20.0);
        private const double ConstSeekUpdateIntervalInMs = 1000.0;

        //private const double ConstMinimumPositionTimeTextDistance = 40.0;

        private const double DefaultVolume = 0.5;

        #endregion Konstanten

        #region Felder und Eigenschaften

        private string m_MediaFilename;

        private bool m_IsAudio;

        private bool m_ShouldPlay;

        private bool m_IsSeeking = false;
        private Point m_SeekingStartPoint;
        private TimeSpan m_SeekingStartPosition;
        private TouchDevice m_LastSeekingTouchDevice;
        private TimeSpan m_NewSeekPosition;
        private double m_LastSeekUpdateTime;

        private bool m_IsPlaying = false;

        public bool IsPlaying
        {
            get => m_IsPlaying;
        }

        public MediaElement CurrentMediaElement
        {
            get => m_MediaElementEn.Visibility == Visibility.Visible ? m_MediaElementEn : m_MediaElement;
        }

        #endregion Felder und Eigenschaften

        #region DependencyProperties

        public static readonly DependencyProperty FondProperty = DependencyProperty.Register(
           "Fond", typeof(Brush), typeof(MediaPlayer),
           new PropertyMetadata(Brushes.Transparent));

        [Description("Fond")]
        [Category("gemelo")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Brush Fond
        {
            get { return (Brush)GetValue(FondProperty); }
            set { SetValue(FondProperty, value); }
        }

        public static readonly DependencyProperty PlayerControlsBackgroundProperty = DependencyProperty.Register(
           "PlayerControlsBackground", typeof(Brush), typeof(MediaPlayer),
           new PropertyMetadata(Brushes.Transparent));

        [Description("PlayerControlsBackground")]
        [Category("gemelo")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Brush PlayerControlsBackground
        {
            get { return (Brush)GetValue(PlayerControlsBackgroundProperty); }
            set { SetValue(PlayerControlsBackgroundProperty, value); }
        }

        public static readonly DependencyProperty PlayOverlayBackgroundProperty = DependencyProperty.Register(
           "PlayOverlayBackground", typeof(Brush), typeof(MediaPlayer),
           new PropertyMetadata(Brushes.Transparent));

        [Description("Hintergrund für das Overlay mit Play-Button")]
        [Category("gemelo")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Brush PlayOverlayBackground
        {
            get { return (Brush)GetValue(PlayOverlayBackgroundProperty); }
            set { SetValue(PlayOverlayBackgroundProperty, value); }
        }

        public static readonly DependencyProperty UseStylusTapGestureInsteadOfTouchEventsProperty =
            DependencyProperty.Register("UseStylusTapGestureInsteadOfTouchEvents", typeof(bool), typeof(MediaPlayer),
            new PropertyMetadata(false));

        [Description("If set to true all buttons reacts to the Tap-StylusGesture instead of the touch events")]
        [Category("gemelo")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool UseStylusTapGestureInsteadOfTouchEvents
        {
            get { return (bool)GetValue(UseStylusTapGestureInsteadOfTouchEventsProperty); }
            set { SetValue(UseStylusTapGestureInsteadOfTouchEventsProperty, value); }
        }

        #endregion DependencyProperties

        #region Ereignisse

        public event EventHandler Completed;

        protected void OnCompleted()
        {
            Completed?.Invoke(this, null);
        }

        public event EventHandler PauseClick;

        protected void OnPauseClick()
        {
            PauseClick?.Invoke(this, null);
        }

        public event EventHandler PlayClick;

        protected void OnPlayClick()
        {
            PlayClick?.Invoke(this, null);
        }

        public event EventHandler CloseClick;

        protected void OnCloseClick()
        {
            CloseClick?.Invoke(this, null);
        }

        public event EventHandler SeekUpdate;

        protected void OnSeekUpdate()
        {
            SeekUpdate?.Invoke(this, null);
        }

        #endregion Ereignisse

        #region Konstruktor und Initialisierung

        public MediaPlayer()
        {
            Foreground = Brushes.White;
            InitializeComponent();
            if (DahApplication.IsAvailable)
            {
                Languages.Changed += App_LanguageChanged;
                CompositionTarget.Rendering += CompositionTarget_Rendering;
            }
        }

        #endregion Konstruktor und Initialisierung

        #region Öffentliche Methoden

        public void PrepareFor(string mediaFilename, bool useVideoPoster = false, bool noImage = false, 
            string mediaDirectoryPath = null, bool autoAdjustHeight = false)
        {
            m_MediaFilename = mediaFilename;
            if (m_MediaFilename.IsNotNullOrEmpty())
            {
                MediaType mediaType = MediaFileHelper.GetTypeFromFilename(mediaFilename);
                m_IsAudio = mediaType == MediaType.Audio;
                string mediaFilePath = mediaType == MediaType.Image ?
                    null : CtsApplication.Current.GetMediaFilePathFor(mediaFilename, mediaDirectoryPath);
                string mediaFilePathEnglish = mediaType == MediaType.Image ?
                    null :
                    CtsApplication.Current.TryGetLocalizedMediaFilePathFor(mediaFilename,
                        language: Languages.English,
                        mediaDirectoryPath: mediaDirectoryPath);
                BitmapSource imageBitmap = noImage || (!useVideoPoster && mediaType == MediaType.Video) ?
                    null : CtsApplication.Current.GetPreviewBitmapFor(mediaFilename,
                        preferPreview: false,
                        mediaDirectoryPath: mediaDirectoryPath);
                m_GridPlayOverlay.Visibility = (mediaType != MediaType.Image).ToVisibility();
                if (autoAdjustHeight)
                {
                    if (imageBitmap != null) Height = Width / imageBitmap.PixelWidth * imageBitmap.PixelHeight;
                    else Height = 0.0;
                }
                PrepareForInternal(
                    mediaFilePath: mediaFilePath,
                    mediaFilePathEnglish: mediaFilePathEnglish,
                    imageBitmap: imageBitmap);
            }
            else
            {
                PrepareForInternal(
                    mediaFilePath: null,
                    mediaFilePathEnglish: null,
                    imageBitmap: null);
            }
        }

        public void Play()
        {
            if (m_MediaElement.Source != null)
            {
                m_ShouldPlay = true;
                m_IsPlaying = true;
                m_MediaElement.Play();
                if (m_MediaElementEn.Source != null) m_MediaElementEn.Play();
                m_BtnPlay.Visibility = Visibility.Collapsed;
                m_BtnPause.Visibility = Visibility.Visible;
                m_BorderPlayOverlayInner.Visibility = Visibility.Collapsed;
            }
        }

        public void Pause()
        {
            m_MediaElement.Pause();
            if (m_MediaElementEn.Source != null) m_MediaElementEn.Pause();
            m_IsPlaying = false;
            m_BtnPlay.Visibility = Visibility.Visible;
            m_BtnPause.Visibility = Visibility.Collapsed;
            m_BorderPlayOverlayInner.Visibility = Visibility.Visible;
        }


        public void Stop()
        {
            m_ShouldPlay = false;
            m_IsPlaying = false;
            m_MediaElement.Stop();
            m_MediaElementEn.Stop();
            m_MediaElement.Position = TimeSpan.Zero;
            m_MediaElementEn.Position = TimeSpan.Zero;
            m_BtnPlay.Visibility = Visibility.Visible;
            m_BtnPause.Visibility = Visibility.Collapsed;
        }

        #endregion Öffentliche Methoden

        #region Private Methoden

        private void PrepareForInternal(string mediaFilePath, string mediaFilePathEnglish, BitmapSource imageBitmap)
        {
            m_MediaElement.Source = string.IsNullOrEmpty(mediaFilePath) ? null : new Uri(mediaFilePath);
            m_MediaElement.Position = TimeSpan.Zero;
            m_MediaElement.Play();
            if (!string.IsNullOrEmpty(mediaFilePathEnglish))
            {
                m_MediaElementEn.Source = new Uri(mediaFilePathEnglish);
                m_MediaElementEn.Position = TimeSpan.Zero;
                m_MediaElementEn.Play();
            }
            else
            {
                m_MediaElementEn.Source = null;
            }
            UpdateMediaPlayerVolumesAndVisiblities();
            m_ImagePreview.Source = imageBitmap;
            m_GridPlayerControls.Visibility = (!string.IsNullOrEmpty(mediaFilePath)).ToVisibility();
            m_ShouldPlay = false;
            SetSeekBarPositionAndText(0.0);
            m_IsPlaying = false;
        }

        private void ProcessSeekBarOuterInputDown(Point pointOnControl, double positionOnSeekBar)
        {
            if (CurrentMediaElement.NaturalDuration.HasTimeSpan)
            {
                m_SeekingStartPoint = pointOnControl;
                m_IsSeeking = true;
                double totalSeconds = CurrentMediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                m_NewSeekPosition = TimeSpan.FromSeconds(positionOnSeekBar / GetSeekBarLength() * totalSeconds);
                m_SeekingStartPosition = m_NewSeekPosition;
                SetSeekBarPositionAndText(m_NewSeekPosition.TotalSeconds / totalSeconds);
            }
            OnSeekUpdate();
        }

        private void ProcessInputDown(Point point)
        {
            m_SeekingStartPoint = point;
            m_SeekingStartPosition = CurrentMediaElement.Position;
            m_IsSeeking = true;
            OnSeekUpdate();
        }

        private void ProcessInputMove(Point point)
        {
            double delta = (point.X - m_SeekingStartPoint.X) / GetSeekBarLength();
            if (CurrentMediaElement.NaturalDuration.HasTimeSpan)
            {
                double totalSeconds = CurrentMediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                m_NewSeekPosition = m_SeekingStartPosition + TimeSpan.FromSeconds(delta * totalSeconds);
                SetSeekBarPositionAndText(m_NewSeekPosition.TotalSeconds / totalSeconds);
            }
            OnSeekUpdate();
        }

        private void ProcessInputUp()
        {
            m_LastSeekingTouchDevice?.Capture(null);
            Mouse.Capture(null);
            StopSeeking();
            OnSeekUpdate();
        }

        private void StopSeeking()
        {
            UpdateMediaPositionFromNewSeekPosition();
            m_IsSeeking = false;
        }

        private void UpdateMediaPositionFromNewSeekPosition()
        {
            m_MediaElement.Position = m_NewSeekPosition;
            if (m_MediaElementEn.Source != null) m_MediaElementEn.Position = m_NewSeekPosition;
            if (CurrentMediaElement.NaturalDuration.HasTimeSpan)
            {
                SetSeekBarPositionAndText(CurrentMediaElement.Position.TotalSeconds /
                    CurrentMediaElement.NaturalDuration.TimeSpan.TotalSeconds);
            }
            m_LastSeekUpdateTime = DahApplication.Current.TimeInMs;
        }

        private double GetSeekBarLength()
        {
            if (m_BorderSeekBar.HasActualSize())
            {
                return Math.Max(1.0, m_BorderSeekBar.ActualWidth);
            }
            else return 1.0;
        }

        private void SetSeekBarPositionAndText(double progress)
        {
            double length = GetSeekBarLength();
            double position = MathEx.MinMax(progress, 0.0, 1.0) * length;
            m_BorderSeekBarFill.Width = Math.Max(1.0, position - m_BorderSeekBar.BorderThickness.Left);
            Canvas.SetLeft(m_GridSeekHandle, position);
            if (CurrentMediaElement.NaturalDuration.HasTimeSpan)
            {
                TimeSpan elapsed = CurrentMediaElement.NaturalDuration.TimeSpan * progress;
                m_TxtPosition.Text = $"{elapsed:m\\:ss}";
                //m_TxtPosition.Visibility = (position + ConstMinimumPositionTimeTextDistance <= length).ToVisibility();
                TimeSpan remaining = CurrentMediaElement.NaturalDuration.TimeSpan - elapsed;
                m_TxtRemaining.Text = $"{remaining:m\\:ss}";
            }
            else
            {
                m_TxtPosition.Text = string.Empty;
                m_TxtRemaining.Text = string.Empty;
            }
        }

        private void UpdateMediaPlayerVolumesAndVisiblities()
        {
            if (m_MediaElementEn.Source != null)
            {
                bool isEnglish = Languages.Current == Languages.English;
                m_MediaElement.Volume = isEnglish ? 0.0 : DefaultVolume;
                m_MediaElement.Visibility = (!isEnglish).ToVisibilityAndHidden();
                m_MediaElementEn.Volume = isEnglish ? DefaultVolume : 0.0;
                m_MediaElementEn.Visibility = isEnglish.ToVisibilityAndHidden();
            }
            else
            {
                m_MediaElement.Volume = DefaultVolume;
                m_MediaElement.Visibility = Visibility.Visible;
                m_MediaElementEn.Visibility = Visibility.Hidden;
            }
        }

        private void ProcessOverlayClick()
        {
            if (m_IsPlaying)
            {
                Pause();
                OnPauseClick();
            }
            else
            {
                Play();
                OnPlayClick();
            }
        }

        #endregion Private Methoden

        #region Ereignishandler

        private void App_LanguageChanged(object sender, EventArgs e)
        {
            UpdateMediaPlayerVolumesAndVisiblities();
            if (CurrentMediaElement.NaturalDuration.HasTimeSpan)
            {
                TimeSpan position = CurrentMediaElement.Position;
                if (position >= CurrentMediaElement.NaturalDuration.TimeSpan - ConstStopAheadTimeSpan)
                {
                    Stop();
                    OnCompleted();
                }
            }
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (m_IsSeeking && DahApplication.Current.TimeInMs >
                m_LastSeekUpdateTime + ConstSeekUpdateIntervalInMs)
            {
                UpdateMediaPositionFromNewSeekPosition();
            }
            if (CurrentMediaElement.NaturalDuration.HasTimeSpan)
            {
                m_TxtDuration.Text = $"{CurrentMediaElement.NaturalDuration.TimeSpan:m\\:ss}";
                if (!m_IsSeeking)
                {
                    SetSeekBarPositionAndText(CurrentMediaElement.Position.TotalSeconds /
                        CurrentMediaElement.NaturalDuration.TimeSpan.TotalSeconds);
                }
            }
            if (m_ShouldPlay)
            {
                if (CurrentMediaElement.NaturalDuration.HasTimeSpan)
                {
                    TimeSpan position = CurrentMediaElement.Position;
                    if (position >= CurrentMediaElement.NaturalDuration.TimeSpan - ConstStopAheadTimeSpan)
                    {
                        Stop();
                        OnCompleted();
                    }
                }
                m_MediaElement.Visibility = Visibility.Visible;
                if (!m_IsAudio) m_ImagePreview.Visibility = Visibility.Collapsed;
            }
            else
            {
                m_MediaElement.Pause();
                if (m_MediaElementEn.Source != null) m_MediaElementEn.Pause();
                if (m_MediaElement.Position == TimeSpan.Zero && m_ImagePreview.Source != null)
                {
                    m_MediaElement.Visibility = Visibility.Hidden;
                    m_ImagePreview.Visibility = Visibility.Visible;
                }
            }
        }

        private void BtnPlay_Click(object sender, EventArgs e)
        {
            Play();
            OnPlayClick();
        }

        private void BtnPause_Click(object sender, EventArgs e)
        {
            Pause();
            OnPauseClick();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            m_MediaElement.Pause();
            if (m_MediaElementEn.Source != null) m_MediaElementEn.Pause();
            m_IsPlaying = false;
            OnCloseClick();
        }

        private void BorderSeekBarOuter_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.StylusDevice == null)
            {
                if (m_LastSeekingTouchDevice != null) m_LastSeekingTouchDevice.Capture(null);
                m_LastSeekingTouchDevice = null;
                Mouse.Capture(m_GridSeekHandle);
                ProcessSeekBarOuterInputDown(e.GetPosition(this), e.GetPosition(m_BorderSeekBarOuter).X);
            }
        }

        private void BorderSeekBarOuter_TouchDown(object sender, TouchEventArgs e)
        {
            if (!UseStylusTapGestureInsteadOfTouchEvents)
            {
                Mouse.Capture(null);
                m_LastSeekingTouchDevice = e.TouchDevice;
                m_LastSeekingTouchDevice.Capture(m_GridSeekHandle);
                ProcessSeekBarOuterInputDown(e.GetTouchPoint(this).Position,
                    e.GetTouchPoint(m_BorderSeekBarOuter).Position.X);
            }
        }

        private void BorderSeekBarOuter_StylusSystemGesture(object sender, StylusSystemGestureEventArgs e)
        {
            if (e.SystemGesture == SystemGesture.Tap)
            {
                ProcessSeekBarOuterInputDown(e.GetPosition(this),
                    e.GetPosition(m_BorderSeekBarOuter).X);
                StopSeeking();
            }
        }

        private void GridSeekHandle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.StylusDevice == null)
            {
                if (m_LastSeekingTouchDevice != null) m_LastSeekingTouchDevice.Capture(null);
                m_LastSeekingTouchDevice = null;
                Mouse.Capture(m_GridSeekHandle);
                ProcessInputDown(e.GetPosition(this));
            }
        }

        private void GridSeekHandle_TouchDown(object sender, TouchEventArgs e)
        {
            if (!UseStylusTapGestureInsteadOfTouchEvents)
            {
                Mouse.Capture(null);
                m_LastSeekingTouchDevice = e.TouchDevice;
                m_LastSeekingTouchDevice.Capture(m_GridSeekHandle);
                ProcessInputDown(e.GetTouchPoint(this).Position);
            }
        }

        private void GridSeekHandle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.StylusDevice == null && e.LeftButton == MouseButtonState.Pressed)
            {
                ProcessInputMove(e.GetPosition(this));
            }
        }

        private void GridSeekHandle_TouchMove(object sender, TouchEventArgs e)
        {
            if (e.TouchDevice == m_LastSeekingTouchDevice) ProcessInputMove(e.GetTouchPoint(this).Position);
        }

        private void GridSeekHandle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.StylusDevice == null && e.ChangedButton == MouseButton.Left)
            {
                ProcessInputUp();
            }
        }

        private void GridSeekHandle_TouchUp(object sender, TouchEventArgs e)
        {
            if (e.TouchDevice == m_LastSeekingTouchDevice) ProcessInputUp();
        }

        private void GridSeekHandle_LostMouseCapture(object sender, MouseEventArgs e)
        {
            StopSeeking();
        }

        private void GridSeekHandle_LostTouchCapture(object sender, TouchEventArgs e)
        {
            StopSeeking();
        }

        private void BtnPlayOverlay_Click(object sender, RoutedEventArgs e)
        {
            ProcessOverlayClick();
        }

        //private void BorderPlayOverlay_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.StylusDevice == null) ProcessOverlayClick();
        //}

        //private void BorderPlayOverlay_TouchDown(object sender, TouchEventArgs e)
        //{
        //    ProcessOverlayClick();
        //}

        private void MediaPlayer_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue) Stop();
        }

        #endregion Ereignishandler
    }
}
