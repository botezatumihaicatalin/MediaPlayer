using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace MediaPlayer
{
    public sealed partial class PlaylistPage : Page
    {
        private MediaPlayer mediaPlayer;
        private List<Track> trackList;

        public PlaylistPage()
        {
            this.InitializeComponent();
            trackList = new List<Track>();
            MusicPlayer.AudioCategory = AudioCategory.BackgroundCapableMedia;
            mediaPlayer = new MediaPlayer(this, MusicPlayer, PlayPause, ProgressSlider);
            mediaPlayer.OnMediaFailed += MediaEnds;
            mediaPlayer.OnMediaEnded += MediaEnds;

            MediaControl.NextTrackPressed += MediaControl_NextTrackPressed;
            MediaControl.PreviousTrackPressed += MediaControl_PreviousTrackPressed;

            list.ItemClick += Grid_ItemClick;
            readPlayList();
        }

        private async Task<string> saveImageToFile(Uri path)
        {
            HttpWebRequest request;
            WebResponse response;
            Stream stream;
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile storageFile = null;
            bool found = false;

            try
            {
                storageFile = await storageFolder.GetFileAsync("thumbnail.jpg");
                found = true;
            }
            catch (Exception er)
            {

            }

            if (!found) storageFile = await storageFolder.CreateFileAsync("thumbnail.jpg");

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
                        }
                    }
                }
                return storageFile.Path;
            }
            catch (Exception er)
            {
                throw er;
            }
        }

        public async void readPlayList()
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFolder nextFolder = null;
            try
            {
                nextFolder = await storageFolder.GetFolderAsync("Playlist");
            }
            catch (Exception er)
            {
                return;
            }
            var read = await nextFolder.GetFilesAsync();
            for (int i = 0; i < read.Count; i++)
            {
                IList<String> lines = await FileIO.ReadLinesAsync(read[i]);
                Track t = new Track(lines[1], lines[0], lines[2], Convert.ToInt32(lines[5]), new Uri(lines[3]), lines[4], null);
                list.Items.Add(t);
                trackList.Add(t);
            }
        }

        private void MediaControl_PreviousTrackPressed(object sender, object e)
        {
            prevTrack();
        }

        private void MediaControl_NextTrackPressed(object sender, object e)
        {
            nextTrack();
        }

        private void MediaEnds(object sender, EventArgs e)
        {
            nextTrack();
        }

        private async Task LoadTrack(Track track)
        {
            YoutubeDecoder decoder = new YoutubeDecoder();
            decoder.VideoID = track.VideoID;
            track.CacheUriString = await decoder.fetchURL();
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
                mediaPlayer.CurrentTrack = track;
                mediaPlayer.play();
                VideoImageHolder.Source = new BitmapImage(track.ImageUri);
                VideoTitleHolder.Text = track.Name + " - " + track.Artist;
                String t = await saveImageToFile(track.ImageUri);
                MediaControl.TrackName = track.Name;
                MediaControl.ArtistName = track.Artist;
                MediaControl.AlbumArt = new Uri("ms-appdata:///Local/thumbnail.jpg");
            });
        }

        private void Grid_ItemClick(object sender, ItemClickEventArgs e)
        {
            Track new_item = ((Track)e.ClickedItem);
            mediaPlayer.stop();
            Task.Run(() => LoadTrack(new_item));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }
        private void PlayPause_Tapped(object sender, TappedRoutedEventArgs e)
        {
            mediaPlayer.playPause();
        }

        public async void nextTrack()
        {
            mediaPlayer.stop();
            mediaPlayer.MediaIndex += 1;
            mediaPlayer.MediaIndex %= trackList.Count;
            Task.Run(() => LoadTrack(trackList[mediaPlayer.MediaIndex]));
        }

        public async void prevTrack()
        {
            mediaPlayer.stop();
            mediaPlayer.MediaIndex -= 1;
            if (mediaPlayer.MediaIndex < 0) mediaPlayer.MediaIndex = trackList.Count - 1;
            Task.Run(() => LoadTrack(trackList[mediaPlayer.MediaIndex]));
        }

        private void Prev_track_Tapped(object sender, TappedRoutedEventArgs e)
        {
            prevTrack();
        }

        private void Next_track_Tapped(object sender, TappedRoutedEventArgs e)
        {
            nextTrack();
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Window.Current.Content = MainPage.current;
        }
    }
}
