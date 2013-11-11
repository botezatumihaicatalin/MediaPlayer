using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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
        private DispatcherTimer timer;

        public MainPage()
        {
            this.InitializeComponent();
            stats = new YoutubeStats();
            decoder = new YoutubeDecoder();
        }

        private async void PopulateUI(string VideoID)
        {
            stats.VideoID = VideoID;
            decoder.VideoID = VideoID;
            await stats.getData();
            VideoImageHolder.Source = stats.VideoImage;
            VideoTitleHolder.Text = stats.VideoTitle;
            await decoder.getVideoCacheURL();
            
            ProgressSlider.Maximum = stats.DurationInSeconds * 4.0 / 5.0;
            ProgressSlider.Value = 0;

            MusicPlayer.Source = new Uri(decoder.DirectVideoURL);
            MusicPlayer.Play();

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void Set_Click(object sender, RoutedEventArgs e)
        {
            PopulateUI(VideoIdTextBox.Text);
        }

        private void MusicPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            timer = new DispatcherTimer();
            timer.Tick += Tick;
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Start();
        }

        private void Tick(object sender, object e)
        {
            ProgressSlider.Value+= 0.1;
        }

        private void MusicPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            ProgressSlider.Value = ProgressSlider.Maximum;
            timer.Stop();
        } 
    }
}
