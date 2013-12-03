using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        private bool mIsSearching;
        private int mRunningThreads;
        public String Tag
        {
            get;
            set;
        }
        public TopTracksByTag(String tag)
        {
            Tag = tag;
            mIsSearching = false;
            mRunningThreads = 0;
        }

        public async Task cancelCurrentSearch()
        {
            mIsSearching = false;
            while (mRunningThreads > 0)
                await Task.Delay(10);
        }

        public async Task waitTillFinish()
        {
            while (mRunningThreads > 0)
                await Task.Delay(10);
        }

        private async Task<Track> getFromXMLNode(String XML)
        {
            if (!mIsSearching)
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

            if (!mIsSearching)
            {
                return null;
            }

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

            if (!mIsSearching)
            {
                return null;
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

            if (!mIsSearching)
            {
                return null;
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

            if (!mIsSearching)
            {
                return null;
            }

            durationNumber = Math.Max(stats.DurationInSeconds, Convert.ToInt32(duration[0].InnerText));
            return new Track(artistName, trackName, musicLink, durationNumber, imageUri, videoID, cacheUrl);

        }

        private async Task Thread(int index, FrameworkElement frameElement, GridView contentHolder , XmlNodeList tracks)
        {
            mRunningThreads++;
            for (int i = index; i < tracks.Length && mIsSearching; i += 12)
            {
                try
                {
                    String xml = tracks[i].GetXml();
                    Track compute = await getFromXMLNode(xml);
                    if (compute == null)
                    {
                        continue;
                    }
                    else if (mIsSearching)
                    {
                        
                        await frameElement.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                        {
                            contentHolder.Items.Add(compute);
                            GlobalArray.list.Add(compute);
                        });
                    }
                }
                catch (Exception error)
                {

                }
            }
            mRunningThreads--;
        }

        public async Task get(FrameworkElement frameElement, GridView contentHolder, int no)
        {
            mIsSearching = true;
            String url = "http://ws.audioscrobbler.com/2.0/?method=tag.gettoptracks&tag=" +
             Tag +
             "&limit=" +
             no +
             "&api_key=30e44ae9c1e227a2f44f410e16e56586";
            string resp;

            try
            {
                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage response = await client.GetAsync(url))
                using (HttpContent content = response.Content)
                {
                    resp = await content.ReadAsStringAsync();
                }
            }
            catch (Exception err)
            {
                throw new Exception(ExceptionMessages.CONNECTION_FAILED);
            }

            XmlDocument fullXML = new XmlDocument();
            fullXML.LoadXml(resp);
            XmlNodeList tracks = fullXML.GetElementsByTagName("track");
            mRunningThreads = 0;

            Task.Run(() => Thread(0, frameElement, contentHolder, tracks));
            Task.Run(() => Thread(1, frameElement, contentHolder, tracks));
            Task.Run(() => Thread(2, frameElement, contentHolder, tracks));
            Task.Run(() => Thread(3, frameElement, contentHolder, tracks));
            Task.Run(() => Thread(4, frameElement, contentHolder, tracks));
            Task.Run(() => Thread(5, frameElement, contentHolder, tracks));
            Task.Run(() => Thread(6, frameElement, contentHolder, tracks));
            Task.Run(() => Thread(7, frameElement, contentHolder, tracks));
            Task.Run(() => Thread(8, frameElement, contentHolder, tracks));
            Task.Run(() => Thread(9, frameElement, contentHolder, tracks));
            Task.Run(() => Thread(10, frameElement, contentHolder, tracks));
            Task.Run(() => Thread(11, frameElement, contentHolder, tracks));
        }
    }
}
