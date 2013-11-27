using MediaPlayer.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Data.Xml.Dom;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MediaPlayer
{
    class TopTrackByTag
    {
        public String Tag
        {
            get;
            set;
        }
        public TopTrackByTag(String tag)
        {
            Tag = tag;
        }

        private async Task getFromXMLNode(String XML, FrameworkElement frameElement, GridView contentHolder)
        {
            XmlDocument new_xml = new XmlDocument();
            new_xml.LoadXml(XML);
            XmlNodeList names = new_xml.GetElementsByTagName("name");
            XmlNodeList duration = new_xml.GetElementsByTagName("duration");
            XmlNodeList music_url = new_xml.GetElementsByTagName("url");
            XmlNodeList images = new_xml.GetElementsByTagName("image");

            String trackName = names[0].InnerText;
            String artistName = names[1].InnerText;
            String musicLink = music_url[0].InnerText;

            Uri imageUri = new Uri("ms-appx:///Assets/blue.png");
            
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
            catch(Exception error)
            {

            }
            
            // If videoID is NONE at this point it means either no video is on the LastFM page or the video cannot be played , so we will search Youtube
            if (videoID == "NONE")
            {
                YoutubeSearch src = new YoutubeSearch(trackName, artistName);
                Pair<string, string> pair = await src.getAVideoCacheUri();

                videoID = pair.second;
                cacheUrl = pair.first;
            }

            if (videoID == "NONE")
                return;

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
                    imageUri = new Uri("ms-appx:///Assets/blue.png");
                }
            }

            durationNumber = Math.Max(stats.DurationInSeconds, Convert.ToInt32(duration[0].InnerText));
            frameElement.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                 artistName = artistName.Replace("&", "and");
                 trackName = trackName.Replace("&", "and");
                 Track new_track = new Track(artistName, trackName, musicLink, durationNumber, imageUri, videoID, cacheUrl);
                 contentHolder.Items.Add(new_track);
                 GlobalArray.list.Add(new_track);
            });
        }

        public async Task get(FrameworkElement frameElement, GridView contentHolder, int no = 50)
        {
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


            for (int i = 0; i < tracks.Length; i++)
            {
                String xml = tracks[i].GetXml();
                Task.Run(() => getFromXMLNode(xml ,frameElement, contentHolder));
                //await getFromXMLNode(xml, frameElement, contentHolder);
            }

        }
    }
}
