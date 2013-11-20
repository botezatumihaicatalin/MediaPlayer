using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

namespace MediaPlayer
{
    public class YoutubeStats
    {

        private string mVideoTitle;
        private string mVideoImageURL;
        private int mDurationInSeconds;
        private BitmapImage mVideoImage;

        public string VideoID
        {
            get;
            set;
        }
        public int DurationInSeconds
        {
            get{ return mDurationInSeconds; }
        }
        public string VideoTitle
        {
            get { return mVideoTitle; }
        }
        public string VideoImageURL
        {
            get { return mVideoImageURL; }
        }

        public BitmapImage VideoImage
        {
            get { return mVideoImage; }
        }

        public YoutubeStats(String VideoID)
        {
            this.VideoID = VideoID;
            mVideoTitle = "";
            mVideoImageURL = "";
            mVideoImage = null;
        }
        public YoutubeStats()
        {
        }

        public async Task getData()
        {
            const String baseUrl = "https://gdata.youtube.com/feeds/api/videos/";
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(baseUrl + VideoID + "?v=2");
            WebResponse response = null;
            try
            {
                response = await request.GetResponseAsync();
            }
            catch (Exception)
            {                
                throw new Exception("No internet connection or bad request!");                
            }
            String result = new StreamReader(response.GetResponseStream()).ReadToEnd();

            XmlDocument Xml = new XmlDocument();
            Xml.LoadXml(result);

            XmlNodeList titleList = Xml.GetElementsByTagName("title");
            
            mVideoTitle = titleList[0].InnerText;

            XmlNamedNodeMap durationAttr = Xml.GetElementsByTagName("media:content")[0].Attributes;
            mDurationInSeconds = Convert.ToInt32(durationAttr.GetNamedItem("duration").InnerText);

            XmlNodeList images = Xml.GetElementsByTagName("media:thumbnail");

            for (int i = 0; i < 4; i++)
            {
                try
                {
                    XmlNamedNodeMap attrs = images[i].Attributes;
                    if (attrs.GetNamedItem("url").InnerText.Contains("default.jpg"))
                    {
                        mVideoImageURL = attrs.GetNamedItem("url").InnerText;
                    }
                }
                catch(Exception er)
                {

                }
            }

        }



    }
}
