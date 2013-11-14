using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace MediaPlayer
{
    class YoutubeDecoder
    {
        public string VideoID
        {
            get;
            set;
        }

        private string mDirectVideoURL;
        public string DirectVideoURL
        {
            get { return mDirectVideoURL; }
        }

        public YoutubeDecoder()
        {           
        }
        public YoutubeDecoder(string VideoID)
        {
            this.VideoID = VideoID;
        }

        private string decodeURL(ref string content)
        {
            Dictionary<string, string> chars = new Dictionary<string, string>(){
            {"24","$"},{"26","&"},{"2B","+"},{"2C",","},{"2F","/"},
            {"3A",":"},{"3B",";"},{"3D","="},{"3F","?"},{"40","@"},
            {"20"," "},{"3C","<"},{"3E",">"},{"23","#"},{"7B","{"},
            {"7D","}"},{"7C","|"},{"5C","\\"},{"5E","^"},{"7E","~"},
            {"5B","["},{"5D","]"},{"60","`"},{"25","%"}};
            string new_content = content;
            foreach (string key in chars.Keys)
                new_content = new_content.Replace("%" + key, chars[key]);

            new_content = new_content.Replace("%2C", ",");
            return new_content;
        }

        private async Task<string> fetchURL()
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://www.youtube.com/watch?v=" + VideoID);
            WebResponse response;
            try
            {
                response = await request.GetResponseAsync();
            }
            catch
            {
                throw new Exception("No internet connection or bad request!"); 
            }
            string result = new StreamReader(response.GetResponseStream()).ReadToEnd();

            string startSearchString = "adaptive_fmts";
            int startIndex = result.IndexOf(startSearchString) + startSearchString.Length;
        
            if (startIndex == -1)
            {
                throw new Exception("Failed to find URL");
            }

            result = result.Substring(startIndex, result.Length - startIndex);
            result = decodeURL(ref result);

            int audioIndex = result.IndexOf("type=audio") + "type=audio".Length;

            if (audioIndex == -1)
            {
                throw new Exception("Failed to find URL");
            }
            result = result.Substring(audioIndex, result.Length - audioIndex);

            int urlIndex = result.IndexOf("url=") + "url=".Length;

            if (urlIndex == -1)
            {
                throw new Exception("Failed to find URL");
            }

            result = result.Substring(urlIndex, result.Length - urlIndex);

            int finalIndex = result.IndexOf("\\u0026");
            result = result.Substring(0, finalIndex);
            return result;
        }

        public async Task getVideoCacheURL()
        {
            bool is_good = false;
            do
            {
                try
                {
                    mDirectVideoURL = await fetchURL();
                    is_good = true;
                }
                catch (Exception er)
                {

                }
                
            }while (!is_good);
        }
    }
}
