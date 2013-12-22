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
            mClient.MaxResponseContentBufferSize = 65536;
            mClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
        }

        public LastFMPageScrapper()
        {
            LastFMUri = new Uri("http://127.0.0.1");
            mClient = new HttpClient();
            mClient.MaxResponseContentBufferSize = 65536;
            mClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
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
                mResponse = await mClient.GetAsync(LastFMUri,HttpCompletionOption.ResponseHeadersRead);               
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
