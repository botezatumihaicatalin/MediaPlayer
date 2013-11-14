using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
        private FrameworkElement mFrameWorkElement = null;

        public string Source
        {
            get;
            set;
        }

        public MediaPlayer(FrameworkElement frameworkElement , MediaElement mediaPlayer, Image playPauseButton, Slider progressSlider)
        {
            if (frameworkElement == null) throw new Exception("FrameworkElement cannot be null");
            if (mediaPlayer == null) throw new Exception("MediaPlayer cannot be null!");
            mFrameWorkElement = frameworkElement;
            mMedia = mediaPlayer;
            mMedia.MediaOpened += mMedia_MediaOpened;
            mMedia.MediaEnded += mMedia_MediaEnded;
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

        public static BitmapImage ImageFromRelativePath(FrameworkElement parent, string path)
        {
            var uri = new Uri(parent.BaseUri, path);
            BitmapImage result = new BitmapImage();
            result.UriSource = uri;
            return result;
        }

        private void mMedia_MediaOpened(object sender, RoutedEventArgs e)
        {
            mMediaWasOpened = true;
            mSlider.Value = 0;
        }

        private void mMedia_MediaEnded(object sender, RoutedEventArgs e)
        {
            mMediaWasOpened = false;
            mSlider.Value = mSlider.Maximum;
        }
      
        private void Tick(object sender, object e)
        {
            if (mMediaIsPlaying && mMediaWasOpened) mSlider.Value+= 0.1;
        }

        public void play()
        {
            mMediaIsPlaying = true;
            if(mMedia.Source != new Uri(Source))
            {
                mMedia.Source = new Uri(Source);
            }
            mMedia.Play();
            mPlayPauseButton.Source = ImageFromRelativePath(mFrameWorkElement, "Assets/pause.png");
        }

        public void pause()
        {
            mMediaIsPlaying = false;
            mMedia.Pause();
            mPlayPauseButton.Source = ImageFromRelativePath(mFrameWorkElement, "Assets/play.png");
        }

        public void playPause()
        {
            if (mMediaIsPlaying) pause();
            else play();
        }
    }
}
