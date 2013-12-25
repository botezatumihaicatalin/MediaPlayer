using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        private HttpClient mClient;
        private HttpResponseMessage mResponse;

        public YoutubeStats(String VideoID)
        {
            this.VideoID = VideoID;
            mVideoTitle = "";
            mVideoImageURL = "";
            mVideoImage = null;
            mDurationInSeconds = 0;
            mClient = new HttpClient();
            mClient.MaxResponseContentBufferSize = 10240;
            mClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
        }
        public YoutubeStats()
        {
            mVideoTitle = "";
            mVideoImageURL = "";
            mVideoImage = null;
            mDurationInSeconds = 0;
            mClient = new HttpClient();
            mClient.MaxResponseContentBufferSize = 10240;
            mClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
        }

        public void Cancel()
        {
            mClient.CancelPendingRequests();
        }
        public async Task GetData()
        {
            mClient.CancelPendingRequests();
            const String baseUrl = "https://gdata.youtube.com/feeds/api/videos/";

            String result = "";
            try
            {

                mResponse = await mClient.GetAsync(baseUrl + VideoID + "?v=2",HttpCompletionOption.ResponseHeadersRead);         
                result = await mResponse.Content.ReadAsStringAsync();                
            }
            catch (Exception error)
            {
                throw new Exception(ExceptionMessages.CONNECTION_FAILED);
            }

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
