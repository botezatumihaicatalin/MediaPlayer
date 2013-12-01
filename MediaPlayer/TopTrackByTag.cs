using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Data.Xml.Dom;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MediaPlayer
{
    class TopTracksByTag
    {
        private CancellationTokenSource mTokenSource = new CancellationTokenSource();
        public String Tag
        {
            get;
            set;
        }
        public TopTracksByTag(String tag)
        {
            Tag = tag;
            mTokenSource = new CancellationTokenSource();
        }

        public void cancelCurrentSearch()
        {
            mTokenSource.Cancel();
        }

        private async Task<Track> getFromXMLNode(String XML)
        {
            if (mTokenSource.IsCancellationRequested)
            {
                return null;
            }
            XmlDocument new_xml = new XmlDocument();
            new_xml.LoadXml(XML);
            XmlNodeList names = new_xml.GetElementsByTagName("name");
            XmlNodeList duration = new_xml.GetElementsByTagName("duration");
            XmlNodeList music_url = new_xml.GetElementsByTagName("url");
            XmlNodeList images = new_xml.GetElementsByTagName("image");

            String trackName = names[0].InnerText;
            String artistName = names[1].InnerText;
            String musicLink = music_url[0].InnerText;

            Uri imageUri = new Uri("ms-appx:///Assets/default.jpg");

            String videoID = "NONE";
            String cacheUrl = "";
            Int32 durationNumber = 0;

            // Check if last fm page has an youtube link
            try
            {
                LastFMPageScrapper scpr = new LastFMPageScrapper(new Uri(musicLink));
                videoID = await scpr.getYoutubeId();
                // Check if it contains signature , if not then the video it isn't good
                if (!videoID.Contains("&signature="))
                {
                    videoID = "NONE";
                }
            }
            catch (Exception error)
            {
                if (error.Message == ExceptionMessages.CONNECTION_FAILED)
                {
                    return null;
                }
            }

            // If videoID is "NONE" at this point it means either no video is on the LastFM page or the video cannot be played , so we will search Youtube
            if (videoID == "NONE")
            {
                try
                {
                    YoutubeSearch src = new YoutubeSearch(trackName, artistName);
                    Pair<string, string> pair = await src.getAVideoCacheUri();

                    videoID = pair.second;
                    cacheUrl = pair.first;
                }
                catch(Exception error)
                {
                    if (error.Message == ExceptionMessages.CONNECTION_FAILED)
                        return null;
                }
            }

            // If videoID is "NONE" at this point then the track doesn't have a video attached , so we dont show it.
            if (videoID == "NONE")
                return null;

            YoutubeStats stats = new YoutubeStats(videoID);
            var length = Convert.ToInt32(images.Length);
            if (length > 0)
            {
                imageUri = new Uri(images[length - 1].InnerText);
            }
            else
            {
                try
                {
                    await stats.getData();
                    imageUri = new Uri(stats.VideoImageURL);
                }
                catch (Exception er)
                {
                    imageUri = new Uri("ms-appx:///Assets/default.jpg");
                }
            }

            if (mTokenSource.IsCancellationRequested)
            {
                return null;
            }

            durationNumber = Math.Max(stats.DurationInSeconds, Convert.ToInt32(duration[0].InnerText));
            return new Track(artistName, trackName, musicLink, durationNumber, imageUri, videoID, cacheUrl);

        }

        private async Task Thread(int index, FrameworkElement frameElement, GridView contentHolder , XmlNodeList tracks)
        {
            for (int i = index; i < tracks.Length && !mTokenSource.IsCancellationRequested; i += 7)
            {
                try
                {

                    String xml = tracks[i].GetXml();
                    Track compute = await Task.Run(() => getFromXMLNode(xml), mTokenSource.Token);
                    if (compute == null)
                        continue;
                    frameElement.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        lock (contentHolder)
                        {
                            contentHolder.Items.Add(compute);
                            GlobalArray.list.Add(compute);
                        }
                    });
                }
                catch (Exception error)
                {

                }
            }
        }

        public async void get(FrameworkElement frameElement, GridView contentHolder, int no)
        {
            mTokenSource = new CancellationTokenSource();
            String url = "http://ws.audioscrobbler.com/2.0/?method=tag.gettoptracks&tag=" +
             Tag +
             "&limit=" +
             no +
             "&api_key=30e44ae9c1e227a2f44f410e16e56586";
            String urlEncoded = Uri.EscapeUriString(url);

            WebRequest request = WebRequest.Create(urlEncoded);
            WebResponse response;
            try
            {
                response = await request.GetResponseAsync();
            }
            catch
            {
                throw new Exception(ExceptionMessages.CONNECTION_FAILED);
            }
            String resp = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
            XmlDocument fullXML = new XmlDocument();
            fullXML.LoadXml(resp);
            XmlNodeList tracks = fullXML.GetElementsByTagName("track");

            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            // 7 THREADS =)))
            Task.Run(() => Thread(0, frameElement, contentHolder, tracks));
            Task.Run(() => Thread(1, frameElement, contentHolder, tracks));
            Task.Run(() => Thread(2, frameElement, contentHolder, tracks));
            Task.Run(() => Thread(3, frameElement, contentHolder, tracks));
            Task.Run(() => Thread(4, frameElement, contentHolder, tracks));
            Task.Run(() => Thread(5, frameElement, contentHolder, tracks));   
            Task.Run(() => Thread(6, frameElement, contentHolder, tracks));
        }
    }
}
