using MediaPlayer.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
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
        private MediaPlayer mediaPlayer;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += new RoutedEventHandler(Page_Loaded);
            MusicPlayer.AudioCategory = AudioCategory.BackgroundCapableMedia;
            mediaPlayer = new MediaPlayer(this, MusicPlayer, PlayPause, ProgressSlider);
            mediaPlayer.OnMediaFailed += MediaEnds;
            mediaPlayer.OnMediaEnded += MediaEnds;            

            MediaControl.NextTrackPressed += MediaControl_NextTrackPressed;
            MediaControl.PreviousTrackPressed += MediaControl_PreviousTrackPressed;

            list.ItemClick += Grid_ItemClick;   
            
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await new DataLayer().getTracksByPreferences(this, list);
        }

        private async void MediaControl_PreviousTrackPressed(object sender, object e)
        {
             await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => prevTrack());
        }

        private async void MediaControl_NextTrackPressed(object sender, object e)
        {
             await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => nextTrack());
        }

        private void MediaEnds(object sender , object e)
        {
            nextTrack();
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
            catch(Exception er)
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

        private async Task LoadTrack(Track track)
        {
            try
            {
                await track.getYoutubeUri();                
                MediaControl.TrackName = track.Name;
                MediaControl.ArtistName = track.Artist;
                String t = await saveImageToFile(track.ImageUri);
                MediaControl.AlbumArt = new Uri("ms-appdata:///Local/thumbnail.jpg");
            }
            catch (Exception er)
            {
                new MessageDialog("Error", er.Message).ShowAsync();
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
            try
            {
                DataLayer t = new DataLayer();
                Preferences.addTag(VideoIdTextBox.Text);
                string txt = VideoIdTextBox.Text;
                Task.Run(()=>t.getTrackByTag(this , list , txt));
            }
            catch (Exception exp)
            {
               new MessageDialog("Error",exp.Message).ShowAsync();
            }
        }

        public async void nextTrack()
        {
            mediaPlayer.stop();
            mediaPlayer.MediaIndex += 1;
            mediaPlayer.MediaIndex %= GlobalArray.list.Count;

            Track new_item = GlobalArray.list[mediaPlayer.MediaIndex];
            ToastNotifications(new_item.Artist, new_item.Name, new_item.ImageUri.AbsoluteUri);
            LiveTileOn(new_item.Artist, new_item.Name, new_item.ImageUri.AbsoluteUri);
            
            await LoadTrack(new_item);

            VideoTitleHolder.Text = new_item.Name + " - " + new_item.Artist;
            VideoImageHolder.Source = new BitmapImage(new_item.ImageUri);

            ProgressSlider.Value = 0;
            ProgressSlider.Maximum = new_item.Duration * 4.0 / 5.0;
            mediaPlayer.Source = new_item.CacheUriString;
            mediaPlayer.play();

           
        }

        public async void prevTrack()
        {
            mediaPlayer.stop();
            mediaPlayer.MediaIndex -= 1;
            if (mediaPlayer.MediaIndex < 0) mediaPlayer.MediaIndex = GlobalArray.list.Count-1;

            Track new_item = GlobalArray.list[mediaPlayer.MediaIndex];
            ToastNotifications(new_item.Artist, new_item.Name, new_item.ImageUri.AbsoluteUri);
            LiveTileOn(new_item.Artist, new_item.Name, new_item.ImageUri.AbsoluteUri);
            await LoadTrack(new_item);

            VideoTitleHolder.Text = new_item.Name;
            VideoImageHolder.Source = new BitmapImage(new_item.ImageUri);

            ProgressSlider.Value = 0;
            ProgressSlider.Maximum = new_item.Duration * 4.0 / 5.0;
            mediaPlayer.Source = new_item.CacheUriString;
            mediaPlayer.play();
            
        }

        public async void Grid_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            mediaPlayer.stop();
            Track track = ((Track)e.ClickedItem);

            mediaPlayer.MediaIndex = list.Items.IndexOf(track);

            await LoadTrack(track);

            VideoTitleHolder.Text = track.Name;
            VideoImageHolder.Source = new BitmapImage(track.ImageUri);

            ProgressSlider.Value = 0;
            ProgressSlider.Maximum = track.Duration * 4.0 / 5.0;

            mediaPlayer.Source = track.CacheUriString;
            mediaPlayer.play();

            ToastNotifications(track.Artist, track.Name, track.ImageUri.AbsoluteUri);
            LiveTileOn(track.Artist, track.Name, track.ImageUri.AbsoluteUri);
            return;
        }


        private void PlayPause_Tapped(object sender, TappedRoutedEventArgs e)
        {
            text.Text = mediaPlayer.Source;
            mediaPlayer.playPause();
        }

        private void PlayPause_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (!mediaPlayer.PlayButtonState)
            {
                PlayPause.Source = new BitmapImage(new Uri("ms-appx:///Assets/play_entered_147x147.png"));
            }
            else
            {
                PlayPause.Source = new BitmapImage(new Uri("ms-appx:///Assets/pause_entered_147x147.png"));
            }
        }

        private void PlayPause_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (!mediaPlayer.PlayButtonState)
            {
                PlayPause.Source = new BitmapImage(new Uri("ms-appx:///Assets/play_147x147.png"));
            }
            else
            {
                PlayPause.Source = new BitmapImage(new Uri("ms-appx:///Assets/pause_147x147.png"));
            }
        }

        private void PlayPause_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!mediaPlayer.PlayButtonState)
            {
                PlayPause.Source = new BitmapImage(new Uri("ms-appx:///Assets/play_clicked_147x147.png"));
            }
            else
            {
                PlayPause.Source = new BitmapImage(new Uri("ms-appx:///Assets/pause_clicked_147x147.png"));
            }
        }
     
        private void LiveTileOn(String artists, String tracks, String images)
        {
            string tileXmlString =
               "<tile>"
               + "<visual version='2'>"
               + "<binding template='TileSquare150x150PeekImageAndText02' fallback='TileSquarePeekImageAndText02' branding='None'>"
               + "<image id='1' " + "src='" + images + "' />"
               + "<text id='1'>" + artists + "</text>"
               + "<text id='2'>" + tracks + "</text>"
               + "</binding>"
               + "<binding template='TileWide310x150ImageAndText02' fallback='TileWide310x150ImageAndText02' branding='None'>"
               + "<image id='1' " + "src='" + images + "' />"
               + "<text id='1'>" + artists + "</text>"
               + "<text id='2'>" + tracks + "</text>"
               + "</binding>"
               + "</visual>"
               + "</tile>";

            // Create a DOM.
            XmlDocument tileDOM = new XmlDocument();


            // Load the xml string into the DOM, catching any invalid xml characters.
            tileDOM.LoadXml(tileXmlString);

            // Create a tile notification.

            TileNotification tile = new TileNotification(tileDOM);

            // Send the notification to the application’s tile.
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tile);

        }
        private async void ToastNotifications(string artists, string tracks, string images)
        {      
      
            string toastXmlString = "<toast>"
                            + "<visual version='2'>"
                            + "<binding template='ToastImageAndText04'>"
                            + "<image id='1' " + "src='" + images + "' />"
                            + "<text id='1'>" + artists + "</text>"
                            + "<text id='2'>" + tracks + "</text>"
                            + "</binding>"
                            + "</visual>"
                            + "</toast>";

            Windows.Data.Xml.Dom.XmlDocument toastDOM = new Windows.Data.Xml.Dom.XmlDocument();
            toastDOM.LoadXml(toastXmlString);

            // Create a toast, then create a ToastNotifier object to show
            // the toast
            try
            {
                ToastNotification toast = new ToastNotification(toastDOM);
                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
            catch (Exception er)
            {
                toastXmlString = "<toast>"
                            + "<visual version='2'>"
                            + "<binding template='ToastImageAndText04'>"
                            + "<text id='1'>" + artists + "</text>"
                            + "<text id='2'>" + tracks + "</text>"
                            + "</binding>"
                            + "</visual>"
                            + "</toast>";
                ToastNotification toast = new ToastNotification(toastDOM);
                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }

            // If you have other applications in your package, you can specify the AppId of
            // the app to create a ToastNotifier for that application
        }

        private void FeelLucky_Tapped(object sender, TappedRoutedEventArgs e)
        {
            list.Items.Clear();
            GlobalArray.list.Clear();
            new DataLayer().getTracksByPreferences(this, list);
        }

        private void FeelLucky_PointerPressed(object sender, PointerRoutedEventArgs e)
        {

        }

        private void FeelLucky_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
        }

        private void FeelLucky_PointerExited(object sender, PointerRoutedEventArgs e)
        {
        }

        private void Next_track_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Next_track.Source  = new BitmapImage(new Uri("ms-appx:///Assets/next_track__entered_147x147.png"));
        }

        private void Next_track_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Next_track.Source = new BitmapImage(new Uri("ms-appx:///Assets/next_track_147x147.png"));
        }

        private void Next_track_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Next_track.Source = new BitmapImage(new Uri("ms-appx:///Assets/next_track_clicked_147x147.png"));
        }     

        private void Prev_track_Tapped(object sender, TappedRoutedEventArgs e)
        {
            prevTrack();
        }

        private void Prev_track_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Prev_track.Source = new BitmapImage(new Uri("ms-appx:///Assets/prev_track_entered_147x147.png"));
        }

        private void Prev_track_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Prev_track.Source = new BitmapImage(new Uri("ms-appx:///Assets/prev_track_147x147.png"));
        }

        private void Prev_track_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Prev_track.Source = new BitmapImage(new Uri("ms-appx:///Assets/prev_track_clicked_147x147.png"));
        }       

        private void Next_track_Tapped(object sender, TappedRoutedEventArgs e)
        {
            nextTrack();
        }




        

    }
}
