using MediaPlayer.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Data.Xml.Dom;
using Windows.UI.Popups;

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

        public async Task<List<Track>> get()
        {

            String url = "http://ws.audioscrobbler.com/2.0/?method=tag.gettoptracks&tag=" +
             Tag + 
             "&api_key=30e44ae9c1e227a2f44f410e16e56586";

            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);

            System.Net.WebResponse response;
            try
            {
                response = await request.GetResponseAsync();
            }
            catch
            {
                throw new Exception("No internet connection or bad request!");
            }

            String resp = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
            

            XmlDocument fullXML = new XmlDocument();
            fullXML.LoadXml(resp);            
            XmlNodeList tracks = fullXML.GetElementsByTagName("track");
            List<Track> list = new List<Track>();
            XmlDocument new_xml = new XmlDocument();

            await new MessageDialog(tracks.Length+"").ShowAsync();

            for (int i = 0; i < tracks.Length; i++)
            {
                new_xml.LoadXml(tracks[i].GetXml());
                XmlNodeList names = new_xml.GetElementsByTagName("name");
                XmlNodeList duration = new_xml.GetElementsByTagName("duration");
                XmlNodeList music_url = new_xml.GetElementsByTagName("url");
                XmlNodeList images = new_xml.GetElementsByTagName("image");
                String trackName = names[0].InnerText;
                String artistName = names[1].InnerText;
                String musicLink = music_url[0].InnerText;
                
                Uri imageUri = null;
                bool gotImageUri = false;
                String videoID = "NONE";
                Int32 durationNumber = 0;

                try
                {
                    imageUri = new Uri(images[Convert.ToInt32(images.Length - 1)].InnerText);
                    gotImageUri = true;
                }
                catch (Exception er)
                {
                    gotImageUri = false;   
                }
                YoutubeStats stats = null;

                if (!gotImageUri)
                {
                    stats = new YoutubeStats(await new LastFMPageScrapper(new Uri(musicLink)).getYoutubeId());
                    try
                    {
                        await stats.getData();
                        imageUri = new Uri(stats.VideoImageURL);
                    }
                    catch (Exception er)
                    {
                        throw er;
                    }
                }

                try
                {
                    durationNumber = Convert.ToInt32(duration[0].InnerText);
                }
                catch (Exception er)
                {
                    if (stats != null) durationNumber = stats.DurationInSeconds;
                }

                if (stats != null)
                    videoID = stats.VideoID;

                list.Add(new Track(artistName, trackName, musicLink , durationNumber , imageUri , videoID));
            }
            return list;

        }
    }
}
