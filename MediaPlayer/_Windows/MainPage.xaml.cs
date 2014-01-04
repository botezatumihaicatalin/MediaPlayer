using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
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
        private DataLayer searchLayer;
        private int lastTrackIndex = -1;
        public MainPage()
        { 
            this.InitializeComponent();
            searchLayer = new DataLayer();
            this.Loaded += OnLoaded;
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            FeelLucky_Tapped(FeelLucky, null);
            Window.Current.VisibilityChanged += Current_VisibilityChanged;
        }

        private async void Current_VisibilityChanged(object sender, Windows.UI.Core.VisibilityChangedEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                MediaPlayer.Volume = 1.0;
            });
        }


        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Initialize(PlayPause, ProgressSlider , VideoImageHolder, VideoTitleHolder);
            MediaPlayer.OnMediaFailed += MediaEnds;
            MediaPlayer.OnMediaEnded += MediaEnds;
            MediaPlayer.MediaIndex = lastTrackIndex;

            MediaControl.NextTrackPressed += MediaControl_NextTrackPressed;
            MediaControl.PreviousTrackPressed += MediaControl_PreviousTrackPressed;
            MediaControl.PlayPressed += MediaControl_PlayPressed;
            MediaControl.PausePressed += MediaControl_PausePressed;
            MediaControl.PlayPauseTogglePressed += MediaControl_PlayPauseTogglePressed;

            list.ItemClick += Grid_ItemClick;
        }

        private async void MediaControl_PlayPauseTogglePressed(object sender, object e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                PlayPause_Tapped(PlayPause,null);
            });
        }
        private async void MediaControl_PausePressed(object sender, object e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => PlayPause_Tapped(PlayPause,null));
        }

        private async void MediaControl_PlayPressed(object sender, object e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => PlayPause_Tapped(PlayPause,null));
        }

        private async void MediaControl_PreviousTrackPressed(object sender, object e)
        {
             await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => prevTrack());
        }

        private async void MediaControl_NextTrackPressed(object sender, object e)
        {
             await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => nextTrack());
        }

        private void MediaEnds(object e)
        {
            nextTrack();
        }
        private async Task LoadTrack(Track track)
        {
            try
            {
                VideoTitleHolder.Text = track.Name + " - " + track.Artist;
                VideoImageHolder.Source = new BitmapImage(track.ImageUri);
                MediaPlayer.CurrentTrack = track;
                MediaPlayer.Play();
            }
            catch (Exception er)
            {
                new MessageDialog("Error", er.Message).ShowAsync();
            }

        }
        public async void nextTrack()
        {
            if (GlobalArray.list.Count > 0)
            {
                MediaPlayer.Stop();
                MediaPlayer.MediaIndex += 1;
                MediaPlayer.MediaIndex %= GlobalArray.list.Count;
                Track new_item = GlobalArray.list[MediaPlayer.MediaIndex];
                LoadTrack(new_item);
            }
        }
     
        public async void prevTrack()
        {
            if (GlobalArray.list.Count > 0)
            {
                MediaPlayer.Stop();
                MediaPlayer.MediaIndex -= 1;
                if (MediaPlayer.MediaIndex < 0) MediaPlayer.MediaIndex = GlobalArray.list.Count - 1;

                Track new_item = GlobalArray.list[MediaPlayer.MediaIndex];
                LoadTrack(new_item);
            }
        }

        public async void Grid_ItemClick(object sender, ItemClickEventArgs e)
        {
            Track new_item = ((Track)e.ClickedItem);
            if (MediaPlayer.MediaIndex == list.Items.IndexOf(new_item))
            {
               return;
            }

            MediaPlayer.Stop();
            MediaPlayer.MediaIndex = list.Items.IndexOf(new_item);
            await LoadTrack(new_item);            
        }


        private void PlayPause_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (MediaPlayer.CurrentTrack == null)
            {
                if (GlobalArray.list.Count > 0)
                {
                    LoadTrack(GlobalArray.list[0]);
                    MediaPlayer.MediaIndex = 0;
                }
            }
            else
            {
                MediaPlayer.PlayPause();
            }
        }

        private async void FeelLucky_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                list.Items.Clear();
                GlobalArray.list.Clear();
                MediaPlayer.MediaIndex = -1;
                SearchBox1.IsEnabled = false;
                FeelLucky.IsEnabled = false;
                searchLayer.CancelSearch();
                searchLayer = new DataLayer();
                Task.Run(()=>searchLayer.GetTracksByPreferences(progressTracker , list));
                SearchBox1.IsEnabled = true;
                FeelLucky.IsEnabled = true;
            }
            catch (Exception error)
            {
                new MessageDialog(error.Message,"Error").ShowAsync();
            }
        }
        private void Prev_track_Tapped(object sender, TappedRoutedEventArgs e)
        {
            prevTrack();
        }      

        private void Next_track_Tapped(object sender, TappedRoutedEventArgs e)
        {
            nextTrack();
        }

        

        private async void SearchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            if (SettingsFlyout1.Queue == false)
            {
                list.Items.Clear();
                GlobalArray.list.Clear();
                MediaPlayer.MediaIndex = -1;
            }
            
            try
            {
                SearchBox1.IsEnabled = false;
                FeelLucky.IsEnabled = false;
                searchLayer.CancelSearch();
                searchLayer = new DataLayer();
                string txt = args.QueryText;
                Task.Run(() => searchLayer.GetTrackByTag(progressTracker , list, txt));
                SearchBox1.IsEnabled = true;
                FeelLucky.IsEnabled = true;
            }
            catch (Exception exp)
            {
                new MessageDialog(exp.Message, "Error").ShowAsync();
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b != null)
            {
                SettingsFlyout1 sf = new SettingsFlyout1();
                sf.ShowIndependent();
            }
            
        }

        private void Playlist_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.OnMediaFailed -= MediaEnds;
            MediaPlayer.OnMediaEnded -= MediaEnds;
            lastTrackIndex = MediaPlayer.MediaIndex;
            App.RootFrame.Navigate(typeof(PlaylistPage));
        }


        private void SearchBox1_Loaded(object sender, RoutedEventArgs e)
        {
            SearchBox1.SearchHistoryEnabled = SettingsFlyout1.History;
        }
      
        private async void AddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            Button sender_button = (Button)sender;
            sender_button.IsEnabled = false;
            int length = list.SelectedItems.Count;
            for (int i = 0; i < length; i++)
                await PlayList.AddToPlayList((Track)list.SelectedItems[i]);
            list.SelectedIndex = -1;
            sender_button.IsEnabled = true;
        }      

    }
}
