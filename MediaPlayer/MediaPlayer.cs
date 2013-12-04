using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MediaPlayer
{
    public delegate void OnMediaEndHandler(object sender, EventArgs e);
    public delegate void OnMediaFailedHandler(object sender, EventArgs e);

    class MediaPlayer
    {
        private MediaElement mMedia;
        private Image mPlayPauseButton = null;
        private Slider mSlider = null;
        private DispatcherTimer mTimer = null;
        private bool mPlayPause = false;
        private FrameworkElement mFrameWorkElement = null;
        private Track mCurrentTrack;
        private readonly object mLock = new object();

        public Track CurrentTrack
        {
            get { return mCurrentTrack; }
            set { mCurrentTrack = value; mSlider.Value = 0; mSlider.Maximum = mCurrentTrack.Duration * 4.5 / 5.0; }
        }
        public bool PlayButtonState
        {
            get { return mPlayPause; }
        }
        public int MediaIndex
        {
            get;
            set;
        }

        public event OnMediaEndHandler OnMediaEnded;
        public event OnMediaFailedHandler OnMediaFailed;

        public MediaPlayer(FrameworkElement frameworkElement , MediaElement mediaPlayer, Image playPauseButton, Slider progressSlider)
        {
            if (frameworkElement == null) throw new Exception("FrameworkElement cannot be null");
            if (mediaPlayer == null) throw new Exception("MediaPlayer cannot be null!");

            mFrameWorkElement = frameworkElement;
            mMedia = mediaPlayer;
            MediaIndex = -1;
            mMedia.MediaOpened += mMedia_MediaOpened;
            mMedia.MediaEnded += mMedia_MediaEnded;
            mMedia.CurrentStateChanged += mMedia_CurrentStateChanged;
            mMedia.MediaFailed += mMedia_MediaFailed;

            mPlayPauseButton = playPauseButton;
            mSlider = progressSlider;
            mSlider.Value = 0;

            mTimer = new DispatcherTimer();
            mTimer.Tick += Tick;
            mTimer.Interval = TimeSpan.FromMilliseconds(100);
            mTimer.Start();

        }

        private void mMedia_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (OnMediaFailed != null) OnMediaFailed(this, EventArgs.Empty);
        }

        // -------------------
        // Media player events
        // -------------------

        private void mMedia_MediaOpened(object sender, RoutedEventArgs e)
        {
            mSlider.Value = 0;
        }

        private void mMedia_MediaEnded(object sender, RoutedEventArgs e)
        {
            stop();
            mSlider.Value = mSlider.Maximum;
            if (OnMediaEnded != null) OnMediaEnded(this, EventArgs.Empty);
        }

        private async void mMedia_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            if (mMedia.CurrentState == MediaElementState.Opening)
            {
                MediaControl.TrackName = CurrentTrack.Name;
                MediaControl.ArtistName = CurrentTrack.Artist;
                await saveImageToFile(CurrentTrack.ImageUri);
                MediaControl.AlbumArt = new Uri("ms-appdata:///Local/thumbnail.jpg");
            }
        }

        // ----------------------------
        // Media player events end here
        // ----------------------------
      
        private void Tick(object sender, object e)
        {
            if (mMedia.CurrentState == MediaElementState.Playing && mSlider.Value <= mSlider.Maximum * mMedia.BufferingProgress)
            {
                mSlider.Value += 0.1;
                if (mSlider.Value >= 2.0 && mSlider.Value <= 2.1)
                {
                    try
                    {
                        ToastAndTileNotifications.ToastNotifications(CurrentTrack.Artist, CurrentTrack.Name, CurrentTrack.ImageUri.AbsoluteUri);
                        ToastAndTileNotifications.LiveTileOn(CurrentTrack.Artist, CurrentTrack.Name, CurrentTrack.ImageUri.AbsoluteUri);
                    }
                    catch (Exception) { }
                }
            }
        }

        public void play()
        {
            lock (mLock)
            {
                if (mMedia.Source != new Uri(mCurrentTrack.CacheUriString))
                {
                    mMedia.Source = new Uri(mCurrentTrack.CacheUriString);
                }

                if (mSlider.Value == mSlider.Maximum)
                    mSlider.Value = 0;
                
                mPlayPause = true;
                mMedia.Play();
                mPlayPauseButton.Source = new BitmapImage(new Uri("ms-appx:///Assets/pause_147x147.png"));
            }
        }

        public void pause()
        {
            lock (mLock)
            {
                mPlayPause = false;
                mMedia.Pause();
                mPlayPauseButton.Source = new BitmapImage(new Uri("ms-appx:///Assets/play_147x147.png"));
            }
        }

        public void stop()
        {
            lock (mLock)
            {
                mPlayPause = false;
                mMedia.Stop();
                mPlayPauseButton.Source = new BitmapImage(new Uri("ms-appx:///Assets/play_147x147.png"));
            }
        }

        public void playPause()
        {
            if (mPlayPause) pause();
            else play();
        }
        private async Task saveImageToFile(Uri path)
        {
            HttpWebRequest request;
            WebResponse response;
            Stream stream;
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile storageFile = await storageFolder.CreateFileAsync("thumbnail.jpg", CreationCollisionOption.ReplaceExisting);

            request = (HttpWebRequest)WebRequest.Create(path);
            try
            {
                request = (HttpWebRequest)WebRequest.Create(path);
                response = await request.GetResponseAsync();
                stream = response.GetResponseStream();

                using (IRandomAccessStream fileStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    using (IOutputStream outputStream = fileStream.GetOutputStreamAt(0))
                    {
                        using (DataWriter dataWriter = new DataWriter(outputStream))
                        {
                            int one_byte;
                            while ((one_byte = stream.ReadByte()) != -1)
                            {
                                dataWriter.WriteByte(Convert.ToByte(one_byte));
                            }
                            await dataWriter.StoreAsync();
                            dataWriter.DetachStream();
                            dataWriter.Dispose();
                        }
                        outputStream.Dispose();
                    }
                    fileStream.Dispose();
                }
                stream.Dispose();
                response.Dispose();
            }
            catch (Exception er)
            {
                
            }
        }

    }
}
