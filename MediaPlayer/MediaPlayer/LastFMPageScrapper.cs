using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MediaPlayer
{
    
    class LastFMPageScrapper
    {
        public Uri LastFMUri
        {
            get;
            set;
        }
        public LastFMPageScrapper(Uri uri)
        {
            LastFMUri = uri;
        }

        public async Task<string> getYoutubeId()
        {
            string ID = "";
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(LastFMUri);
            WebResponse response;
            try
            {
                response = await request.GetResponseAsync();
            }
            catch
            {
                throw new Exception("No internet connection or bad URI");
            }

            string page_source = new StreamReader(response.GetResponseStream()).ReadToEnd();
            string search_string = "<embed src=\"http://www.youtube.com/v/";
            int index = page_source.IndexOf(search_string) + search_string.Length;
            int end = index;
            while (page_source[end] != '?')
            {
                end++;
            }
            ID = page_source.Substring(index, end - index);
            return ID;
        }
    }
}
