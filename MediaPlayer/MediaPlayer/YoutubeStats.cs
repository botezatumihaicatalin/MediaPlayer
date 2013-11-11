using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

namespace MediaPlayer
{
    public class YoutubeStats
    {

        private string mVideoTitle;
        private string mVideoImageURL;
        private BitmapImage mVideoImage;

        public string VideoID
        {
            get;
            set;
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
            catch (Exception error)
            {
                new MessageDialog(error.Message).ShowAsync();
                return;
            }
            String result = new StreamReader(response.GetResponseStream()).ReadToEnd();

            int titleStartIndex = result.IndexOf("<title>") + "<title>".Length;
            int titleEndIndex = result.IndexOf("</title>");
            mVideoTitle = result.Substring(titleStartIndex, titleEndIndex - titleStartIndex);

            string startString = "<media:thumbnail url='";
            int imageStartIndex = result.IndexOf(startString) + startString.Length;
            int imageEndIndex = result.IndexOf("default.jpg") + "default.jpg".Length;

            mVideoImageURL = result.Substring(imageStartIndex, imageEndIndex - imageStartIndex);
            mVideoImage = new BitmapImage(new Uri(mVideoImageURL));
        }



    }
}
