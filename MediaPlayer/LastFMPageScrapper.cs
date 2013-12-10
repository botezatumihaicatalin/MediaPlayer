using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        private HttpClient mClient;
        private HttpResponseMessage mResponse;
        public LastFMPageScrapper(Uri uri)
        {
            LastFMUri = uri;
            mClient = new HttpClient();
        }

        public LastFMPageScrapper()
        {
            LastFMUri = new Uri("http://127.0.0.1");
            mClient = new HttpClient();
        }

        public void cancel()
        {
            mClient.CancelPendingRequests();
        }
        public async Task<string> getYoutubeId()
        {
            mClient.CancelPendingRequests();
            string ID = "";
            string page_source = "";
            try
            {                
                mResponse = await mClient.GetAsync(LastFMUri);               
                page_source = await mResponse.Content.ReadAsStringAsync();                
            }
            catch (Exception error)
            {
                throw new Exception(ExceptionMessages.CONNECTION_FAILED);
            }
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

            if (ID.Length <= 3)
            {
                throw new Exception(ExceptionMessages.YOUTUBE_VIDEO_ID_NOT_FOUND);
            }

            return ID;
        }
    }
}
