﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MediaPlayer
{
    class MediaPlayer
    {
        private MediaElement mMedia;
        private Image mPlayPauseButton = null;
        private Slider mSlider = null;
        private DispatcherTimer mTimer = null;
        private bool mMediaWasOpened = false;
        private bool mMediaIsPlaying = false;
        private bool mPlayPause = false;
        private FrameworkElement mFrameWorkElement = null;
        private string mSource;

        public string Source
        {
            get { return mSource; }
            set { mSource = value; mMediaWasOpened = false; }
        }

        public MediaPlayer(FrameworkElement frameworkElement , MediaElement mediaPlayer, Image playPauseButton, Slider progressSlider)
        {
            if (frameworkElement == null) throw new Exception("FrameworkElement cannot be null");
            if (mediaPlayer == null) throw new Exception("MediaPlayer cannot be null!");
            mFrameWorkElement = frameworkElement;
            mMedia = mediaPlayer;
            mMedia.MediaOpened += mMedia_MediaOpened;
            mMedia.MediaEnded += mMedia_MediaEnded;
            mMedia.CurrentStateChanged += mMedia_CurrentStateChanged;
            
            MediaControl.PlayPressed += MediaControl_PlayPressed;
            MediaControl.PausePressed += MediaControl_PausePressed;
            MediaControl.PlayPauseTogglePressed += MediaControl_PlayPauseTogglePressed;
            MediaControl.NextTrackPressed += MediaControl_NextTrackPressed;
            MediaControl.PreviousTrackPressed += MediaControl_PreviousTrackPressed;
            MediaControl.StopPressed += MediaControl_StopPressed;
           

            mPlayPauseButton = playPauseButton;
            mSlider = progressSlider;

            if (mSlider != null)
            {
                mSlider.Value = 0;
            }

            mTimer = new DispatcherTimer();
            mTimer.Tick += Tick;
            mTimer.Interval = TimeSpan.FromMilliseconds(100);
            mTimer.Start();

        }

        // -------------------
        // Media player events
        // -------------------

        private void mMedia_MediaOpened(object sender, RoutedEventArgs e)
        {
            mMediaWasOpened = true;
            mSlider.Value = 0;
        }

        private void mMedia_MediaEnded(object sender, RoutedEventArgs e)
        {
            stop();
            mSlider.Value = mSlider.Maximum;
        }

        private void mMedia_BufferingProgressChanged(object sender, RoutedEventArgs e)
        {
            
        }

        private void mMedia_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            if (mMedia.CurrentState == MediaElementState.Buffering)
            {
                mMediaIsPlaying = false;
            }
            if (mMedia.CurrentState == MediaElementState.Playing)
            {
                mMediaIsPlaying = true;
            }
            if (mMedia.CurrentState == MediaElementState.Paused)
            {
                mMediaIsPlaying = false;
            }
            if (mMedia.CurrentState == MediaElementState.Opening)
            {
                mMediaWasOpened = false;
            }
            if (mMedia.CurrentState == MediaElementState.Stopped)
            {
                mMediaIsPlaying = false;
            }
        }

        // ----------------------------
        // Media player events end here
        // ----------------------------

        public static BitmapImage ImageFromRelativePath(FrameworkElement parent, string path)
        {
            var uri = new Uri(parent.BaseUri, path);
            BitmapImage result = new BitmapImage();
            result.UriSource = uri;
            return result;
        } 
      
        private void Tick(object sender, object e)
        {
            if (mMediaIsPlaying && mMediaWasOpened && mSlider.Value <= mSlider.Maximum * mMedia.BufferingProgress) 
                mSlider.Value += 0.1;
            
        }

        public void play()
        {
            if (mMedia.Source != new Uri(mSource))
            {
                mMedia.Source = new Uri(mSource);
            }
            if (mSlider.Value == mSlider.Maximum)
                mSlider.Value = 0;

            mPlayPause = !mPlayPause;
            mMedia.Play();
            mPlayPauseButton.Source = ImageFromRelativePath(mFrameWorkElement, "Assets/pause_147x147.png");            
        }

        public void pause()
        {
            mPlayPause = !mPlayPause;
            mMedia.Pause();
            mPlayPauseButton.Source = ImageFromRelativePath(mFrameWorkElement, "Assets/play_147x147.png");
        }

        public void stop()
        {
            mMedia.Stop();
            mPlayPauseButton.Source = ImageFromRelativePath(mFrameWorkElement, "Assets/play_147x147.png");
        }

        public void playPause()
        {
            if (mPlayPause) pause();
            else play();
        }

        // --------------------
        // Media control events
        // --------------------

        private async void MediaControl_StopPressed(object sender, object e)
        {
            await mFrameWorkElement.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => stop());
        }

        private async void MediaControl_PlayPauseTogglePressed(object sender, object e)
        {
            await mFrameWorkElement.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                playPause();
            });
        }

        private async void MediaControl_PausePressed(object sender, object e)
        {
           await mFrameWorkElement.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => pause());
        }

        private async void MediaControl_PlayPressed(object sender, object e)
        {
            await mFrameWorkElement.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => play());
        }

        private void MediaControl_PreviousTrackPressed(object sender, object e)
        {

        }

        private void MediaControl_NextTrackPressed(object sender, object e)
        {

        }

        // -----------------------------
        // Media control events end here
        // -----------------------------
    }
}
