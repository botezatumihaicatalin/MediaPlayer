using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
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
    public delegate void OnMediaEndHandler(EventArgs e);
    public delegate void OnMediaFailedHandler(EventArgs e);

    abstract class MediaPlayer
    {
        private static MediaElement mMedia;
        private static Image mPlayPauseButton = null;
        private static Slider mSlider = null;
        private static DispatcherTimer mTimer = null;
        private static bool mPlayPause = false;
        private static Track mCurrentTrack;
        private static double mPlayed;
        private static readonly object mLock = new object();
        private static double mVolume;

        public static Track CurrentTrack
        {
            get { return mCurrentTrack; }
            set { mCurrentTrack = value; mPlayed = 0.0 ; mSlider.Maximum = mCurrentTrack.Duration * 4.5 / 5.0; }
        }
        public static bool PlayButtonState
        {
            get { return mPlayPause; }
        }
        public static int MediaIndex
        {
            get;
            set;
        }

        public static double Volume
        {
            get { return mVolume; }          
            set { mVolume = value; if (mMedia != null) mMedia.Volume = mVolume; }
        }

        public static event OnMediaEndHandler OnMediaEnded;
        public static event OnMediaFailedHandler OnMediaFailed;

        public static void initialize(Image playPauseButton, Slider progressSlider , Image videoImageHolder , TextBlock videoTitleHolder)
        {
            if (mMedia == null)
            {
                DependencyObject rootGrid = VisualTreeHelper.GetChild(Window.Current.Content, 0);
                MediaElement rootMediaElement = (MediaElement)VisualTreeHelper.GetChild(rootGrid, 0);

                mMedia = rootMediaElement;
                mMedia.AudioCategory = AudioCategory.BackgroundCapableMedia;

                MediaIndex = -1;
                mMedia.MediaOpened += mMedia_MediaOpened;
                mMedia.MediaEnded += mMedia_MediaEnded;
                mMedia.CurrentStateChanged += mMedia_CurrentStateChanged;
                mMedia.MediaFailed += mMedia_MediaFailed;
                mPlayed = 0.0;

                if (mTimer == null)
                {
                    mTimer = new DispatcherTimer();
                    mTimer.Tick += Tick;
                    mTimer.Interval = TimeSpan.FromMilliseconds(100);
                    mTimer.Start();
                }
                Volume = 1.0;
            }
            mPlayPauseButton = playPauseButton;
            mSlider = progressSlider;
            if (mCurrentTrack != null)
            {
                mSlider.Maximum = mCurrentTrack.Duration * 4.5 / 5.0;
                videoImageHolder.Source = new BitmapImage(mCurrentTrack.ImageUri);
                videoTitleHolder.Text = mCurrentTrack.Name + " - " + mCurrentTrack.Artist;
            }
        }

        private static void mMedia_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            if (OnMediaFailed != null) OnMediaFailed(EventArgs.Empty);
        }

        // -------------------
        // Media player events
        // -------------------

        private static void mMedia_MediaOpened(object sender, RoutedEventArgs e)
        {
            mPlayed = 0.0;
        }

        private static void mMedia_MediaEnded(object sender, RoutedEventArgs e)
        {
            stop();
            mPlayed = mSlider.Maximum;
            if (OnMediaEnded != null) OnMediaEnded(EventArgs.Empty);
        }

        private static async void mMedia_CurrentStateChanged(object sender, RoutedEventArgs e)
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

        private static void Tick(object sender, object e)
        {
            if (mMedia.CurrentState == MediaElementState.Playing && mSlider.Value <= mSlider.Maximum * mMedia.BufferingProgress)
            {
                mPlayed += 0.1;
                if (mPlayed >= 2.0 && mPlayed <= 2.1)
                {
                    try
                    {
                        ToastAndTileNotifications.ToastNotifications(CurrentTrack.Artist, CurrentTrack.Name, CurrentTrack.ImageUri.AbsoluteUri);
                        ToastAndTileNotifications.LiveTileOn(CurrentTrack.Artist, CurrentTrack.Name, CurrentTrack.ImageUri.AbsoluteUri);
                    }
                    catch (Exception) { }
                }
            }
            mSlider.Value = mPlayed;
        }

        public static void play()
        {
            lock (mLock)
            {
                if (mMedia.Source != new Uri(mCurrentTrack.CacheUriString))
                {
                    mMedia.Source = new Uri(mCurrentTrack.CacheUriString);
                }

                if (mPlayed == mSlider.Maximum)
                    mPlayed = 0;

                mPlayPause = true;
                mMedia.Play();
                mPlayPauseButton.Source = new BitmapImage(new Uri("ms-appx:///Assets/pause_147x147.png"));
            }
        }

        public static void pause()
        {
            lock (mLock)
            {
                mPlayPause = false;
                mMedia.Pause();
                mPlayPauseButton.Source = new BitmapImage(new Uri("ms-appx:///Assets/play_147x147.png"));
            }
        }

        public static void stop()
        {
            lock (mLock)
            {
                mPlayPause = false;
                mMedia.Stop();
                mPlayPauseButton.Source = new BitmapImage(new Uri("ms-appx:///Assets/play_147x147.png"));
            }
        }

        public static void playPause()
        {
            if (mPlayPause) pause();
            else play();
        }
        private static async Task saveImageToFile(Uri path)
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
