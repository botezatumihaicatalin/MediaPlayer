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
            WebResponse response = null;
            try
            {
                response = await request.GetResponseAsync();
            }
            catch (Exception er)
            {
                throw new Exception(ExceptionMessages.CONNECTION_FAILED);
            }
            string page_source = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
            string search_string = "<embed src=\"http://www.youtube.com/v/";
            
            int index = page_source.IndexOf(search_string);

            if (index == -1)
            {
                throw new Exception(ExceptionMessages.YOUTUBE_VIDEO_ID_NOT_FOUND); 
            }
            index += search_string.Length;
            int end = index;
            while (page_source[end] != '?' && page_source[end] != '&' && page_source[end] != '"')
            {
                end++;
            }
            ID = page_source.Substring(index, end - index);
            //if (ID.Length <= 3)
            //{
                //throw new Exception(ExceptionMessages.YOUTUBE_VIDEO_ID_NOT_FOUND);
            //}
            return ID;
        }
    }
}
