using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MediaPlayer
{
    //https://www.youtube.com/embed/1VnSzOzL0UM?autoplay=1
    //https://gdata.youtube.com/feeds/api/videos/-biwNmWLFa5Q?v=2

    public sealed partial class MainPage : Page
    {
        private YoutubeStats stats;
        private YoutubeDecoder decoder;
        private MediaPlayer mediaPlayer;

        public MainPage()
        {
            this.InitializeComponent();
            stats = new YoutubeStats();
            decoder = new YoutubeDecoder();
            MusicPlayer.AudioCategory = AudioCategory.BackgroundCapableMedia;
            mediaPlayer = new MediaPlayer(this, MusicPlayer, PlayPause, ProgressSlider);
            
        }

        private async Task<string> saveImageToFile(BitmapImage image)
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
            catch(Exception er)
            {
                
            }

            if (!found) storageFile = await storageFolder.CreateFileAsync("thumbnail.jpg");

            request = (HttpWebRequest)WebRequest.Create(image.UriSource);
            try
            {
                request = (HttpWebRequest)WebRequest.Create(image.UriSource);
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

        private async Task PopulateUI(string VideoID)
        {
            try
            {
                stats.VideoID = VideoID;
                decoder.VideoID = VideoID;
                await stats.getData();
                VideoImageHolder.Source = stats.VideoImage;
                VideoTitleHolder.Text = stats.VideoTitle;
                await decoder.getVideoCacheURL();

                ProgressSlider.Maximum = stats.DurationInSeconds * 4.0 / 5.0;
                ProgressSlider.Value = 0;

                MediaControl.TrackName = stats.VideoTitle;
                String t = await saveImageToFile(stats.VideoImage);
                MediaControl.AlbumArt = new Uri("ms-appdata:///local/thumbnail.jpg");

                mediaPlayer.Source = decoder.DirectVideoURL;
                text.Text = decoder.DirectVideoURL;

            }
            catch (Exception er)
            {
                new MessageDialog("Error",er.Message).ShowAsync();
            }

        }

        public static BitmapImage ImageFromRelativePath(FrameworkElement parent, string path)
        {
            var uri = new Uri(parent.BaseUri, path);
            BitmapImage result = new BitmapImage();
            result.UriSource = uri;
            return result;
        } 

        private async void Set_Click(object sender, RoutedEventArgs e)
        {
            TopTrackByTag t = new TopTrackByTag(VideoIdTextBox.Text);
            await t.get();
        }


        private void PlayPause_Tapped(object sender, TappedRoutedEventArgs e)
        {
            text.Text = mediaPlayer.Source;
            mediaPlayer.playPause();
        }

        private void PlayPause_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            PlayPause.Source = new BitmapImage(new Uri("ms-appx:///Assets/play_entered_147x147.png"));
        }

        private void PlayPause_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            PlayPause.Source = new BitmapImage(new Uri("ms-appx:///Assets/play_147x147.png"));
        }

        private void PlayPause_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            PlayPause.Source = new BitmapImage(new Uri("ms-appx:///Assets/play_clicked_147x147.png"));
        }

    }
}
