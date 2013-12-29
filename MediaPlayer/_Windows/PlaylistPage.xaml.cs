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
using Windows.UI.Popups;
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
        private int lastTrackIndex = -1;
        public PlaylistPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Initialize(PlayPause, ProgressSlider, VideoImageHolder, VideoTitleHolder);
            MediaPlayer.OnMediaFailed += MediaEnds;
            MediaPlayer.OnMediaEnded += MediaEnds;
            MediaPlayer.MediaIndex = lastTrackIndex;

            MediaControl.NextTrackPressed += MediaControl_NextTrackPressed;
            MediaControl.PreviousTrackPressed += MediaControl_PreviousTrackPressed;
            MediaControl.PlayPressed += MediaControl_PlayPressed;
            MediaControl.PausePressed += MediaControl_PausePressed;
            MediaControl.PlayPauseTogglePressed += MediaControl_PlayPauseTogglePressed;

            list.ItemClick += Grid_ItemClick;
            list.Items.Clear();
            Task.Run(() => PlayList.ReadPlayList(list));
        }

        private async void MediaControl_PlayPauseTogglePressed(object sender, object e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                PlayPause_Tapped(PlayPause, null);
            });
        }
        private async void MediaControl_PausePressed(object sender, object e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => PlayPause_Tapped(PlayPause, null));
        }

        private async void MediaControl_PlayPressed(object sender, object e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => PlayPause_Tapped(PlayPause, null));
        }

        private async void MediaControl_PreviousTrackPressed(object sender, object e)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => prevTrack());
        }

        private async void MediaControl_NextTrackPressed(object sender, object e)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => nextTrack());
        }

        private void MediaEnds(EventArgs e)
        {
            nextTrack();
        }

        private async Task LoadTrack(Track track)
        {
            YoutubeDecoder decoder = new YoutubeDecoder();
            decoder.VideoID = track.VideoID;

            try
            {
                track.CacheUriString = await decoder.FetchURL();
            }
            catch (Exception error)
            {                
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
                {
                    new MessageDialog("Unable to load track!", "Error").ShowAsync();
                });
                return;
            }

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
                MediaPlayer.CurrentTrack = track;
                MediaPlayer.Play();
                VideoImageHolder.Source = new BitmapImage(track.ImageUri);
                VideoTitleHolder.Text = track.Name + " - " + track.Artist;
            });
        }

        private void Grid_ItemClick(object sender, ItemClickEventArgs e)
        {
            Track new_item = ((Track)e.ClickedItem);
            MediaPlayer.MediaIndex = PlayList.getIndex(new_item);
            MediaPlayer.Stop();
            Task.Run(() => LoadTrack(new_item));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }
        private async void PlayPause_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (MediaPlayer.CurrentTrack == null)
            {
                if (PlayList.getSize() > 0)
                {
                    MediaPlayer.MediaIndex = 0;
                    await Task.Run(() => LoadTrack(PlayList.getElement(0)));
                    
                }
            }
            else
            { 
                MediaPlayer.PlayPause();
            }
        }

        public async void nextTrack()
        {
            if (PlayList.getSize() > 0)
            {
                MediaPlayer.Stop();
                MediaPlayer.MediaIndex += 1;
                MediaPlayer.MediaIndex %= PlayList.getSize();
                Task.Run(() => LoadTrack(PlayList.getElement(MediaPlayer.MediaIndex)));
            }
        }

        public async void prevTrack()
        {
            if (PlayList.getSize() > 0)
            {
                MediaPlayer.Stop();
                MediaPlayer.MediaIndex -= 1;
                if (MediaPlayer.MediaIndex < 0) MediaPlayer.MediaIndex = PlayList.getSize() - 1;
                Task.Run(() => LoadTrack(PlayList.getElement(MediaPlayer.MediaIndex)));
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

        private void BackButtonClick(object sender, RoutedEventArgs e)
        {
            MediaPlayer.OnMediaFailed -= MediaEnds;
            MediaPlayer.OnMediaEnded -= MediaEnds;
            lastTrackIndex = MediaPlayer.MediaIndex;
            App.RootFrame.GoBack();
        }

        private  async void RemoveButtonClick(object sender, RoutedEventArgs e)
        {
            AppBarButton sender_button = (AppBarButton)sender;
            sender_button.IsEnabled = false;
            int length = list.SelectedItems.Count;
            for (int i = 0; i < length; i++)
            {
                Track track_to_delete = (Track)list.SelectedItems[list.SelectedItems.Count - 1];
                await Task.Run(() => PlayList.RemoveFromPlayList(track_to_delete, list));
            }
            list.SelectedIndex = -1;
            if (MediaPlayer.MediaIndex > list.Items.Count - 1)
            {
                MediaPlayer.MediaIndex = list.Items.Count - 1;
            }
            sender_button.IsEnabled = true;
        }

        private void AddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            lastTrackIndex = MediaPlayer.MediaIndex;
            App.RootFrame.GoBack();
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

        private async void SearchBox1_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            list.Items.Clear();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => PlayList.filterPlayList(args.QueryText, list));
        }

        private void SearchBox1_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
